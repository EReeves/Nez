using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nez.PipelineRuntime.UISkin;
using Nez.UI;

namespace Nez.PipelineImporter.UISkin
{
	[ContentTypeWriter]
	public class UISkinWriter : ContentTypeWriter<UiSkinConfig>
	{
		protected override void Write( ContentWriter writer, UiSkinConfig data )
		{
			if( data.Colors != null )
			{
				writer.Write( true );
				writer.WriteObject( data.Colors );
			}
			else
			{
				writer.Write( false );
			}

			if( data.TextureAtlases != null )
			{
				writer.Write( true );
				writer.WriteObject( data.TextureAtlases );
			}
			else
			{
				writer.Write( false );
			}

			if( data.LibGdxAtlases != null )
			{
				writer.Write( true );
				writer.WriteObject( data.LibGdxAtlases );
			}
			else
			{
				writer.Write( false );
			}

			if( data.Styles != null )
			{
				writer.Write( true );
				writer.WriteObject( data.Styles );
			}
			else
			{
				writer.Write( false );
			}
		}


		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( UiSkinConfig ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			return typeof( UiSkinConfigReader ).AssemblyQualifiedName;
		}
	}
}

