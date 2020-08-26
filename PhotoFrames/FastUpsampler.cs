using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace PhotoFrames {

    /*
     *  a = input max index
     *  b = output max index
     *  x = output index
     *  0 < n < 1 but infinitely close to 1 is ideal
     *      
     *            x(2a + n)
     *       y = ------------
     *                b  
     *      
     *  if y is even you sample input at y/2
     *  if y is odd you sample input at y/2 and y/2 + 1 and average them
     *  
     *  in order to avoid branching the code is going to treat both
     *  cases the same and sample the same pixel twice when y is even
     *  
     *  because n has to be an integer and we don't want it to be 0,
     *  we can simulate a number closer to 1 using a large factor:
     *      
     *      x(2a + .99)     x(200a + 99)
     *      -----------  =  -----------
     *           b             100b
     */
    internal struct FastUpsampler {
        //as long as dimensions are less than 2^23 pixels we wont overflow
        private const int BIG_FACTOR = 2 ^ 8;

        private readonly int numerator;
        private readonly int denominator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal FastUpsampler(int inCount, int outCount) {
            numerator = 2 * (inCount - 2) * BIG_FACTOR + (BIG_FACTOR - 1);
            denominator = (outCount - 2) * BIG_FACTOR;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Map(int outputIndex, out int inputIndexA, out int inputIndexB) {
            int n = (outputIndex * numerator) / denominator;
            inputIndexA = n / 2;
            inputIndexB = (n + 1) / 2;
        }
    }
}
