using System.Xml.Serialization;
using Nez.Graphics.SVG.Shapes;
using Nez.Graphics.SVG.Shapes.Paths;

namespace Nez.Graphics.SVG
{
	/// <summary>
	///     container in SVG. The 'g' XML tag.
	/// </summary>
	public class SvgGroup : SvgElement
    {
        [XmlElement("path")] public SvgPath[] Paths;

        [XmlElement("rect")] public SvgRectangle[] Rectangles;

        [XmlElement("line")] public SvgLine[] Lines;

        [XmlElement("circle")] public SvgCircle[] Circles;

        [XmlElement("ellipse")] public SvgEllipse[] Ellipses;

        [XmlElement("title")] public string Title;

        [XmlElement("g")] public SvgGroup[] Groups;

        [XmlElement("polygon")] public SvgPolygon[] Polygons;

        [XmlElement("polyline")] public SvgPolyline[] Polylines;

        [XmlElement("image")] public SvgImage[] Images;
    }
}