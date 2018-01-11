using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.PipelineRuntime.Overlap2D
{
    public class O2DMainItem
    {
        private Dictionary<string, string> _customVarsDict;
        public string CustomVars;
        public string ItemIdentifier;
        public string ItemName;

	    /// <summary>
	    ///     layerDepth is calculated by the Pipeline processor. It is derrived by getting the max zIndex and converting it to
	    ///     the MonoGame
	    ///     0 - 1 range. If sorting issues arise the CompositeItemVO.calculateLayerDepthForChild method is where to look. The
	    ///     default value
	    ///     probably just needs to be increased a bit.
	    /// </summary>
	    public float LayerDepth;

        public string LayerName;
        public float OriginX;
        public float OriginY;

	    /// <summary>
	    ///     renderLayer is derived from the layer name set in Overlap2D. If the layer name contains an integer that value will
	    ///     be parsed and set.
	    /// </summary>
	    public int RenderLayer;

        public float Rotation;
        public float ScaleX;
        public float ScaleY;
        public Color Tint;
        public int UniqueId;
        public float X;
        public float Y;
        public int ZIndex;


	    /// <summary>
	    ///     translates the bottom-left based origin of Overlap2D to a top-left based origin
	    /// </summary>
	    /// <returns>The for image size.</returns>
	    /// <param name="width">Width.</param>
	    /// <param name="height">Height.</param>
	    public Vector2 OrginForImageSize(float width, float height)
        {
            var origin = new Vector2(0, height);
            return origin;
        }


	    /// <summary>
	    ///     helper to translate zIndex to layerDepth. zIndexMax should be at least equal to the highest zIndex
	    /// </summary>
	    /// <returns>The depth.</returns>
	    /// <param name="zIndexMax">Z index max.</param>
	    public float CalculateLayerDepth(float zIndexMax)
        {
            return (zIndexMax - ZIndex) / zIndexMax;
        }


        public Dictionary<string, string> GetCustomVars()
        {
            if (_customVarsDict == null)
                ParseCustomVars();

            return _customVarsDict;
        }


        public string GetCustomVarString(string key)
        {
            if (_customVarsDict == null)
                ParseCustomVars();

            string value = null;
            _customVarsDict.TryGetValue(key, out value);

            return value;
        }


        public float GetCustomVarFloat(string key, float defaultValue = 0f)
        {
            if (_customVarsDict == null)
                ParseCustomVars();

            string value = null;
            if (_customVarsDict.TryGetValue(key, out value))
                return float.Parse(value);

            return defaultValue;
        }


        public int GetCustomVarInt(string key, int defaultValue = 0)
        {
            if (_customVarsDict == null)
                ParseCustomVars();

            string value = null;
            if (_customVarsDict.TryGetValue(key, out value))
                return int.Parse(value);

            return defaultValue;
        }


        public bool GetCustomVarBool(string key, bool defaultValue = true)
        {
            if (_customVarsDict == null)
                ParseCustomVars();

            string value = null;
            if (_customVarsDict.TryGetValue(key, out value))
                return bool.Parse(value);

            return defaultValue;
        }


        private void ParseCustomVars()
        {
            _customVarsDict = new Dictionary<string, string>();

            var vars = CustomVars.Split(';');
            for (var i = 0; i < vars.Length; i++)
            {
                var tmp = vars[i].Split(':');
                if (tmp.Length > 1)
                    _customVarsDict.Add(tmp[0], tmp[1]);
            }
        }
    }
}