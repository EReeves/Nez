using System.Xml.Serialization;

namespace Nez.PipelineImporter.ParticleDesigner.ConversionTypes
{
	public class ParticleDesignerTexture
	{
		[XmlAttribute]
		public string name;

		[XmlAttribute]
		public string data;
	}
}

