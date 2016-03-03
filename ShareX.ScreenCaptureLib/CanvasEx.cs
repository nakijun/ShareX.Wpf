﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShareX.ScreenCaptureLib
{
    public class CanvasEx : Canvas
    {
        public AnnotationMode AnnotationMode { get; private set; } = AnnotationMode.None;

        public static readonly DependencyProperty ImageProperty;

        [Category("Editor")]
        public ImageEx Image
        {
            get { return (ImageEx)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        private Point _startPoint;
        private Rectangle _currentRectangle;
        private Annotate _annotationBeingAdded;

        static CanvasEx()
        {
            ImageProperty = DependencyProperty.Register("Image", typeof(ImageEx), typeof(CanvasEx), new FrameworkPropertyMetadata(ImagePropertyChangedCallback));
        }

        private static void ImagePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CanvasEx obj = d as CanvasEx;
            ImageEx img = e.NewValue as ImageEx;
            if (img == null)
            {
                obj.Background = null;
                return;
            }
            obj.Width = img.Source.Width;
            obj.Height = img.Source.Height;
            obj.Background = new ImageBrush(img.Source);
            obj.RedrawAnnotations();
        }

        public void SetAnnotationMode(AnnotationMode mode)
        {
            AnnotationMode = mode;

            switch (mode)
            {
                case AnnotationMode.Highlight:
                    _annotationBeingAdded = new Highlight();
                    break;
                case AnnotationMode.Obfuscate:
                    _annotationBeingAdded = new Obfuscate();
                    break;
            }
        }

        private void RedrawAnnotations()
        {
            if (Image.Annotations == null) { return; }

            RemoveAllAnnotations();

            foreach (var ann in Image.Annotations)
            {
                if (ann.GetType() == typeof(Highlight))
                {
                    Highlight hl = ann as Highlight;
                    AddRectangle(hl, hl.TopLeft.X, hl.TopLeft.Y, hl.Width, hl.Height);
                }
            }
        }

        public void RemoveAllAnnotations()
        {
            if (Image.Annotations == null) { return; }
            Children.RemoveRange(0, Children.Count);
        }

        private Rectangle AddRectangle(Annotate ann, double x, double y, double w = 0, double h = 0)
        {
            var r = ann.Render();

            SetLeft(r, x);
            SetTop(r, y);
            r.Width = w;
            r.Height = h;
            Children.Add(r);

            return r;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (AnnotationMode == AnnotationMode.None) { return; };

            base.OnMouseLeftButtonDown(e);
            _startPoint = e.GetPosition(this);

            _currentRectangle = AddRectangle(_annotationBeingAdded, _startPoint.X, _startPoint.Y);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (AnnotationMode == AnnotationMode.None) { return; };

            base.OnMouseUp(e);

            switch (AnnotationMode)
            {
                case AnnotationMode.Highlight:
                    _annotationBeingAdded = new Highlight
                    {
                        Width = _currentRectangle.Width,
                        Height = _currentRectangle.Height,
                        TopLeft = _startPoint
                    };
                    break;
                case AnnotationMode.Obfuscate:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            Image.Annotations.Add(_annotationBeingAdded);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (AnnotationMode == AnnotationMode.None)
                return;

            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Released || _currentRectangle == null)
                return;

            bool controlkey = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            var pos = e.GetPosition(this);
            var x = Math.Min(pos.X, _startPoint.X);
            var y = Math.Min(pos.Y, _startPoint.Y);
            var w = Math.Max(pos.X, _startPoint.X) - x;
            var h = Math.Max(pos.Y, _startPoint.Y) - y;

            if (controlkey)
            {
                //make square based on the smallest
                var sml = Math.Min(w, h);
                w = sml;
                h = sml;
            }
            _currentRectangle.Width = w;
            _currentRectangle.Height = h;

            SetLeft(_currentRectangle, x);
            SetTop(_currentRectangle, y);
        }
    }
}