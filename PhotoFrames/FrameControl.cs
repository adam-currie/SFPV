using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoFrames {

    //todo: introduce way for user to shrink the frame to fit the size of its content (just set ContentSize to the value returned by GetVisualChild(0).Measure)
    public class FrameControl : ContentControl {

        //set in arrange pass
        private Rect contentRect = new Rect();

        private ResizeOperation? resize = null;
        private MouseDevice? resizeDevice;

        private UnmanagedBlock renderBuf = new UnmanagedBlock(0);

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
                new Size(0, 0),
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
                propertyChangedCallback: (d, e)
                    => d.CoerceValue(ContentSizeProperty)));

        public FrameData? Frame {
            get => (FrameData?)GetValue(FrameProperty);
            set => SetValue(FrameProperty, value);
        }

        public Size MinimumContentSize {
            get => (Size)GetValue(MinimumContentSizeProperty);
            set => SetValue(MinimumContentSizeProperty, value);
        }

        public Size ContentSize {
            get => (Size)GetValue(ContentSizeProperty);
            set => SetValue(ContentSizeProperty, value);
        }

        protected override Size MeasureOverride(Size constraint) {
            var child = (UIElement)GetVisualChild(0);

            child.Measure(ContentSize);

            return Frame == null ? ContentSize : new Size(
                ContentSize.Width + Frame.LeftMargin + Frame.RightMargin,
                ContentSize.Height + Frame.TopMargin + Frame.BottomMargin);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            var child = (UIElement)(GetVisualChild(0));

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

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            Point p = e.GetPosition(this);

            bool above = p.Y < contentRect.Top;
            bool below = p.Y > contentRect.Bottom;
            bool left = p.X < contentRect.Left;
            bool right = p.X > contentRect.Right;

            if (above || left) {
                Window.GetWindow(this).DragMove();
                e.Handled = true;
            } else if (below || right) {
                resize = new ResizeOperation(p, contentRect, false, below, false, right);
                resizeDevice = e.MouseDevice;
                resizeDevice.Capture(this);
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e) {
            if (resize != null) {
                resize = null;
                resizeDevice!.Capture(this, CaptureMode.None);
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e) {
            if (resize != null) {
                Rect rect = resize.Evaluate(e.GetPosition(this));
                ContentSize = new Size(rect.Width, rect.Height);
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e) {
            if (!contentRect.Contains(e.GetPosition(this))) {
                //todo: context menu
                e.Handled = true;
            }
        }

        protected override void OnRender(DrawingContext dc) {
            if (Frame == null)
                return;

            int pixelWidth = (int)RenderSize.Width;
            int pixelHeight = (int)RenderSize.Height;
            int bufferStride = pixelWidth * Frame.BytesPerPixel;
            int bufferSize = pixelHeight * bufferStride;

            renderBuf.Zero();
            renderBuf.EnsureCapacity(bufferSize, true);

            unsafe {
                byte* buf = (byte*)renderBuf.ToPointer();

                FrameRendering.DrawCorner(Frame.TopLeft, buf, 0, 0, bufferStride);
                FrameRendering.DrawCorner(Frame.TopRight, buf, pixelWidth - Frame.RightMargin, 0, bufferStride);
                FrameRendering.DrawCorner(Frame.BottomLeft, buf, 0, pixelHeight - Frame.BottomMargin, bufferStride);
                FrameRendering.DrawCorner(Frame.BottomRight, buf, pixelWidth - Frame.RightMargin, pixelHeight - Frame.BottomMargin, bufferStride);

                FrameRendering.DrawHorizontalSide(Frame.Top, buf, Frame.LeftMargin, 0, (int)contentRect.Width, bufferStride);
                FrameRendering.DrawHorizontalSide(Frame.Bottom, buf, Frame.LeftMargin, pixelHeight - Frame.BottomMargin, (int)contentRect.Width, bufferStride);

                FrameRendering.DrawVerticalSide(Frame.Left, buf, 0, Frame.TopMargin, (int)contentRect.Height, bufferStride);
                FrameRendering.DrawVerticalSide(Frame.Right, buf, pixelWidth - Frame.RightMargin, Frame.TopMargin, (int)contentRect.Height, bufferStride);
            }

            var bmp = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, Frame.Format, Frame.Palette, renderBuf, bufferSize, bufferStride);
            dc.DrawImage(bmp, new Rect(RenderSize));
        }

        private static Size Max(Size a, Size b)
            => new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));

    }
}
