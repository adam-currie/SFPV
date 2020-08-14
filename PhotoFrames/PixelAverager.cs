using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhotoFrames {

    /// <summary>
    /// Averages an arbitrary number of pixels, requires that each pixel represents a separate channel.
    /// </summary>
    internal struct PixelAverager {
        private readonly int bytesPerPixel;
        private readonly int[] byteSums;
        private int sumCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PixelAverager(int bytesPerPixel){
            this.bytesPerPixel = bytesPerPixel;
            byteSums = new int[bytesPerPixel];
            sumCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void WriteAverage(byte* destination) {
            for (int i = 0; i < bytesPerPixel; i++) {
                destination[i] = (byte)(byteSums[i] / sumCount);
                byteSums[i] = 0;
            }
            sumCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void Add(byte* source) {
            for (int i = 0; i < bytesPerPixel; i++) {
                byteSums[i] += source[i];
            }
            sumCount++;
        }
    }
}
