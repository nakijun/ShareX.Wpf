﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

namespace ShareX.ScreenCaptureLib
{
    public class RectangleAnnotation : Annotation
    {
        public int ShadowSize = 10;

        protected override Geometry DefiningGeometry
        {
            get
            {
                return new RectangleGeometry(new Rect(0, 0, Math.Max(0, Width - ShadowSize), Math.Max(0, Height - ShadowSize)));
            }
        }

        public RectangleAnnotation()
        {
            brush = Brushes.Red;

            Stroke = brush;
            StrokeThickness = 1;
        }

        public override RenderTargetBitmap Render()
        {
            Effect = new DropShadowEffect
            {
                RenderingBias = RenderingBias.Quality,
                Opacity = 0.8d,
                Color = Colors.Black,
                ShadowDepth = 0,
                BlurRadius = ShadowSize,
                Direction = 45
            };

            return base.Render();
        }

        public override void Render(DrawingContext dc)
        {
            Render();
            dc.DrawRectangle(null, new Pen(brush, StrokeThickness), Area);
            // dc.DrawImage(Render(), Area);
        }
    }
}