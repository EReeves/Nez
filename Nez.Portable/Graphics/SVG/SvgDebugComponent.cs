﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.ECS;
using Nez.ECS.Components.Renderables;
using Nez.Graphics.Batcher;
using Nez.Graphics.SVG.Shapes;
using Nez.Graphics.SVG.Shapes.Paths;
using Nez.Maths;

namespace Nez.Graphics.SVG
{
	/// <summary>
	///     assists in debugging the data from an SVG file. All the supported shapes will be displayed.
	/// </summary>
	public class SvgDebugComponent : RenderableComponent
    {
        private readonly ISvgPathBuilder _pathBuilder;

        public SvgDocument SvgDoc;


	    /// <summary>
	    ///     beware! If pathBuilder is null the SvgReflectionPathBuilder will be used and it is slow as dirt.
	    /// </summary>
	    /// <param name="pathToSvgFile">Path to svg file.</param>
	    /// <param name="pathBuilder">Path builder.</param>
	    public SvgDebugComponent(string pathToSvgFile, ISvgPathBuilder pathBuilder = null)
        {
            SvgDoc = SvgDocument.Open(TitleContainer.OpenStream("Content/" + pathToSvgFile));
            _pathBuilder = pathBuilder;
            _bounds = new RectangleF(0, 0, SvgDoc.Width, SvgDoc.Height);

            if (_pathBuilder == null)
                _pathBuilder = new SvgReflectionPathBuilder();
        }

        public override RectangleF Bounds
        {
            get
            {
                if (AreBoundsDirty)
                {
                    _bounds.Location = Entity.Transform.Position;
                    AreBoundsDirty = false;
                }
                return Bounds;
            }
        }


        public override void Render(Graphics graphics, Camera camera)
        {
            // in some rare cases, there are SVG elements outside of a group so we'll render those cases first
            RenderRects(graphics.Batcher, SvgDoc.Rectangles);
            RenderPaths(graphics.Batcher, SvgDoc.Paths);
            RenderLines(graphics.Batcher, SvgDoc.Lines);
            RenderCircles(graphics.Batcher, SvgDoc.Circles);
            RenderPolygons(graphics.Batcher, SvgDoc.Polygons);
            RenderPolylines(graphics.Batcher, SvgDoc.Polylines);
            RenderImages(graphics.Batcher, SvgDoc.Images);

            if (SvgDoc.Groups != null)
                foreach (var g in SvgDoc.Groups)
                {
                    RenderRects(graphics.Batcher, g.Rectangles);
                    RenderPaths(graphics.Batcher, g.Paths);
                    RenderLines(graphics.Batcher, g.Lines);
                    RenderCircles(graphics.Batcher, g.Circles);
                    RenderPolygons(graphics.Batcher, g.Polygons);
                    RenderPolylines(graphics.Batcher, g.Polylines);
                    RenderImages(graphics.Batcher, g.Images);
                }
        }


        private void RenderRects(Batcher.Batcher batcher, SvgRectangle[] rects)
        {
            if (rects == null)
                return;

            foreach (var rect in rects)
            {
                var fixedPts = rect.GetTransformedPoints();
                for (var i = 0; i < fixedPts.Length - 1; i++)
                    batcher.DrawLine(fixedPts[i], fixedPts[i + 1], rect.StrokeColor, rect.StrokeWidth);
                batcher.DrawLine(fixedPts[3], fixedPts[0], rect.StrokeColor, rect.StrokeWidth);
            }
        }


        private void RenderPaths(Batcher.Batcher batcher, SvgPath[] paths)
        {
            if (paths == null && _pathBuilder != null)
                return;

            foreach (var path in paths)
            {
                var points = path.GetTransformedDrawingPoints(_pathBuilder, 3);
                for (var i = 0; i < points.Length - 1; i++)
                {
                    batcher.DrawLine(points[i], points[i + 1], path.StrokeColor, path.StrokeWidth);
                    batcher.DrawPixel(points[i], Color.Yellow, 3);
                }
            }
        }


        private void RenderLines(Batcher.Batcher batcher, SvgLine[] lines)
        {
            if (lines == null)
                return;

            foreach (var line in lines)
            {
                var fixedPts = line.GetTransformedPoints();
                batcher.DrawLine(fixedPts[0], fixedPts[1], line.StrokeColor, line.StrokeWidth);
            }
        }


        private void RenderCircles(Batcher.Batcher batcher, SvgCircle[] circles)
        {
            if (circles == null)
                return;

            foreach (var circ in circles)
                batcher.DrawCircle(circ.CenterX, circ.CenterY, circ.Radius, circ.StrokeColor, (int) circ.StrokeWidth);
        }


        private void RenderPolygons(Batcher.Batcher batcher, SvgPolygon[] polygons)
        {
            if (polygons == null)
                return;

            foreach (var poly in polygons)
            {
                var fixedPts = poly.GetTransformedPoints();
                for (var i = 0; i < fixedPts.Length - 1; i++)
                    batcher.DrawLine(fixedPts[i], fixedPts[i + 1], poly.StrokeColor, poly.StrokeWidth);
            }
        }


        private void RenderPolylines(Batcher.Batcher batcher, SvgPolyline[] polylines)
        {
            if (polylines == null)
                return;

            foreach (var poly in polylines)
            {
                var fixedPts = poly.GetTransformedPoints();
                for (var i = 0; i < fixedPts.Length - 1; i++)
                    batcher.DrawLine(fixedPts[i], fixedPts[i + 1], poly.StrokeColor, poly.StrokeWidth);
            }
        }


	    /// <summary>
	    ///     attempts to load and draw the SvgImage. If it cannot load a Texture it will just draw a rect.
	    /// </summary>
	    /// <param name="batcher">Batcher.</param>
	    /// <param name="images">Images.</param>
	    private void RenderImages(Batcher.Batcher batcher, SvgImage[] images)
        {
            if (images == null)
                return;

            foreach (var image in images)
            {
                var tex = image.GetTexture(Entity.Scene.Content);
                if (tex != null)
                {
                    var rotation = image.RotationDegrees * Mathf.Deg2Rad;
                    if (rotation == 0)
                    {
                        batcher.Draw(tex, image.Rect);
                    }
                    else
                    {
                        var rect = image.Rect;
                        var origin = new Vector2(0.5f * rect.Width, 0.5f * rect.Height);
                        rect.Location += origin;

                        batcher.Draw(tex, rect, null, Color.White, rotation, origin, SpriteEffects.None, layerDepth);
                    }
                }
                else
                {
                    batcher.DrawRect(image.X, image.Y, image.Width, image.Height, Color.DarkRed);
                }
            }
        }
    }
}