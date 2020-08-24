using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PhotoFrames {
    internal sealed class UnmanagedBlock : IDisposable {
        private bool disposed;

        public IntPtr Pointer { get; }
        public int ByteCount { get; }

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
    }
}
