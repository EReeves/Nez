using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;

// ReSharper disable once CheckNamespace
namespace Nez
{
	[ContentImporter( ".fnt", DefaultProcessor = "BitmapFontProcessor", DisplayName = "BMFont Importer" )]
	public class BitmapFontImporter : ContentImporter<BitmapFontFile>
	{
		public override BitmapFontFile Import( string filename, ContentImporterContext context )
		{
			context.Logger.LogMessage( "Importing XML file: {0}", filename );

			using( var streamReader = new StreamReader( filename ) )
			{
				var deserializer = new XmlSerializer( typeof( BitmapFontFile ) );
				var bmFontFile = (BitmapFontFile)deserializer.Deserialize( streamReader );
				bmFontFile.file = filename;

				return bmFontFile;
			}
		}
	}
}