﻿using HelpersLib;
using System;
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
    public class CanvasEditor : Canvas
    {
        public AnnotationMode AnnotationMode { get; private set; } = AnnotationMode.None;

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ImageEx), typeof(CanvasEditor), new FrameworkPropertyMetadata(ImagePropertyChangedCallback));

        [Category("Editor")]
        public ImageEx CapturedImage
        {
            get { return (ImageEx)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private Point pStart;
        private Annotatation currentAnnotation;

        private static void ImagePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CanvasEditor obj = d as CanvasEditor;
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

        public void Init(AnnotationMode mode)
        {
            AnnotationMode = mode;
            AnnotationHelper.LoadCapturedImage(CapturedImage);
        }

        private void RedrawAnnotations()
        {
            if (CapturedImage.Annotations == null) { return; }

            RemoveAllAnnotations();

            foreach (var ann in CapturedImage.Annotations)
            {
                if (ann.GetType() == typeof(HighlightAnnotation))
                {
                    HighlightAnnotation hl = ann as HighlightAnnotation;
                    // AddShape(hl, hl.X1, hl.Y1, hl.Width, hl.Height);
                }
                else if (ann.GetType() == typeof(ObfuscateAnnotation))
                {
                    ObfuscateAnnotation obf = ann as ObfuscateAnnotation;
                    //  AddShape(obf, obf.X1, obf.Y1, obf.Width, obf.Height);
                }
            }
        }

        public void RemoveAllAnnotations()
        {
            if (CapturedImage.Annotations == null) { return; }
            Children.RemoveRange(0, Children.Count);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (AnnotationMode == AnnotationMode.None) { return; };

            base.OnMouseLeftButtonDown(e);
            pStart = e.GetPosition(this);

            switch (AnnotationMode)
            {
                case AnnotationMode.Highlight:
                    currentAnnotation = new HighlightAnnotation();
                    break;
                case AnnotationMode.Obfuscate:
                    currentAnnotation = new ObfuscateAnnotation();
                    break;
                default:
                    throw new NotImplementedException();
            }

            currentAnnotation.X1 = pStart.X;
            currentAnnotation.Y1 = pStart.Y;

            SetLeft(currentAnnotation, pStart.X);
            SetTop(currentAnnotation, pStart.Y);

            Children.Add(currentAnnotation);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (AnnotationMode == AnnotationMode.None) { return; };

            base.OnMouseUp(e);

            Point pFinish = e.GetPosition(this);
            currentAnnotation.X2 = pFinish.X;
            currentAnnotation.Y2 = pFinish.Y;

            currentAnnotation.Render();

            CapturedImage.Annotations.Add(currentAnnotation);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (AnnotationMode == AnnotationMode.None)
                return;

            if (e.LeftButton == MouseButtonState.Released || currentAnnotation == null)
                return;

            base.OnMouseMove(e);

            var pos = e.GetPosition(this);
            var x = Math.Min(pos.X, pStart.X);
            var y = Math.Min(pos.Y, pStart.Y);
            var w = Math.Max(pos.X, pStart.X) - x;
            var h = Math.Max(pos.Y, pStart.Y) - y;

            currentAnnotation.Width = w;
            currentAnnotation.Height = h;

            SetLeft(currentAnnotation, x);
            SetTop(currentAnnotation, y);
        }
    }
}