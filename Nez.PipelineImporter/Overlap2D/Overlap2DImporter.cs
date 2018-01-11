using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Newtonsoft.Json;
using Nez.PipelineImporter.Overlap2D.VOs;

namespace Nez.PipelineImporter.Overlap2D
{
	[ContentImporter( ".dt", DefaultProcessor = "Overlap2DProcessor", DisplayName = "Overlap2D Importer" )]
	public class Overlap2DImporter : ContentImporter<SceneVO>
	{
		public static ContentBuildLogger logger;


		public override SceneVO Import( string filename, ContentImporterContext context )
		{
			var projectFolder = Directory.GetParent( Path.GetDirectoryName( filename ) ).FullName;
			var projectfile = Path.Combine( projectFolder, "project.dt" );

			// if the project file isnt a directly up check the same directory
			if( !File.Exists( projectfile ) )
			{
				projectfile = Path.Combine( Path.GetDirectoryName( filename ), "project.dt" );

				if( !File.Exists( projectfile ) )
				{
					projectfile = null;
					logger.LogMessage( "could not find project.dt file. Using a pixelToWorld of 1. If this is not the correct value make sure your project.dt file is present!" );
				}
			}

			// we need the pixelToWorld from the project file so we have to load it up
			ProjectInfoVO project;
			if( projectfile != null )
			{
				using( var reader = new StreamReader( projectfile ) )
				{
					logger = context.Logger;
					context.Logger.LogMessage( "deserializing project file: {0}", projectfile );

					project = JsonConvert.DeserializeObject<ProjectInfoVO>( reader.ReadToEnd() );
				}
			}
			else
			{
				project = new ProjectInfoVO();
			}

			using( var reader = new StreamReader( filename ) )
			{
				logger = context.Logger;
				context.Logger.LogMessage( "deserializing scene file: {0}", filename );

				var scene = JsonConvert.DeserializeObject<SceneVO>( reader.ReadToEnd() );
				scene.pixelToWorld = project.pixelToWorld;

				return scene;
			}
		}
	}
}

