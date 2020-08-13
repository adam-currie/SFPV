using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PhotoFrames {

    //todo: introduce way for user to shrink the frame to fit the size of its content (just set ContentSize to the value returned by GetVisualChild(0).Measure)
    public class FrameControl : ContentControl {

        public static readonly DependencyProperty FrameProperty =
            DependencyProperty.Register(
            "Frame", 
            typeof(FrameData),
            typeof(FrameControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty ContentSizeProperty =
            DependencyProperty.Register(
            "ContentSize",
            typeof(Size),
            typeof(FrameControl),
            new FrameworkPropertyMetadata(
                new Size(0,0),
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                (d, e) => { },
                coerceValueCallback: (d, value) 
                    => Max((Size)value, ((FrameControl)d).MinimumContentSize)));

        public static readonly DependencyProperty MinimumContentSizeProperty =
            DependencyProperty.Register(
            "MinimumContentSize",
            typeof(Size),
            typeof(FrameControl),
            new FrameworkPropertyMetadata(
                new Size(0, 0), 
                propertyChangedCallback: (d,e) 
                    => d.CoerceValue(ContentSizeProperty)));

        public FrameData? Frame {
            get => (FrameData?)GetValue(FrameProperty);
            set => SetValue(FrameProperty, value);
        }

        public Size MinimumContentSize {
            get => (Size)GetValue(ContentSizeProperty);
            set => SetValue(ContentSizeProperty, value);
        }

        public Size ContentSize {
            get => (Size)GetValue(ContentSizeProperty);
            set => SetValue(ContentSizeProperty, value);
        }

        protected override Size MeasureOverride(Size constraint) {
            var child = (UIElement)(GetVisualChild(0));

            child.Measure(ContentSize);

            return Frame == null ? ContentSize : new Size(
                ContentSize.Width + Frame.LeftMargin + Frame.RightMargin,
                ContentSize.Height + Frame.TopMargin + Frame.BottomMargin);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            var child = (UIElement)(GetVisualChild(0));

            var contentRect = new Rect();
            if (Frame != null) {
                contentRect.X = Frame.LeftMargin;
                contentRect.Y = Frame.TopMargin;
                contentRect.Width = arrangeBounds.Width - (Frame.LeftMargin + Frame.RightMargin);
                contentRect.Height = arrangeBounds.Height - (Frame.TopMargin + Frame.BottomMargin);
            } else {
                contentRect.Width = arrangeBounds.Width;
                contentRect.Height = arrangeBounds.Height;
            }

            child.Arrange(contentRect);

            return arrangeBounds;
        }

        protected override void OnRender(DrawingContext drawingContext) {
            //todo
        }

        private static Size Max(Size a, Size b)
            => new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));

    }
}
