using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhotoFrames {
    internal struct FastInterpolater {
        private readonly int numerator;
        private readonly int denominator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal FastInterpolater(int inCount, int outCount) {
            numerator = outCount - 1;
            denominator = inCount - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int Map(int x) 
            => (x* numerator) / denominator;
    }
}
