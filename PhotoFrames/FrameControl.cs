using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoFrames {

    //todo: introduce way for user to shrink the frame to fit the size of its content (just set ContentSize to the value returned by GetVisualChild(0).Measure)
    public class FrameControl : ContentControl {
        //used with ui dimensions, as long as dimensions are less than 2^23 pixels we wont overflow
        private const int BIG_FACTOR = 2 ^ 8;

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
                    DrawCorner(Frame.TopRight, buf, pixelWidth-Frame.RightMargin, 0, bytesPerPixel, bufferStride);
                    DrawCorner(Frame.BottomLeft, buf, 0, pixelHeight - Frame.BottomMargin, bytesPerPixel, bufferStride);
                    DrawCorner(Frame.BottomRight, buf, pixelWidth - Frame.RightMargin, pixelHeight - Frame.BottomMargin, bytesPerPixel, bufferStride);

                    int contentWidth = pixelWidth - (Frame.RightMargin + Frame.LeftMargin);
                    int contentHeight = pixelHeight - (Frame.TopMargin + Frame.BottomMargin);

                    DrawHorizontalSide(Frame.Top, buf, Frame.LeftMargin, 0, contentWidth, bytesPerPixel, bufferStride);
                    DrawHorizontalSide(Frame.Bottom, buf, Frame.LeftMargin, pixelHeight - Frame.BottomMargin, contentWidth, bytesPerPixel, bufferStride);

                    //todo: vert sizes

                }
                var bmp = BitmapSource.Create(pixelWidth, pixelHeight, 96, 96, Frame.Format, Frame.Palette, bufHPtr, bufferSize, bufferStride);
                dc.DrawImage(bmp, new Rect(RenderSize));
            } finally {
                Marshal.FreeHGlobal(bufHPtr);
            }
        }

        private static unsafe void DrawHorizontalSide(FrameData.Section s, byte* dest, int destXStart, int destYStart, int writeWidth, int bytesPerPixel, int destStride)
            => DrawPixelsStrechX((byte*)s.Pixels.ToPointer(), dest, destXStart, destYStart, s.Width, s.Height, writeWidth, bytesPerPixel, destStride);

        private static unsafe void DrawCorner(FrameData.Section s, byte* dest, int destX, int destY, int bytesPerPixel, int destStride)
            => DrawPixels((byte*)s.Pixels.ToPointer(), dest, destX, destY, s.Width, s.Height, bytesPerPixel, destStride);

        private static unsafe void DrawPixelsStrechX(byte* src, byte* dest, int destXStart, int destYStart, int srcWidth, int srcHeight, int writeWidth, int bytesPerPixel, int destStride) {
            int srcStride = srcWidth * bytesPerPixel;

            if (writeWidth > srcWidth) {//todo: test using this for interpolation as well, and test something more simple for it too
                GetExtrapolationFactor(out int numerator, out int denominator, srcWidth - 1, writeWidth - 1);
                for (int y = 0; y < srcHeight; y++) {
                    for (int x = 0; x < writeWidth; x++) {
                        int n = (x * numerator) / denominator;
                        int sampleXa = n / 2;
                        int sampleXb = (n + 1) / 2;

                        for (int i = 0; i < bytesPerPixel; i++) {
                            byte outByte = (byte)(
                                    (src[y * srcStride + sampleXa * bytesPerPixel + i]
                                    +
                                    src[y * srcStride + sampleXb * bytesPerPixel + i])
                                    / 2);

                            dest[(y + destYStart) * destStride + (x + destXStart) * bytesPerPixel + i] = outByte;
                        }
                    }
                }
            } else {//todo: decide which also is better for writeWidth == srcWidth
                //INTERPOLATE  (writeWidth <= srcWidth)
                int numerator = writeWidth - 1;
                int denominator = srcWidth - 1;

                for (int y = 0; y < srcHeight; y++) {
                    int[] byteSums = new int[bytesPerPixel];
                    int sumCount = 0;
                    int destXIndex = destXStart;
                    for (int x = 0; x < srcWidth; x++) {
                        int n = (x * numerator) / denominator + destXStart;
                        if(n != destXIndex) {
                            //index changed, time to average, if sumCount is zero nothing happens
                            WriteAverage(byteSums, dest, sumCount, destXIndex * bytesPerPixel, (y + destYStart) * destStride, bytesPerPixel);
                            sumCount = 0;
                            destXIndex = n;
                        }

                        //add to sums to be averaged later
                        for (int i = 0; i < bytesPerPixel; i++) {
                            byteSums[i] += src[y * srcStride + x * bytesPerPixel + i];
                        }
                        sumCount++;
                    }
                    WriteAverage(byteSums, dest, sumCount, destXIndex * bytesPerPixel, (y + destYStart) * destStride, bytesPerPixel);
                }
            }
        }

        //todo: ensure inlining
        private static unsafe void WriteAverage(int[] byteSums, byte* dest, int sumCount, int destXIndex, int destYIndex, int bytesPerPixel) {
            for (int i = 0; i < bytesPerPixel; i++) {
                dest[destYIndex + destXIndex + i]
                    = (byte)(byteSums[i] / sumCount);
                byteSums[i] = 0;
            }
        }

        private static void GetExtrapolationFactor(out int numerator, out int denominator, int maxInIndex, int maxOutIndex) {
            /*
             *  a = input max index
             *  b = output max index
             *  x = output index
             *  0 < n < 1 but infinitively close to 1 is ideal
             *      
             *            x(2a + n)
             *       y = ------------
             *                b  
             *      
             *  if y is even you sample input at y/2
             *  if y is odd you sample input at y/2 and y/2+1 and average them
             *  
             *  in order to avoid branching the code is going to treat both
             *  cases the same and sample the same pixel twice when y is even
             *  
             *  because n has to be an integer and we don't want it to be 0,
             *  we can simulate a number closer to 1 using a large factor:
             *      
             *       x(2a + .99)       x(200a + 99)
             *      -----------  =  -----------
             *           b               100b
             */
            numerator = 2 * (maxInIndex - 1) * BIG_FACTOR + (BIG_FACTOR - 1);
            denominator = (maxOutIndex - 1) * BIG_FACTOR;
        }

        private static unsafe void DrawPixels(byte* src, byte* dest, int destX, int destY, int srcWidth, int srcHeight, int bytesPerPixel, int destStride) {
            int srcStride = srcWidth * bytesPerPixel;
            for (int y = 0; y < srcHeight; y++) {
                for (int x = 0; x < srcWidth; x++) {
                    for (int i = 0; i < bytesPerPixel; i++) {
                        dest[(y+destY)*destStride + (x+destX)*bytesPerPixel + i] 
                            = src[y*srcStride + x*bytesPerPixel + i];
                    }
                }
            }
        }

        private static Size Max(Size a, Size b)
            => new Size(Math.Max(a.Width, b.Width), Math.Max(a.Height, b.Height));

    }
}
