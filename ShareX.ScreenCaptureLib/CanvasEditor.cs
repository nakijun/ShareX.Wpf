﻿using HelpersLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShareX.ScreenCaptureLib
{
    public class CanvasEditor : Canvas
    {
        public delegate void ImageLoadedEventHandler();
        public event ImageLoadedEventHandler ImageLoaded;

        public ImageCapture CapturedImage { get; private set; }

        private AnnotationMode annotationMode = AnnotationMode.Cursor;

        public AnnotationMode AnnotationMode
        {
            get
            {
                return annotationMode;
            }
            set
            {
                annotationMode = value;
                AnnotationHelper.LoadCapturedImage(CapturedImage);
            }
        }

        public ObservableCollection<Annotation> Annotations { get; set; } = new ObservableCollection<Annotation>();

        public bool IsCreatingAnnotation
        {
            get
            {
                return AnnotationMode != AnnotationMode.Cursor && currentAnnotation != null && currentAnnotation.IsCreating;
            }
        }

        private Annotation currentAnnotation;

        public CanvasEditor()
        {
            ClipToBounds = true;
        }

        protected virtual void OnImageLoaded()
        {
            if (ImageLoaded != null) ImageLoaded();
        }

        public void LoadImage(ImageCapture src)
        {
            CapturedImage = src;
            Children.Clear();
            ImageAnnotation ann = new ImageAnnotation(src.Source);
            ann.Selectable = false;
            Children.Add(ann);

            Width = CapturedImage.Source.Width;
            Height = CapturedImage.Source.Height;

            OnImageLoaded();
        }

        private Annotation CreateCurrentAnnotation()
        {
            Annotation annotation;

            switch (AnnotationMode)
            {
                case AnnotationMode.Highlight:
                    annotation = new HighlightAnnotation();
                    break;
                case AnnotationMode.Obfuscate:
                    annotation = new ObfuscateAnnotation();
                    break;
                case AnnotationMode.Rectangle:
                    annotation = new RectangleAnnotation();
                    break;
                case AnnotationMode.Ellipse:
                    annotation = new EllipseAnnotation();
                    break;
                case AnnotationMode.Line:
                    annotation = new LineAnnotation();
                    break;
                case AnnotationMode.Arrow:
                    annotation = new ArrowAnnotation();
                    break;
                default:
                    throw new NotImplementedException();
            }

            annotation.IsCreating = true;

            return annotation;
        }

        public void UnselectAll()
        {
            foreach (Annotation ann in Annotations)
            {
                ann.Selected = false;
            }
        }

        public void DeleteSelected()
        {
            foreach (Annotation ann in Annotations.Where(x => x.Selected).ToArray())
            {
                Annotations.Remove(ann);
                Children.Remove(ann);
            }
        }

        private void FinishCurrentAnnotation()
        {
            if (currentAnnotation != null && currentAnnotation.IsCreating)
            {
                currentAnnotation.IsCreating = false;
                currentAnnotation.Selected = true;
                Annotations.Add(currentAnnotation);
                currentAnnotation.Render();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left && AnnotationMode != AnnotationMode.Cursor)
            {
                UnselectAll();
                currentAnnotation = CreateCurrentAnnotation();
                currentAnnotation.PointStart = currentAnnotation.PointFinish = e.GetPosition(this);
                currentAnnotation.UpdateDimensions();

                Children.Add(currentAnnotation);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsCreatingAnnotation)
            {
                currentAnnotation.PointFinish = e.GetPosition(this);
                currentAnnotation.UpdateDimensions();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (IsCreatingAnnotation)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    currentAnnotation.PointFinish = e.GetPosition(this);
                    currentAnnotation.UpdateDimensions();
                    FinishCurrentAnnotation();
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    Children.Remove(currentAnnotation);
                    currentAnnotation = null;
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (e.LeftButton == MouseButtonState.Pressed && IsCreatingAnnotation)
            {
                FinishCurrentAnnotation();
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Delete)
            {
                DeleteSelected();
            }
        }

        public RenderTargetBitmap GetBitmap()
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)CapturedImage.Source.Width, (int)CapturedImage.Source.Height, CapturedImage.Source.DpiX, CapturedImage.Source.DpiY, PixelFormats.Pbgra32);

            foreach (var ann in Children)
            {
                rtb.Render(ann as UIElement);
            }

            return rtb;
        }

        public MemoryStream GetStream()
        {
            var stream = new MemoryStream();
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(GetBitmap()));
            encoder.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}