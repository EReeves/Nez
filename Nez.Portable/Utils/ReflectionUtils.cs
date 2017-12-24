#define NETFX_CORE
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nez
{
	/// <summary>
	///     helper class to fetch property delegates
	/// </summary>
	internal class ReflectionUtils
    {
        public static Assembly GetAssembly(Type type)
        {
#if NETFX_CORE
            return type.GetTypeInfo().Assembly;
#else
			return type.Assembly;
			#endif
        }


        public static FieldInfo GetFieldInfo(object targetObject, string fieldName)
        {
            FieldInfo fieldInfo = null;
            var type = targetObject.GetType();

#if NETFX_CORE
            foreach (var fi in type.GetRuntimeFields())
                if (fi.Name == fieldName)
                {
                    fieldInfo = fi;
                    break;
                }
#else
			do
			{
				fieldInfo = type.GetField( fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
				type = type.BaseType;
			} while ( fieldInfo == null && type != null );
			#endif

            return fieldInfo;
        }


        public static IEnumerable<FieldInfo> GetFields(Type type)
        {
#if NETFX_CORE
            return type.GetRuntimeFields();
#else
			return type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			#endif
        }


        public static object GetFieldValue(object targetObject, string fieldName)
        {
            var fieldInfo = GetFieldInfo(targetObject, fieldName);
            return fieldInfo.GetValue(targetObject);
        }


        public static PropertyInfo GetPropertyInfo(object targetObject, string propertyName)
        {
#if NETFX_CORE
            return targetObject.GetType().GetRuntimeProperty(propertyName);
#else
			return targetObject.GetType().GetProperty( propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			#endif
        }


        public static IEnumerable<PropertyInfo> GetProperties(Type type)
        {
#if NETFX_CORE
            return type.GetRuntimeProperties();
#else
			return type.GetProperties( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			#endif
        }


        public static MethodInfo GetPropertyGetter(PropertyInfo prop)
        {
#if NETFX_CORE
            return prop?.GetMethod;
#else
			return prop.GetGetMethod( true );
			#endif
        }


        public static MethodInfo GetPropertySetter(PropertyInfo prop)
        {
#if NETFX_CORE
            return prop.SetMethod;
#else
			return prop.GetSetMethod( true );
			#endif
        }


        public static object GetPropertyValue(object targetObject, string propertyName)
        {
            var propInfo = GetPropertyInfo(targetObject, propertyName);
            var methodInfo = GetPropertyGetter(propInfo);
            return methodInfo.Invoke(targetObject, new object[] { });
        }


        public static IEnumerable<MethodInfo> GetMethods(Type type)
        {
#if NETFX_CORE
            return type.GetRuntimeMethods();
#else
			return type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			#endif
        }


        public static MethodInfo GetMethodInfo(object targetObject, string methodName)
        {
            return GetMethodInfo(targetObject.GetType(), methodName);
        }


        public static MethodInfo GetMethodInfo(object targetObject, string methodName, Type[] parameters)
        {
            return GetMethodInfo(targetObject.GetType(), methodName, parameters);
        }


        public static MethodInfo GetMethodInfo(Type type, string methodName, Type[] parameters = null)
        {
#if NETFX_CORE
            if (parameters != null)
                return type.GetRuntimeMethod(methodName, parameters);

            foreach (var method in type.GetRuntimeMethods())
                if (method.Name == methodName)
                    return method;
            return null;
#else
			if( parameters == null )
				return type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
			return type.GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, Type.DefaultBinder, parameters, null );
			#endif
        }


        public static T CreateDelegate<T>(object targetObject, MethodInfo methodInfo)
        {
#if NETFX_CORE
            return (T) (object) methodInfo.CreateDelegate(typeof(T), targetObject);
#else
			return (T)(object)Delegate.CreateDelegate( typeof( T ), targetObject, methodInfo );
			#endif
        }


	    /// <summary>
	    ///     either returns a super fast Delegate to set the given property or null if it couldn't be found
	    ///     via reflection
	    /// </summary>
	    public static T SetterForProperty<T>(object targetObject, string propertyName)
        {
            // first get the property
            var propInfo = GetPropertyInfo(targetObject, propertyName);
            if (propInfo == null)
                return default(T);

            return CreateDelegate<T>(targetObject, propInfo.SetMethod);
        }


	    /// <summary>
	    ///     either returns a super fast Delegate to get the given property or null if it couldn't be found
	    ///     via reflection
	    /// </summary>
	    public static T GetterForProperty<T>(object targetObject, string propertyName)
        {
            // first get the property
            var propInfo = GetPropertyInfo(targetObject, propertyName);
            if (propInfo == null)
                return default(T);

            return CreateDelegate<T>(targetObject, propInfo.GetMethod);
        }
    }
}