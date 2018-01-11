using System.Xml.Serialization;

namespace Nez.PipelineImporter.Tiled
{
	public class TmxImageLayer : TmxLayer
	{
		[XmlElement( ElementName = "image" )]
		public TmxImage image;


		public override string ToString()
		{
			return string.Format( "[TmxImageLayer] name: {0}, image: {1}, visible: {2}", name, image, visible );
		}
	}
}