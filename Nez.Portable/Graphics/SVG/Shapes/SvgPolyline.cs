using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Nez.Svg
{
    public class SvgPolyline : SvgElement
    {
        public Vector2[] Points;

        [XmlAttribute("points")]
        public string PointsAttribute
        {
            get => null;
            set => ParsePoints(value);
        }


        private void ParsePoints(string str)
        {
            // normalize commas and spaces since some programs use comma separate points and others use spaces
            str = str.Replace(',', ' ');
            var pairs = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            Points = new Vector2[pairs.Length / 2];

            var pointIndex = 0;
            for (var i = 0; i < pairs.Length; i += 2)
                Points[pointIndex++] = new Vector2(float.Parse(pairs[i]), float.Parse(pairs[i + 1]));
        }


        public Vector2[] GetTransformedPoints()
        {
            var pts = new Vector2[Points.Length];
            var mat = GetCombinedMatrix();
            Vector2Ext.Transform(Points, ref mat, pts);

            return pts;
        }
    }
}