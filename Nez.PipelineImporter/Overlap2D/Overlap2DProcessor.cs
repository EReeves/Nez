using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline;
using Nez.PipelineImporter.Overlap2D.VOs;

namespace Nez.PipelineImporter.Overlap2D
{
	[ContentProcessor( DisplayName = "Overlap2D Processor" )]
	public class Overlap2DProcessor : ContentProcessor<SceneVO,SceneVO>
	{
		public override SceneVO Process( SceneVO scene, ContentProcessorContext context )
		{
			// deal with converting renderLayer into a layerDepth. first we need to find the max zIndex
			var indicies = scene.composite.findMinMaxZindexForRenderLayers();
			var minIndicies = indicies.Item1;
			var maxIndicies = indicies.Item2;

			// increment all values by 1 to avoid divide by zero issues
			var keys = new List<int>( maxIndicies.Keys );
			foreach( var renderLayer in keys )
				maxIndicies[renderLayer] = maxIndicies[renderLayer] + 1;
			
			scene.composite.setLayerDepthRecursively( minIndicies, maxIndicies, null );

			return scene;
		}

	}
}
