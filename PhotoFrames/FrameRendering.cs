namespace PhotoFrames {
    public static class FrameRendering {

        public static unsafe void DrawHorizontalSide(FrameData.Section s, byte* dest, int destXStart, int destYStart, int writeWidth, int destStride)
            => DrawPixelsStrechX((byte*)s.Pixels.ToPointer(), dest, destXStart, destYStart, s.Width, s.Height, writeWidth, s.BytesPerPixel, destStride);

        public static unsafe void DrawVerticalSide(FrameData.Section s, byte* dest, int destXStart, int destYStart, int writeHeight, int destStride)
            => DrawPixelsStrechY((byte*)s.Pixels.ToPointer(), dest, destXStart, destYStart, s.Width, s.Height, writeHeight, s.BytesPerPixel, destStride);

        public static unsafe void DrawCorner(FrameData.Section s, byte* dest, int destX, int destY, int destStride)
            => DrawPixels((byte*)s.Pixels.ToPointer(), dest, destX, destY, s.Width, s.Height, s.BytesPerPixel, destStride);

        private static unsafe void DrawPixelsStrechX(byte* src, byte* dest, int destXStart, int destYStart, int srcWidth, int srcHeight, int writeWidth, int bytesPerPixel, int destStride) {
            int srcStride = srcWidth * bytesPerPixel;
            if (writeWidth > srcWidth) {
                var extrapolater = new FastUpsampler(srcWidth, writeWidth);
                for (int y = 0; y < srcHeight; y++) {
                for (int x = 0; x < writeWidth; x++) {
                    extrapolater.Map(x, out int sampleXa, out int sampleXb);
                    for (int i = 0; i < bytesPerPixel; i++)
                        dest[(y + destYStart) * destStride + (x + destXStart) * bytesPerPixel + i] = (byte)((
                            src[y * srcStride + sampleXa * bytesPerPixel + i] +
                            src[y * srcStride + sampleXb * bytesPerPixel + i]) / 2);
                }}
            } else {
                var interp = new FastDownsampler(srcWidth, writeWidth);
                int prevDestIndex = 0;
                for (int y = 0; y < srcHeight; y++) {
                    var averager = new PixelAverager(bytesPerPixel);
                    for (int x = 0; x < srcWidth; x++) {
                        int destX = interp.Map(x);
                        int destIndex = (destX + destXStart) * bytesPerPixel + (y + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + prevDestIndex);
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
                var extrapolater = new FastUpsampler(srcHeight, writeHeight);
                for (int y = 0; y < writeHeight; y++) {
                    extrapolater.Map(y, out int sampleYa, out int sampleYb);
                    for (int x = 0; x < srcWidth; x++) {
                    for (int i = 0; i < bytesPerPixel; i++)
                        dest[(y + destYStart) * destStride + (x + destXStart) * bytesPerPixel + i] = (byte)((
                            src[sampleYa * srcStride + x * bytesPerPixel + i] +
                            src[sampleYb * srcStride + x * bytesPerPixel + i]) / 2);
                    }
                }
            } else {
                var interp = new FastDownsampler(srcHeight, writeHeight);
                int prevDestIndex = 0;
                for (int y = 0; y < srcHeight; y++) {
                    int destY = interp.Map(y);
                    var averager = new PixelAverager(bytesPerPixel);
                    for (int x = 0; x < srcWidth; x++) {
                        int destIndex = (x + destXStart) * bytesPerPixel + (destY + destYStart) * destStride;
                        if (prevDestIndex != destIndex && averager.Count != 0)
                            averager.WriteAverage(dest + prevDestIndex);
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
                dest[(y + destY) * destStride + (x + destX) * bytesPerPixel + i]
                    = src[y * srcStride + x * bytesPerPixel + i];
        }

    }
}
