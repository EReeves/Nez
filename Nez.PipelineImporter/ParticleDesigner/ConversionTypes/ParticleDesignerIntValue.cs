using System.Xml.Serialization;

namespace Nez.PipelineImporter.ParticleDesigner.ConversionTypes
{
	public class ParticleDesignerIntValue
	{
		[XmlAttribute]
		public int value;


		public static implicit operator int( ParticleDesignerIntValue obj )
		{
			return obj.value;
		}

	}
}

