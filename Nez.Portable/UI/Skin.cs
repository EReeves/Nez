﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Graphics.Textures;
using Nez.PipelineRuntime.LibGdxAtlases;
using Nez.PipelineRuntime.TextureAtlas;
using Nez.PipelineRuntime.UISkin;
using Nez.UI.Containers;
using Nez.UI.Drawable;
using Nez.UI.Widgets;
using Nez.Utils;
using IDrawable = Nez.UI.Drawable.IDrawable;

namespace Nez.UI
{
    public class Skin
    {
        private readonly Dictionary<Type, Dictionary<string, object>> _resources =
            new Dictionary<Type, Dictionary<string, object>>();


        public Skin()
        {
        }


	    /// <summary>
	    ///     creates a UISkin from a UISkinConfig
	    /// </summary>
	    /// <param name="configName">the path of the UISkinConfig xnb</param>
	    /// <param name="contentManager">Content manager.</param>
	    public Skin(string configName, NezContentManager contentManager)
        {
            var config = contentManager.Load<UiSkinConfig>(configName);
            if (config.Colors != null)
                foreach (var entry in config.Colors)
                    Add(entry.Key, config.Colors[entry.Key]);

            if (config.TextureAtlases != null)
                foreach (var atlas in config.TextureAtlases)
                    AddSubtextures(contentManager.Load<TextureAtlas>(atlas));

            if (config.LibGdxAtlases != null)
                foreach (var atlas in config.LibGdxAtlases)
                    AddSubtextures(contentManager.Load<LibGdxAtlas>(atlas));

            if (config.Styles != null)
            {
                var styleClasses = config.Styles.GetStyleClasses();
                for (var i = 0; i < styleClasses.Count; i++)
                {
                    var styleType = styleClasses[i];
                    try
                    {
                        var type = Type.GetType("Nez.UI." + styleType, true);
                        var styleNames = config.Styles.GetStyleNames(styleType);

                        for (var j = 0; j < styleNames.Count; j++)
                        {
                            var style = Activator.CreateInstance(type);
                            var styleDict = config.Styles.GetStyleDict(styleType, styleNames[j]);

                            // Get the method by simple name check since we know it's the only one
                            var setStylesForStyleClassMethod =
                                ReflectionUtils.GetMethodInfo(this, "setStylesForStyleClass");
                            setStylesForStyleClassMethod = setStylesForStyleClassMethod.MakeGenericMethod(type);

                            // Return not nec., but it shows that the style is being modified
                            style = setStylesForStyleClassMethod.Invoke(this,
                                new[] {style, styleDict, contentManager, styleNames[j]});

                            Add(styleNames[j], style, type);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Debug.Error("Error creating style from UISkin: {0}", e);
                    }
                }
            }
        }


	    /// <summary>
	    ///     creates a default Skin that can be used for quick mockups. Includes button, textu button, checkbox, progress bar
	    ///     and slider styles.
	    /// </summary>
	    /// <returns>The default skin.</returns>
	    public static Skin CreateDefaultSkin()
        {
            var skin = new Skin();

            // define our colors
            var buttonColor = new Color(78, 91, 98);
            var buttonOver = new Color(168, 207, 115);
            var buttonDown = new Color(244, 23, 135);
            var overFontColor = new Color(85, 127, 27);
            var downFontColor = new Color(255, 255, 255);
            var checkedOverFontColor = new Color(247, 217, 222);

            var checkboxOn = new Color(168, 207, 115);
            var checkboxOff = new Color(63, 63, 63);
            var checkboxOver = new Color(130, 130, 130);
            var checkboxOverFontColor = new Color(220, 220, 220);

            var barBg = new Color(78, 91, 98);
            var barKnob = new Color(25, 144, 188);
            var barKnobOver = new Color(168, 207, 115);
            var barKnobDown = new Color(244, 23, 135);

            var windowColor = new Color(17, 17, 17);

            var textFieldFontColor = new Color(220, 220, 220);
            var textFieldCursorColor = new Color(83, 170, 116);
            var textFieldSelectionColor = new Color(180, 52, 166);
            var textFieldBackgroundColor = new Color(22, 22, 22);

            var scrollPaneScrollBarColor = new Color(44, 44, 44);
            var scrollPaneKnobColor = new Color(241, 156, 0);

            var listBoxBackgroundColor = new Color(20, 20, 20);
            var listBoxSelectionColor = new Color(241, 156, 0);
            var listBoxHoverSelectionColor = new Color(120, 78, 0);

            var selectBoxBackgroundColor = new Color(10, 10, 10);

            // add all our styles
            var buttonStyle = new ButtonStyle
            {
                Up = new PrimitiveDrawable(buttonColor, 10),
                Over = new PrimitiveDrawable(buttonOver),
                Down = new PrimitiveDrawable(buttonDown)
            };
            skin.Add("default", buttonStyle);

            var textButtonStyle = new TextButtonStyle
            {
                Up = new PrimitiveDrawable(buttonColor, 6, 2),
                Over = new PrimitiveDrawable(buttonOver),
                Down = new PrimitiveDrawable(buttonDown),
                OverFontColor = overFontColor,
                DownFontColor = downFontColor,
                PressedOffsetX = 1,
                PressedOffsetY = 1
            };
            skin.Add("default", textButtonStyle);

            var toggleButtonStyle = new TextButtonStyle
            {
                Up = new PrimitiveDrawable(buttonColor, 10, 5),
                Over = new PrimitiveDrawable(buttonOver),
                Down = new PrimitiveDrawable(buttonDown),
                Checkked = new PrimitiveDrawable(new Color(255, 0, 0, 255)),
                CheckedOverFontColor = checkedOverFontColor,
                OverFontColor = overFontColor,
                DownFontColor = downFontColor,
                PressedOffsetX = 1,
                PressedOffsetY = 1
            };
            skin.Add("toggle", toggleButtonStyle);

            var checkboxStyle = new CheckBoxStyle
            {
                CheckboxOn = new PrimitiveDrawable(30, checkboxOn),
                CheckboxOff = new PrimitiveDrawable(30, checkboxOff),
                CheckboxOver = new PrimitiveDrawable(30, checkboxOver),
                OverFontColor = checkboxOverFontColor,
                DownFontColor = downFontColor,
                PressedOffsetX = 1,
                PressedOffsetY = 1
            };
            skin.Add("default", checkboxStyle);

            var progressBarStyle = new ProgressBarStyle
            {
                Background = new PrimitiveDrawable(14, barBg),
                KnobBefore = new PrimitiveDrawable(14, barKnobOver)
            };
            skin.Add("default", progressBarStyle);

            var sliderStyle = new SliderStyle
            {
                Background = new PrimitiveDrawable(6, barBg),
                Knob = new PrimitiveDrawable(14, barKnob),
                KnobOver = new PrimitiveDrawable(14, barKnobOver),
                KnobDown = new PrimitiveDrawable(14, barKnobDown)
            };
            skin.Add("default", sliderStyle);

            var windowStyle = new WindowStyle
            {
                Background = new PrimitiveDrawable(windowColor)
            };
            skin.Add("default", windowStyle);

            var textFieldStyle = TextFieldStyle.Create(textFieldFontColor, textFieldCursorColor,
                textFieldSelectionColor, textFieldBackgroundColor);
            skin.Add("default", textFieldStyle);

            var labelStyle = new LabelStyle();
            skin.Add("default", labelStyle);

            var scrollPaneStyle = new ScrollPaneStyle
            {
                VScroll = new PrimitiveDrawable(6, 0, scrollPaneScrollBarColor),
                VScrollKnob = new PrimitiveDrawable(6, 50, scrollPaneKnobColor),
                HScroll = new PrimitiveDrawable(0, 6, scrollPaneScrollBarColor),
                HScrollKnob = new PrimitiveDrawable(50, 6, scrollPaneKnobColor)
            };
            skin.Add("default", scrollPaneStyle);

            var listBoxStyle = new ListBoxStyle
            {
                FontColorHovered = new Color(255, 255, 255),
                Selection = new PrimitiveDrawable(listBoxSelectionColor, 5, 5),
                HoverSelection = new PrimitiveDrawable(listBoxHoverSelectionColor, 5, 5),
                Background = new PrimitiveDrawable(listBoxBackgroundColor)
            };
            skin.Add("default", listBoxStyle);

            var selectBoxStyle = new SelectBoxStyle
            {
                ListStyle = listBoxStyle,
                ScrollStyle = scrollPaneStyle,
                Background = new PrimitiveDrawable(selectBoxBackgroundColor, 4, 4)
            };
            skin.Add("default", selectBoxStyle);

            var textTooltipStyle = new TextTooltipStyle
            {
                LabelStyle = new LabelStyle(listBoxBackgroundColor),
                Background = new PrimitiveDrawable(checkboxOn, 4, 2)
            };
            skin.Add("default", textTooltipStyle);

            return skin;
        }


	    /// <summary>
	    ///     Recursively finds and sets all styles for a specific style config class that are within
	    ///     the dictionary passed in. This allows skins to contain nested, dynamic style declarations.
	    ///     For example, it allows a SelectBoxStyle to contain a listStyle that is declared inline
	    ///     (and not a reference).
	    /// </summary>
	    /// <param name="styleClass">The style config class instance that needs to be "filled out"</param>
	    /// <param name="styleDict">A dictionary that represents one style name within the style config class (i.e. 'default').</param>
	    /// <param name="styleName">The style name that the dictionary represents (i.e. 'default').</param>
	    /// <typeparam name="T">The style config class type (i.e. SelectBoxStyle)</typeparam>
	    public T SetStylesForStyleClass<T>(T styleClass, Dictionary<string, object> styleDict,
            NezContentManager contentManager, string styleName)
        {
            foreach (var styleConfig in styleDict)
            {
                var name = styleConfig.Key;
                var valueObject = styleConfig.Value;
                var identifier = valueObject.ToString();

                // if name has 'color' in it, we are looking for a color. we check color first because some styles have things like
                // fontColor so we'll check for font after color. We assume these are strings and do no error checking on 'identifier'
                if (name.ToLower().Contains("color"))
                {
                    ReflectionUtils.GetFieldInfo(styleClass, name).SetValue(styleClass, GetColor(identifier));
                }
                else if (name.ToLower().Contains("font"))
                {
                    ReflectionUtils.GetFieldInfo(styleClass, name)
                        .SetValue(styleClass, contentManager.Load<BitmapFont>(identifier));
                }
                else if (name.ToLower().EndsWith("style"))
                {
                    var styleField = ReflectionUtils.GetFieldInfo(styleClass, name);

                    // Check to see if valueObject is a Dictionary object instead of a string. If so, it is an 'inline' style
                    //	and needs to be recursively parsed like any other style. Otherwise, it is assumed to be a string and 
                    //	represents an existing style that has been previously parsed.
                    if (valueObject is Dictionary<string, object>)
                    {
                        // Since there is no existing field to reference, we create it and fill it out by hand
                        var inlineStyle = Activator.CreateInstance(styleField.FieldType);

                        // Recursively call this method with the new field type and dictionary
                        var setStylesForStyleClassMethod =
                            ReflectionUtils.GetMethodInfo(this, "setStylesForStyleClass");
                        setStylesForStyleClassMethod =
                            setStylesForStyleClassMethod.MakeGenericMethod(styleField.FieldType);
                        inlineStyle = setStylesForStyleClassMethod.Invoke(this,
                            new[] {inlineStyle, valueObject as Dictionary<string, object>, contentManager, styleName});
                        styleField.SetValue(styleClass, inlineStyle);
                    }
                    else
                    {
                        // We have a style reference. First we need to find out what type of style name refers to from the field.
                        // Then we need to fetch the "get" method and properly type it.
                        var getStyleMethod = ReflectionUtils.GetMethodInfo(this, "get", new[] {typeof(string)});
                        getStyleMethod = getStyleMethod.MakeGenericMethod(styleField.FieldType);

                        // now we look up the style and finally set it
                        var theStyle = getStyleMethod.Invoke(this, new object[] {identifier});
                        styleField.SetValue(styleClass, theStyle);

                        if (theStyle == null)
                            Debug.Debug.Error("could not find a style reference named {0} when setting {1} on {2}",
                                identifier, name, styleName);
                    }
                }
                else
                {
                    // we have an IDrawable. first we'll try to find a Subtexture and if we cant find one we will see if
                    // identifier is a color
                    var drawable = GetDrawable(identifier);
                    if (drawable != null)
                        ReflectionUtils.GetFieldInfo(styleClass, name).SetValue(styleClass, drawable);
                    else
                        Debug.Debug.Error("could not find a drawable or color named {0} when setting {1} on {2}", identifier,
                            name, styleName);
                }
            }

            return styleClass;
        }

	    /// <summary>
	    ///     Adds all named subtextures from the atlas. If NinePatchSubtextures are found they will be explicitly added as such.
	    /// </summary>
	    /// <param name="atlas">Atlas.</param>
	    public void AddSubtextures(LibGdxAtlas atlas)
        {
            for (int i = 0, n = atlas.Atlases.Count; i < n; i++)
                AddSubtextures(atlas.Atlases[i]);
        }


	    /// <summary>
	    ///     Adds all named subtextures from the atlas
	    /// </summary>
	    /// <param name="atlas">Atlas.</param>
	    public void AddSubtextures(TextureAtlas atlas)
        {
            for (int i = 0, n = atlas.Subtextures.Length; i < n; i++)
            {
                var subtexture = atlas.Subtextures[i];
                if (subtexture is NinePatchSubtexture)
                    Add(atlas.RegionNames[i], subtexture as NinePatchSubtexture);
                else
                    Add(atlas.RegionNames[i], subtexture);
            }
        }


	    /// <summary>
	    ///     adds the typed resource to this skin
	    /// </summary>
	    /// <param name="name">Name.</param>
	    /// <param name="resource">Resource.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T Add<T>(string name, T resource)
        {
            Dictionary<string, object> typedResources;
            if (!_resources.TryGetValue(typeof(T), out typedResources))
            {
                typedResources = new Dictionary<string, object>();
                _resources.Add(typeof(T), typedResources);
            }
            typedResources[name] = resource;
            return resource;
        }


	    /// <summary>
	    ///     adds the typed resource to this skin
	    /// </summary>
	    /// <param name="name">Name.</param>
	    /// <param name="resource">Resource.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public void Add(string name, object resource, Type type)
        {
            Dictionary<string, object> typedResources;
            if (!_resources.TryGetValue(type, out typedResources))
            {
                typedResources = new Dictionary<string, object>();
                _resources.Add(type, typedResources);
            }
            typedResources[name] = resource;
        }


	    /// <summary>
	    ///     removes the typed resource from this skin
	    /// </summary>
	    /// <param name="name">Name.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public void Remove<T>(string name)
        {
            Dictionary<string, object> typedResources;
            if (_resources.TryGetValue(typeof(T), out typedResources))
                typedResources.Remove(name);
        }


	    /// <summary>
	    ///     checks to see if a typed resource exists with the given name
	    /// </summary>
	    /// <param name="name">Name.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public bool Has<T>(string name)
        {
            Dictionary<string, object> typedResources;
            if (_resources.TryGetValue(typeof(T), out typedResources))
                return typedResources.ContainsKey(name);
            return false;
        }


	    /// <summary>
	    ///     First checks for a resource named "default". If it cant find default it will return either the first resource of
	    ///     type T
	    ///     or default(T) if none are found.
	    /// </summary>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T Get<T>()
        {
            if (Has<T>("default"))
                return Get<T>("default");

            Dictionary<string, object> typedResources;
            if (_resources.TryGetValue(typeof(T), out typedResources))
                return (T) typedResources[typedResources.First().Key];

            return default(T);
        }


	    /// <summary>
	    ///     Returns a named resource of the specified type or default(T) if it couldnt be found
	    /// </summary>
	    /// <param name="name">Name.</param>
	    /// <typeparam name="T">The 1st type parameter.</typeparam>
	    public T Get<T>(string name)
        {
            if (name == null)
                return Get<T>();

            Dictionary<string, object> typedResources;
            if (!_resources.TryGetValue(typeof(T), out typedResources))
                return default(T);

            if (!typedResources.ContainsKey(name))
                return default(T);

            return (T) typedResources[name];
        }


        public Color GetColor(string name)
        {
            return Get<Color>(name);
        }


        public BitmapFont GetFont(string name)
        {
            return Get<BitmapFont>(name);
        }


        public Subtexture GetSubtexture(string name)
        {
            return Get<Subtexture>(name);
        }


        public NinePatchSubtexture GetNinePatchSubtexture(string name)
        {
            return Get<NinePatchSubtexture>(name);
        }


	    /// <summary>
	    ///     Returns a registered subtexture drawable. If no subtexture drawable is found but a Subtexture exists with the name,
	    ///     a
	    ///     subtexture drawable is created from the Subtexture and stored in the skin
	    /// </summary>
	    /// <returns>The subtexture drawable.</returns>
	    /// <param name="name">Name.</param>
	    public SubtextureDrawable GetSubtextureDrawable(string name)
        {
            var subtextureDrawable = Get<SubtextureDrawable>(name);
            if (subtextureDrawable != null)
                return subtextureDrawable;

            var subtexture = Get<Subtexture>(name);
            if (subtexture != null)
            {
                subtextureDrawable = new SubtextureDrawable(subtexture);
                Add(name, subtextureDrawable);
            }

            return subtextureDrawable;
        }


	    /// <summary>
	    ///     Returns a registered drawable. If no drawable is found but a Subtexture/NinePatchSubtexture exists with the name,
	    ///     then the
	    ///     appropriate drawable is created and stored in the skin. If name is a color a PrimitiveDrawable will be created and
	    ///     stored.
	    /// </summary>
	    /// <returns>The drawable.</returns>
	    /// <param name="name">Name.</param>
	    public IDrawable GetDrawable(string name)
        {
            var drawable = Get<IDrawable>(name);
            if (drawable != null)
                return drawable;

            // Check for explicit registration of ninepatch, subtexture or tiled drawable
            drawable = Get<SubtextureDrawable>(name);
            if (drawable != null)
                return drawable;

            drawable = Get<NinePatchDrawable>(name);
            if (drawable != null)
                return drawable;

            drawable = Get<TiledDrawable>(name);
            if (drawable != null)
                return drawable;

            drawable = Get<PrimitiveDrawable>(name);
            if (drawable != null)
                return drawable;

            // still nothing. check for a NinePatchSubtexture or a Subtexture and create a new drawable if we find one
            var ninePatchSubtexture = Get<NinePatchSubtexture>(name);
            if (ninePatchSubtexture != null)
            {
                drawable = new NinePatchDrawable(ninePatchSubtexture);
                Add(name, drawable as NinePatchDrawable);
                return drawable;
            }

            var subtexture = Get<Subtexture>(name);
            if (subtexture != null)
            {
                drawable = new SubtextureDrawable(subtexture);
                Add(name, drawable as SubtextureDrawable);
                return drawable;
            }

            // finally, we will check if name is a Color and create a PrimitiveDrawable if it is
            if (Has<Color>(name))
            {
                var color = Get<Color>(name);
                drawable = new PrimitiveDrawable(color);
                Add(name, drawable as PrimitiveDrawable);
                return drawable;
            }

            return null;
        }


	    /// <summary>
	    ///     Returns a registered tiled drawable. If no tiled drawable is found but a Subtexture exists with the name, a tiled
	    ///     drawable is
	    ///     created from the Subtexture and stored in the skin
	    /// </summary>
	    /// <returns>The tiled drawable.</returns>
	    /// <param name="name">Name.</param>
	    public TiledDrawable GetTiledDrawable(string name)
        {
            var tiledDrawable = Get<TiledDrawable>(name);
            if (tiledDrawable != null)
                return tiledDrawable;

            var subtexture = Get<Subtexture>(name);
            if (subtexture != null)
            {
                tiledDrawable = new TiledDrawable(subtexture);
                Add(name, tiledDrawable);
            }

            return tiledDrawable;
        }


	    /// <summary>
	    ///     Returns a registered ninepatch. If no ninepatch is found but a Subtexture exists with the name, a ninepatch is
	    ///     created from the
	    ///     Subtexture and stored in the skin.
	    /// </summary>
	    /// <returns>The nine patch.</returns>
	    /// <param name="name">Name.</param>
	    public NinePatchDrawable GetNinePatchDrawable(string name)
        {
            var ninePatchDrawable = Get<NinePatchDrawable>(name);
            if (ninePatchDrawable != null)
                return ninePatchDrawable;

            var ninePatchSubtexture = Get<NinePatchSubtexture>(name);
            if (ninePatchSubtexture != null)
            {
                ninePatchDrawable = new NinePatchDrawable(ninePatchSubtexture);
                Add(name, ninePatchDrawable);
                return ninePatchDrawable;
            }

            var subtexture = Get<NinePatchSubtexture>(name);
            if (subtexture != null)
            {
                ninePatchDrawable = new NinePatchDrawable(subtexture, 0, 0, 0, 0);
                Add(name, ninePatchDrawable);
            }

            return ninePatchDrawable;
        }


	    /// <summary>
	    ///     Returns a tinted copy of a drawable found in the skin via getDrawable. Note that the new drawable is NOT
	    ///     added to the skin! Tinting is only supported on SubtextureDrawables and NinePatchDrawables.
	    /// </summary>
	    /// <returns>The tinted drawable.</returns>
	    /// <param name="name">Name.</param>
	    /// <param name="tint">Tint.</param>
	    public IDrawable NewTintedDrawable(string name, Color tint)
        {
            var drawable = GetDrawable(name);
            if (drawable is SubtextureDrawable)
                return (drawable as SubtextureDrawable).NewTintedDrawable(tint);

            if (drawable is NinePatchDrawable)
                return (drawable as NinePatchDrawable).NewTintedDrawable(tint);

            throw new Exception("Unable to copy, unknown or unsupported drawable type: " + drawable);
        }
    }
}