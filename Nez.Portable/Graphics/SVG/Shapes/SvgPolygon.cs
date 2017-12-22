using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;

namespace Nez.Svg
{
    public class SvgPolygon : SvgElement
    {
        [XmlAttribute("cx")] public float CenterX;

        [XmlAttribute("cy")] public float CenterY;

        public Vector2[] Points;

        [XmlAttribute("sides")] public int Sides;

        [XmlAttribute("points")]
        public string PointsAttribute
        {
            get => null;
            set => ParsePoints(value);
        }


        private void ParsePoints(string str)
        {
            var pairs = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            Points = new Vector2[pairs.Length];

            for (var i = 0; i < pairs.Length; i++)
            {
                var parts = pairs[i].Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
                Points[i] = new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
        }


        public Vector2[] GetTransformedPoints()
        {
            var pts = new Vector2[Points.Length];
            var mat = GetCombinedMatrix();
            Vector2Ext.Transform(Points, ref mat, pts);

            return pts;
        }


	    /// <summary>
	    ///     gets the points relative to the center. SVG by default uses absolute positions for points.
	    /// </summary>
	    /// <returns>The relative points.</returns>
	    public Vector2[] GetRelativePoints()
        {
            var pts = new Vector2[Points.Length];

            var center = new Vector2(CenterX, CenterY);
            for (var i = 0; i < Points.Length; i++)
                pts[i] = Points[i] - center;

            return pts;
        }
    }
}