using System.Xml.Serialization;

namespace Nez.Graphics.SVG.Shapes
{
    public class SvgCircle : SvgElement
    {
        [XmlAttribute("cx")] public float CenterX;

        [XmlAttribute("cy")] public float CenterY;

        [XmlAttribute("r")] public float Radius;
    }
}