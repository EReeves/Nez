using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.IEnumerableExtensions;
using Nez.UI;

#if DEBUG
namespace Nez
{
	/// <summary>
	///     the heart of the inspector system. Subclasses of Inspector are responsible for setting up and managing the UI.
	///     Currently,
	///     custom type handling is not yet implemented.
	/// </summary>
	public abstract class Inspector
    {
        protected Func<object> Getter;
        protected MemberInfo MemberInfo;
        protected string Name;
        protected Action<object> Setter;
        protected object Target;
        protected Type ValueType;


        public static List<Inspector> GetInspectableProperties(object target)
        {
            var props = new List<Inspector>();
            var targetType = target.GetType();

            var fields = ReflectionUtils.GetFields(targetType);
            foreach (var field in fields)
            {
                if (!field.IsPublic && field.GetCustomAttributes<InspectableAttribute>().Count() == 0)
                    continue;

                if (field.IsInitOnly)
                    continue;

                // skip enabled which is handled elsewhere
                if (field.Name == "enabled")
                    continue;

                var inspector = GetInspectorForType(field.FieldType, target, field);
                if (inspector != null)
                {
                    inspector.SetTarget(target, field);
                    props.Add(inspector);
                }
            }

            var properties = ReflectionUtils.GetProperties(targetType);
            foreach (var prop in properties)
            {
                if (!prop.CanRead || !prop.CanWrite)
                    continue;

                if ((!prop.GetMethod.IsPublic || !prop.SetMethod.IsPublic) &&
                    prop.GetCustomAttributes<InspectableAttribute>().Count() == 0)
                    continue;

                // skip Component.enabled which is handled elsewhere
                if (prop.Name == "enabled")
                    continue;

                var inspector = GetInspectorForType(prop.PropertyType, target, prop);
                if (inspector != null)
                {
                    inspector.SetTarget(target, prop);
                    props.Add(inspector);
                }
            }

            var methods = ReflectionUtils.GetMethods(targetType);
            foreach (var method in methods)
            {
                var attr = method.GetCustomAttribute<InspectorCallableAttribute>();
                if (attr == null)
                    continue;

                if (!MethodInspector.AreParametersValid(method.GetParameters()))
                    continue;

                var inspector = new MethodInspector();
                inspector.SetTarget(target, method);
                props.Add(inspector);
            }

            return props;
        }


        public static List<Inspector> GetTransformProperties(object transform)
        {
            var props = new List<Inspector>();
            var type = transform.GetType();

            var allowedProps = new[] {"localPosition", "localRotationDegrees", "localScale"};
            var properties = ReflectionUtils.GetProperties(type);
            foreach (var prop in properties)
            {
                if (!allowedProps.Contains(prop.Name))
                    continue;

                var inspector = GetInspectorForType(prop.PropertyType, transform, prop);
                inspector.SetTarget(transform, prop);
                props.Add(inspector);
            }

            return props;
        }


	    /// <summary>
	    ///     gets an Inspector subclass that can handle valueType. If no default Inspector is available the memberInfo custom
	    ///     attributes
	    ///     will be checked for the CustomInspectorAttribute.
	    /// </summary>
	    /// <returns>The inspector for type.</returns>
	    /// <param name="valueType">Value type.</param>
	    /// <param name="memberInfo">Member info.</param>
	    protected static Inspector GetInspectorForType(Type valueType, object target, MemberInfo memberInfo)
        {
            // built-in types
            if (valueType == typeof(int))
                return new IntInspector();
            if (valueType == typeof(float))
                return new FloatInspector();
            if (valueType == typeof(bool))
                return new BoolInspector();
            if (valueType == typeof(string))
                return new StringInspector();
            if (valueType == typeof(Vector2))
                return new Vector2Inspector();
            if (valueType == typeof(Color))
                return new ColorInspector();
            if (valueType.GetTypeInfo().IsEnum)
                return new EnumInspector();
            if (valueType.GetTypeInfo().IsValueType)
                return new StructInspector();

            // check for custom inspectors before checking Nez types in case a subclass implemented one
            var customInspectorType = valueType.GetTypeInfo().GetCustomAttribute<CustomInspectorAttribute>();
            if (customInspectorType != null)
            {
                if (customInspectorType.InspectorType.GetTypeInfo().IsSubclassOf(typeof(Inspector)))
                    return (Inspector) Activator.CreateInstance(customInspectorType.InspectorType);
                Debug.Warn(
                    $"found CustomInspector {customInspectorType.InspectorType} but it is not a subclass of Inspector");
            }

            // Nez types
            if (valueType == typeof(Material))
                return GetMaterialInspector(target);
            if (valueType.GetTypeInfo().IsSubclassOf(typeof(Effect)))
                return GetEffectInspector(target, memberInfo);

            //Debug.log( $"no inspector for type {valueType}" );

            return null;
        }


	    /// <summary>
	    ///     null checks the Material and Material.effect and ony returns an Inspector if we have data
	    /// </summary>
	    /// <returns>The material inspector.</returns>
	    /// <param name="target">Target.</param>
	    private static Inspector GetMaterialInspector(object target)
        {
            var materialProp = ReflectionUtils.GetPropertyInfo(target, "material");
            var materialMethod = ReflectionUtils.GetPropertyGetter(materialProp);
            var material = materialMethod.Invoke(target, new object[] { }) as Material;
            if (material == null || material.Effect == null)
                return null;

            // we only want subclasses of Effect. Effect itself is not interesting
            if (material.Effect.GetType().GetTypeInfo().IsSubclassOf(typeof(Effect)))
                return new EffectInspector();

            return null;
        }


	    /// <summary>
	    ///     null checks the Effect and creates an Inspector only if it is not null
	    /// </summary>
	    /// <returns>The effect inspector.</returns>
	    /// <param name="target">Target.</param>
	    /// <param name="memberInfo">Member info.</param>
	    private static Inspector GetEffectInspector(object target, MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
                if (fieldInfo.GetValue(target) != null)
                    return new EffectInspector();

            var propInfo = memberInfo as PropertyInfo;
            if (propInfo != null)
            {
                var getter = ReflectionUtils.GetPropertyGetter(propInfo);
                if (getter.Invoke(target, new object[] { }) != null)
                    return new EffectInspector();
            }

            return null;
        }


        public void SetTarget(object target, FieldInfo field)
        {
            Target = target;
            MemberInfo = field;
            Name = field.Name;
            ValueType = field.FieldType;

            Getter = () => { return field.GetValue(target); };
            Setter = val => { field.SetValue(target, val); };
        }


	    /// <summary>
	    ///     this version will first fetch the struct before getting/setting values on it when invoking the getter/setter
	    /// </summary>
	    /// <returns>The struct target.</returns>
	    /// <param name="target">Target.</param>
	    /// <param name="structName">Struct name.</param>
	    /// <param name="field">Field.</param>
	    public void SetStructTarget(object target, Inspector parentInspector, FieldInfo field)
        {
            Target = target;
            MemberInfo = field;
            Name = field.Name;
            ValueType = field.FieldType;

            Getter = () =>
            {
                var structValue = parentInspector.GetValue();
                return field.GetValue(structValue);
            };
            Setter = val =>
            {
                var structValue = parentInspector.GetValue();
                field.SetValue(structValue, val);
                parentInspector.SetValue(structValue);
            };
        }


        public void SetTarget(object target, PropertyInfo prop)
        {
            MemberInfo = prop;
            Target = target;
            Name = prop.Name;
            ValueType = prop.PropertyType;

            Getter = () => { return ReflectionUtils.GetPropertyGetter(prop).Invoke(target, null); };
            Setter = val => { ReflectionUtils.GetPropertySetter(prop).Invoke(target, new[] {val}); };
        }


	    /// <summary>
	    ///     this version will first fetch the struct before getting/setting values on it when invoking the getter/setter
	    /// </summary>
	    /// <returns>The struct target.</returns>
	    /// <param name="target">Target.</param>
	    /// <param name="structName">Struct name.</param>
	    /// <param name="field">Field.</param>
	    public void SetStructTarget(object target, Inspector parentInspector, PropertyInfo prop)
        {
            Target = target;
            MemberInfo = prop;
            Name = prop.Name;
            ValueType = prop.PropertyType;

            Getter = () =>
            {
                var structValue = parentInspector.GetValue();
                return ReflectionUtils.GetPropertyGetter(prop).Invoke(structValue, null);
            };
            Setter = val =>
            {
                var structValue = parentInspector.GetValue();
                prop.SetValue(structValue, val);
                parentInspector.SetValue(structValue);
            };
        }


        public void SetTarget(object target, MethodInfo method)
        {
            MemberInfo = method;
            Target = target;
            Name = method.Name;
        }


        protected T GetValue<T>()
        {
            return (T) Getter.Invoke();
        }


        protected object GetValue()
        {
            return Getter.Invoke();
        }


        protected void SetValue(object value)
        {
            Setter.Invoke(value);
        }


        protected T GetFieldOrPropertyAttribute<T>() where T : Attribute
        {
            var attributes = MemberInfo.GetCustomAttributes<T>();
            foreach (var attr in attributes)
                if (attr is T)
                    return attr;
            return null;
        }


	    /// <summary>
	    ///     creates the name label and adds a tooltip if present
	    /// </summary>
	    /// <returns>The name label.</returns>
	    /// <param name="table">Table.</param>
	    /// <param name="skin">Skin.</param>
	    protected Label CreateNameLabel(Table table, Skin skin)
        {
            var label = new Label(Name, skin);
            label.SetTouchable(Touchable.Enabled);

            var tooltipAttribute = GetFieldOrPropertyAttribute<TooltipAttribute>();
            if (tooltipAttribute != null)
            {
                var tooltip = new TextTooltip(tooltipAttribute.Tooltip, label, skin);
                table.GetStage().AddElement(tooltip);
            }

            return label;
        }


	    /// <summary>
	    ///     used to setup the UI for the Inspector
	    /// </summary>
	    /// <param name="table">Table.</param>
	    /// <param name="skin">Skin.</param>
	    public abstract void Initialize(Table table, Skin skin);


	    /// <summary>
	    ///     used to update the UI for the Inspector
	    /// </summary>
	    public abstract void Update();
    }
}
#endif