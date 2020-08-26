using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PhotoFrames {
    internal sealed class UnmanagedBlock : IDisposable {
        private bool disposed;

        public IntPtr Pointer { get; private set; }
        public int ByteCount { get; private set; }

        public UnmanagedBlock(int size) 
            => Pointer = Marshal.AllocHGlobal(ByteCount = size);

        ~UnmanagedBlock() 
            => Dispose();

        public void Dispose() {
            if (disposed) return;
            Marshal.FreeHGlobal(Pointer);
            GC.SuppressFinalize(this);
            disposed = true;
        }

        public unsafe void* ToPointer() => Pointer.ToPointer();
        public int ToInt32() => Pointer.ToInt32();
        public long ToInt64() => Pointer.ToInt64();
        public override string ToString() => Pointer.ToString();
        public static implicit operator IntPtr(UnmanagedBlock x) => x.Pointer;

        internal unsafe void Zero() {
            Unsafe.InitBlockUnaligned(ToPointer(), 0, (uint)ByteCount);
        }

        /// <summary>
        /// Grows the buffer if it is smaller than size.
        /// </summary>
        /// <param name="size">Minimum required size.</param>
        /// <param name="roundUpPow2">Whether to round up to the next power of 2 if growing is necessary.</param>
        public void EnsureCapacity(int size, bool roundUpPow2 = false) {
            int RoundUpPow2(uint x)
                => 1 << (sizeof(uint) * 8 - BitOperations.LeadingZeroCount(x - 1));

            if (ByteCount < size) {
                ByteCount = roundUpPow2 ? RoundUpPow2((uint)size) : size;
                Pointer = Marshal.ReAllocHGlobal(Pointer, (IntPtr)ByteCount);
            }
        }

    }
}
