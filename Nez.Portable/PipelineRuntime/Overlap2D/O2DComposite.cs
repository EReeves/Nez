using System.Collections.Generic;

namespace Nez.PipelineRuntime.Overlap2D
{
    public class O2DComposite
    {
        public List<O2DColorPrimitive> ColorPrimitives = new List<O2DColorPrimitive>();
        public List<O2DCompositeItem> CompositeItems = new List<O2DCompositeItem>();
        public List<O2DImage> Images = new List<O2DImage>();
    }
}