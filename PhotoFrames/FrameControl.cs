using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
            get => (Size)GetValue(MinimumContentSizeProperty);
            set => SetValue(MinimumContentSizeProperty, value);
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

            if (!above && !below && !left && !right)
                return;

            resize = new ResizeOperation(p, contentRect, above, below, left, right);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e) {
            if (resize != null) {
                resize = null;
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e) {
            if (resize != null) {
                Rect rect = resize.Evaluate(e.GetPosition(this));
                ContentSize = new Size(rect.Width, rect.Height);
                //todo: move the window
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
            int bytesPerPixel = (Frame.Format.BitsPerPixel + 7) / 8;
            int bufferStride = pixelWidth * bytesPerPixel;
            int bufferSize = pixelHeight * bufferStride;

            IntPtr bufHPtr = Marshal.AllocHGlobal(bufferStride * pixelHeight);
            try {
                unsafe {
                    byte* buf = (byte*)bufHPtr.ToPointer();

                    //todo: offsets
                    //todo: repeating

                    DrawCorner(Frame.TopLeft, buf, 0, 0, bytesPerPixel, bufferStride);
                    DrawCorner(Frame.TopRight, buf, pixelWidth - Frame.RightMargin, 0, bytesPerPixel, bufferStride);
                    DrawCorner(Frame.BottomLeft, buf, 0, pixelHeight - Frame.BottomMargin, bytesPerPixel, bufferStride);
                    DrawCorner(Frame.BottomRight, buf, pixelWidth - Frame.RightMargin, pixelHeight - Frame.BottomMargin, bytesPerPixel, bufferStride);

                    DrawHorizontalSide(Frame.Top, buf, Frame.LeftMargin, 0, (int)contentRect.Width, bytesPerPixel, bufferStride);
                    DrawHorizontalSide(Frame.Bottom, buf, Frame.LeftMargin, pixelHeight - Frame.BottomMargin, (int)contentRect.Width, bytesPerPixel, bufferStride);

                    DrawVerticalSide(Frame.Left, buf, 0, Frame.TopMargin, (int)contentRect.Height, bytesPerPixel, bufferStride);
                    DrawVerticalSide(Frame.Right, buf, pixelWidth - Frame.RightMargin, Frame.TopMargin, (int)contentRect.Height, bytesPerPixel, bufferStride);

                }
                var bmp = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, Frame.Format, Frame.Palette, bufHPtr, bufferSize, bufferStride);
                dc.DrawImage(bmp, new Rect(RenderSize));
            } finally {
                Marshal.FreeHGlobal(bufHPtr);
            }
        }

        private static unsafe void DrawHorizontalSide(FrameData.Section s, byte* dest, int destXStart, int destYStart, int writeWidth, int bytesPerPixel, int destStride)
            => DrawPixelsStrechX((byte*)s.Pixels.ToPointer(), dest, destXStart, destYStart, s.Width, s.Height, writeWidth, bytesPerPixel, destStride);

        private static unsafe void DrawVerticalSide(FrameData.Section s, byte* dest, int destXStart, int destYStart, int writeHeight, int bytesPerPixel, int destStride)
            => DrawPixelsStrechY((byte*)s.Pixels.ToPointer(), dest, destXStart, destYStart, s.Width, s.Height, writeHeight, bytesPerPixel, destStride);

        private static unsafe void DrawCorner(FrameData.Section s, byte* dest, int destX, int destY, int bytesPerPixel, int destStride)
            => DrawPixels((byte*)s.Pixels.ToPointer(), dest, destX, destY, s.Width, s.Height, bytesPerPixel, destStride);

        private static unsafe void DrawPixelsStrechX(byte* src, byte* dest, int destXStart, int destYStart, int srcWidth, int srcHeight, int writeWidth, int bytesPerPixel, int destStride) {
            int srcStride = srcWidth * bytesPerPixel;
            if (writeWidth > srcWidth) {
                var extrapolater = new FastExtrapolater(srcWidth, writeWidth);
                for (int y = 0; y < srcHeight; y++) {
                    for (int x = 0; x < writeWidth; x++) {
                        extrapolater.Map(x, out int sampleXa, out int sampleXb);
                        for (int i = 0; i < bytesPerPixel; i++) {
                            dest[(y + destYStart) * destStride + (x + destXStart) * bytesPerPixel + i] = (byte)((
                                src[y * srcStride + sampleXa * bytesPerPixel + i] +
                                src[y * srcStride + sampleXb * bytesPerPixel + i]) / 2);
                }   }   }
            } else {
                var interp = new FastInterpolater(srcWidth, writeWidth);
                int prevDestIndex = 0;
                for (int y = 0; y < srcHeight; y++) {
                    var averager = new PixelAverager(bytesPerPixel);
                    for (int x = 0; x < srcWidth; x++) {
                        int destX = interp.Map(x);
                        int destIndex = (destX + destXStart) * bytesPerPixel + (y + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + destIndex);
                        prevDestIndex = destIndex;
                        averager.Add(src + y * srcStride + x * bytesPerPixel);
                    }
                    averager.WriteAverage(dest + prevDestIndex);
                }
            }
        }

        private static unsafe void DrawPixelsStrechY(byte* src, byte* dest, int destXStart, int destYStart, int srcWidth, int srcHeight, int writeHeight, int bytesPerPixel, int destStride) {
            int srcStride = srcWidth * bytesPerPixel;
            if (writeHeight > srcHeight) {
                var extrapolater = new FastExtrapolater(srcHeight, writeHeight);
                for (int y = 0; y < writeHeight; y++) {
                    extrapolater.Map(y, out int sampleYa, out int sampleYb);
                    for (int x = 0; x < srcWidth; x++) {
                        for (int i = 0; i < bytesPerPixel; i++) {
                            dest[(y + destYStart) * destStride + (x + destXStart) * bytesPerPixel + i] = (byte)((
                                src[sampleYa * srcStride + x * bytesPerPixel + i] +
                                src[sampleYb * srcStride + x * bytesPerPixel + i]) / 2);
                }   }   }
            } else {
                var interp = new FastInterpolater(srcHeight, writeHeight);
                int prevDestIndex = 0;
                for (int y = 0; y < srcHeight; y++) {
                    int destY = interp.Map(y);
                    var averager = new PixelAverager(bytesPerPixel);
                    for (int x = 0; x < srcWidth; x++) {
                        int destIndex = (x + destXStart) * bytesPerPixel + (destY + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + destIndex);
                        prevDestIndex = destIndex;
                        averager.Add(src + y * srcStride + x * bytesPerPixel);
                    }
                    averager.WriteAverage(dest + prevDestIndex);
                }
            }
        }

        private static unsafe void DrawPixels(byte* src, byte* dest, int destX, int destY, int srcWidth, int srcHeight, int bytesPerPixel, int destStride) {
            int srcStride = srcWidth * bytesPerPixel;
            for (int y = 0; y < srcHeight; y++)
                for (int x = 0; x < srcWidth; x++)
                    for (int i = 0; i < bytesPerPixel; i++)
                        dest[(y+destY)*destStride + (x+destX)*bytesPerPixel + i] 
                            = src[y*srcStride + x*bytesPerPixel + i];
        }

        private static Size Max(Size a, Size b)
            => new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));

    }
}
