using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Nez.PipelineRuntime.UISkin
{
    public class UiSkinConfigReader : ContentTypeReader<UiSkinConfig>
    {
        protected override UiSkinConfig Read(ContentReader reader, UiSkinConfig existingInstance)
        {
            var skinConfig = new UiSkinConfig();

            if (reader.ReadBoolean())
                skinConfig.Colors = reader.ReadObject<Dictionary<string, Color>>();

            if (reader.ReadBoolean())
                skinConfig.TextureAtlases = reader.ReadObject<string[]>();

            if (reader.ReadBoolean())
                skinConfig.LibGdxAtlases = reader.ReadObject<string[]>();

            if (reader.ReadBoolean())
                skinConfig.Styles = reader.ReadObject<UiSkinStyleConfig>();

            return skinConfig;
        }
    }
}