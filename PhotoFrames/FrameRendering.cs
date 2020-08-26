using System.Runtime.CompilerServices;

namespace PhotoFrames {
    public static class FrameRendering {

        public static unsafe void DrawHorizontalSide(FrameData.Section s, byte* dest, int destXStart, int destYStart, int writeWidth, int destStride) {
            var src = (byte*)s.Pixels.ToPointer();
            int srcStride = s.Width * s.BytesPerPixel;

            if (s.Repeating) {
                int repeatCount = (writeWidth + s.Width - 1) / s.Width;
                var interp = new SimpleResampler(s.Width*repeatCount, writeWidth);
                int prevDestIndex = 0;
                var averager = new PixelAverager(s.BytesPerPixel);
                for (int y = 0; y < s.Height; y++) {
                    for (int x = 0; x < s.Width*repeatCount; x++) {
                        int destX = interp.Map(x);
                        int destIndex = (destX + destXStart) * s.BytesPerPixel + (y + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + prevDestIndex);
                        prevDestIndex = destIndex;
                        averager.Add(src + y * srcStride + (x%s.Width) * s.BytesPerPixel);
                    }
                    averager.WriteAverage(dest + prevDestIndex);
                }
            } else if (writeWidth > s.Width) {
                var extrapolater = new FastUpsampler(s.Width, writeWidth);
                for (int x = 0; x < writeWidth; x++) {
                    extrapolater.Map(x, out int sampleXa, out int sampleXb);
                    for (int y = 0; y < s.Height; y++) {
                        for (int i = 0; i < s.BytesPerPixel; i++)
                            dest[(y + destYStart) * destStride + (x + destXStart) * s.BytesPerPixel + i] = (byte)((
                                src[y * srcStride + sampleXa * s.BytesPerPixel + i] +
                                src[y * srcStride + sampleXb * s.BytesPerPixel + i]) / 2);
                    }
                }
            } else {
                var interp = new SimpleResampler(s.Width, writeWidth);
                int prevDestIndex = 0;
                var averager = new PixelAverager(s.BytesPerPixel);
                for (int y = 0; y < s.Height; y++) {
                    for (int x = 0; x < s.Width; x++) {
                        int destX = interp.Map(x);
                        int destIndex = (destX + destXStart) * s.BytesPerPixel + (y + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + prevDestIndex);
                        prevDestIndex = destIndex;
                        averager.Add(src + y * srcStride + x * s.BytesPerPixel);
                    }
                    averager.WriteAverage(dest + prevDestIndex);
                }
            }
        }

        public static unsafe void DrawVerticalSide(FrameData.Section s, byte* dest, int destXStart, int destYStart, int writeHeight, int destStride) {
            var src = (byte*)s.Pixels.ToPointer();
            int srcStride = s.Width * s.BytesPerPixel;

            if (s.Repeating) {
                int repeatCount = (writeHeight + s.Height - 1) / s.Height;
                var interp = new SimpleResampler(s.Height*repeatCount, writeHeight);
                int prevDestIndex = 0;
                var averager = new PixelAverager(s.BytesPerPixel);
                for (int y = 0; y < s.Height*repeatCount; y++) {
                    int destY = interp.Map(y);
                    for (int x = 0; x < s.Width; x++) {
                        int destIndex = (x + destXStart) * s.BytesPerPixel + (destY + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + prevDestIndex);
                        prevDestIndex = destIndex;
                        averager.Add(src + (y%s.Height) * srcStride + x * s.BytesPerPixel);
                    }
                    averager.WriteAverage(dest + prevDestIndex);
                }
            } else if (writeHeight > s.Height) {
                var extrapolater = new FastUpsampler(s.Height, writeHeight);
                for (int y = 0; y < writeHeight; y++) {
                    extrapolater.Map(y, out int sampleYa, out int sampleYb);
                    for (int x = 0; x < s.Width; x++) {
                        for (int i = 0; i < s.BytesPerPixel; i++)
                            dest[(y + destYStart) * destStride + (x + destXStart) * s.BytesPerPixel + i] = (byte)((
                                src[sampleYa * srcStride + x * s.BytesPerPixel + i] +
                                src[sampleYb * srcStride + x * s.BytesPerPixel + i]) / 2);
                    }
                }
            } else {
                var interp = new SimpleResampler(s.Height, writeHeight);
                int prevDestIndex = 0;
                var averager = new PixelAverager(s.BytesPerPixel);
                for (int y = 0; y < s.Height; y++) {
                    int destY = interp.Map(y);
                    for (int x = 0; x < s.Width; x++) {
                        int destIndex = (x + destXStart) * s.BytesPerPixel + (destY + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + prevDestIndex);
                        prevDestIndex = destIndex;
                        averager.Add(src + y * srcStride + x * s.BytesPerPixel);
                    }
                    averager.WriteAverage(dest + prevDestIndex);
                }
            }
        }

        public static unsafe void DrawCorner(FrameData.Section s, byte* dest, int destX, int destY, int destStride) {
            var src = (byte*)s.Pixels.ToPointer();
            int srcStride = s.Width * s.BytesPerPixel;
            for (int y = 0; y < s.Height; y++)
            for (int x = 0; x < s.Width; x++)
            for (int i = 0; i < s.BytesPerPixel; i++)
                dest[(y + destY) * destStride + (x + destX) * s.BytesPerPixel + i]
                    = src[y * srcStride + x * s.BytesPerPixel + i];
        }

    }
}
