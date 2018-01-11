using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Nez.PipelineRuntime.Overlap2D
{
    public class O2DSceneReader : ContentTypeReader<O2DScene>
    {
        protected override O2DScene Read(ContentReader reader, O2DScene existingInstance)
        {
            var scene = new O2DScene();
            scene.SceneName = reader.ReadString();
            scene.AmbientColor = reader.ReadColor();
            scene.Composite = ReadComposite(reader);

            return scene;
        }


        private O2DComposite ReadComposite(ContentReader reader)
        {
            var composite = new O2DComposite();

            var imageCount = reader.ReadInt32();
            for (var i = 0; i < imageCount; i++)
            {
                var image = new O2DImage();
                ReadMainItem(reader, image);
                image.ImageName = reader.ReadString();

                composite.Images.Add(image);
            }


            var colorPrimitiveCount = reader.ReadInt32();
            for (var i = 0; i < colorPrimitiveCount; i++)
            {
                var colorPrim = new O2DColorPrimitive();
                ReadMainItem(reader, colorPrim);
                ReadColorPrimitive(reader, colorPrim);

                composite.ColorPrimitives.Add(colorPrim);
            }


            var compositeItemCount = reader.ReadInt32();
            for (var i = 0; i < compositeItemCount; i++)
            {
                var compositeItem = new O2DCompositeItem();
                ReadMainItem(reader, compositeItem);
                compositeItem.Composite = ReadComposite(reader);

                composite.CompositeItems.Add(compositeItem);
            }

            return composite;
        }


        private void ReadMainItem(ContentReader reader, O2DMainItem item)
        {
            item.UniqueId = reader.ReadInt32();
            item.ItemIdentifier = reader.ReadString();
            item.ItemName = reader.ReadString();
            item.CustomVars = reader.ReadString();
            item.X = reader.ReadSingle();
            item.Y = reader.ReadSingle();
            item.ScaleX = reader.ReadSingle();
            item.ScaleY = reader.ReadSingle();
            item.OriginX = reader.ReadSingle();
            item.OriginY = reader.ReadSingle();
            item.Rotation = reader.ReadSingle();
            item.ZIndex = reader.ReadInt32();
            item.LayerDepth = reader.ReadSingle();
            item.LayerName = reader.ReadString();
            item.RenderLayer = reader.ReadInt32();
            item.Tint = reader.ReadColor();
        }


        private void ReadColorPrimitive(ContentReader reader, O2DColorPrimitive colorPrim)
        {
            var count = reader.ReadInt32();

            // special care needs to be taken here. if we have 4 verts everything will be fine. If we have any other number we need to
            // reverse the array
            colorPrim.Polygon = new Vector2[count];

            for (var i = 0; i < count; i++)
                colorPrim.Polygon[i] = reader.ReadVector2();

            if (count != 4)
                Array.Reverse(colorPrim.Polygon);
        }
    }
}