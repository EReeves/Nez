using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.PipelineRuntime.ContentExtensions;

namespace Nez.PipelineRuntime.TexturePacker
{
    public class TexturePackerAtlasReader : ContentTypeReader<TexturePackerAtlas>
    {
        protected override TexturePackerAtlas Read(ContentReader reader, TexturePackerAtlas existingInstance)
        {
            var assetName = reader.GetRelativeAssetPath(reader.ReadString());
            var texture = reader.ContentManager.Load<Texture2D>(assetName);
            var atlas = new TexturePackerAtlas(texture);

            var regionCount = reader.ReadInt32();
            for (var i = 0; i < regionCount; i++)
                atlas.CreateRegion
                (
                    reader.ReadString(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadInt32(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                );

            atlas.SpriteAnimationDetails = reader.ReadObject<Dictionary<string, List<int>>>();

            return atlas;
        }
    }
}