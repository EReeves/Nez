using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
    public abstract class TiledLayer
    {
        public string Name;
        public Vector2 Offset;
        public float Opacity;
        public Dictionary<string, string> Properties;
        public bool Visible = true;


        protected TiledLayer(string name)
        {
            this.Name = name;
            Properties = new Dictionary<string, string>();
        }


        public abstract void Draw(Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds);
    }
}