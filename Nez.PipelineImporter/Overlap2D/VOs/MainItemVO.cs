﻿using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Nez.Maths;
using Nez.PipelineImporter.Overlap2D.VOs.NotImplemented;

namespace Nez.PipelineImporter.Overlap2D.VOs
{
	public class MainItemVO
	{
		public int uniqueId = -1;
		public string itemIdentifier = string.Empty;
		public string itemName = string.Empty;
		public string[] tags;
		public string customVars = string.Empty;
		public float x;
		public float y;
		public float scaleX	= 1f;
		public float scaleY	= 1f;
		public float originX = 0;
		public float originY = 0;
		public float rotation;
		public int zIndex = 0;
		public float layerDepth;
		public string layerName
		{
			get => _layerName;
			set
			{
				if( !string.IsNullOrEmpty(value) )
				{
					var regex = new Regex( @"\d+" );
					var match = regex.Match( value );
					if( match.Success )
						renderLayer = int.Parse( match.Value );
				}
				_layerName = value;
			}
		}
		string _layerName;

		// not part of Overlap2D spec
		public int renderLayer = 0;
		public float[] tint = { 1, 1, 1, 1 };

		public string shaderName = string.Empty;

		// this is written only for sColorPrimitives
		public ShapeVO shape;
		public PhysicsBodyDataVO physics;


		/// <summary>
		/// helper to translate zIndex to layerDepth. zIndexMax should be at least equal to the highest zIndex
		/// </summary>
		/// <returns>The depth.</returns>
		/// <param name="zIndexMax">Z index max.</param>
		public float calculateLayerDepth( float zIndexMin, float zIndexMax, CompositeItemVO compositeItem )
		{
			return compositeItem?.calculateLayerDepthForChild( zIndexMin, zIndexMax, this ) ?? Mathf.Map01( (float)zIndex, zIndexMin, zIndexMax );
		}


		public override string ToString()
		{
			return JsonConvert.SerializeObject( this, Formatting.Indented );
		}
	}
}

