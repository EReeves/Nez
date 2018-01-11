using System.Xml.Serialization;

namespace Nez.Graphics.SVG.Shapes
{
    public class SvgEllipse : SvgElement
    {
        [XmlAttribute("cx")] public float CenterX;

        [XmlAttribute("cy")] public float CenterY;

        [XmlAttribute("rx")] public float RadiusX;

        [XmlAttribute("ry")] public float RadiusY;
    }
}