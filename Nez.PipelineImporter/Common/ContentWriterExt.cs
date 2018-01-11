using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Nez.PipelineImporter.Common
{
	public static class ContentWriterExt
	{
		public static void Write( this ContentWriter writer, Rectangle rect )
		{
			writer.Write( rect.X );
			writer.Write( rect.Y );
			writer.Write( rect.Width );
			writer.Write( rect.Height );
		}
	}
}

