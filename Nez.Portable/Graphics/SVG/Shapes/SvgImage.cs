﻿using System;
using System.IO;
using System.Net.Http;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Maths;
using Nez.Utils;

namespace Nez.Graphics.SVG.Shapes
{
	/// <summary>
	///     represents the image tag in an SVG document. This class will do its best to load the image from the href attribute.
	///     It will check for
	///     embedded images, web-based images and then fall back to using the href to load from a ContentManager.
	/// </summary>
	public class SvgImage : SvgElement
    {
	    /// <summary>
	    ///     flag that determines if we tried to load the texture. We only attempt to load it once.
	    /// </summary>
	    private bool _didAttemptTextureLoad;

	    /// <summary>
	    ///     cached texture if loaded successfully
	    /// </summary>
	    private Texture2D _texture;

        [XmlAttribute("height")] public float Height;

        [XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")] public string Href;

        [XmlAttribute("width")] public float Width;

        [XmlAttribute("x")] public float X;

        [XmlAttribute("y")] public float Y;

	    /// <summary>
	    ///     the rect encompassing this image. Note that the rect is with no transforms applied.
	    /// </summary>
	    /// <value>The rect.</value>
	    public RectangleF Rect => new RectangleF(X, Y, Width, Height);


	    /// <summary>
	    ///     attempts to get a texture for the image
	    ///     - first it will check the href for a png file name. If it finds one it will load it with the ContentManager passed
	    ///     in
	    ///     - next it will see if the href is a url and if so it will load it
	    ///     - next it checks for an embedded, base64 image. It will load that if it finds one
	    /// </summary>
	    /// <returns>The texture.</returns>
	    /// <param name="content">Content.</param>
	    public Texture2D GetTexture(NezContentManager content)
        {
            if (_didAttemptTextureLoad || _texture != null)
                return _texture;

            // check for a url
            if (Href.StartsWith("http"))
            {
                using (var client = new HttpClient())
                {
                    var stream = client.GetStreamAsync(Href).Result;
                    _texture = Texture2D.FromStream(Core.GraphicsDevice, stream);
                }
            }
            // see if we have a path to a png files in the href
            else if (Href.EndsWith("png"))
            {
                // check for existance before attempting to load! We are a PCL so we cant so we'll catch the Exception instead
                try
                {
                    if (content != null)
                        _texture = content.Load<Texture2D>(Href);
                }
                catch (ContentLoadException)
                {
                    Debug.Debug.Error("Could not load SvgImage from href: {0}", Href);
                }
            }
            // attempt to parse the base64 string if it is embedded in the href
            else if (Href.StartsWith("data:"))
            {
                var startIndex = Href.IndexOf("base64,", StringComparison.OrdinalIgnoreCase) + 7;
                var imageContents = Href.Substring(startIndex);
                var bytes = Convert.FromBase64String(imageContents);

                using (var m = new MemoryStream())
                {
                    m.Write(bytes, 0, bytes.Length);
                    m.Seek(0, SeekOrigin.Begin);

                    _texture = Texture2D.FromStream(Core.GraphicsDevice, m);
                }
            }

            _didAttemptTextureLoad = true;
            return _texture;
        }
    }
}