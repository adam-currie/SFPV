using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SimpleFrame {
    public static class WmCopyDataHelper {

        public const int WM_COPYDATA = 0x004A;

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr Hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        public static void SendString(IntPtr hwnd, string msg) {
            IntPtr _stringMessageBuffer = Marshal.StringToHGlobalUni(msg);

            COPYDATASTRUCT data = new COPYDATASTRUCT();
            data.dwData = IntPtr.Zero;
            data.lpData = _stringMessageBuffer;
            data.cbData = msg.Length * 2;

            IntPtr buff = Marshal.AllocHGlobal(Marshal.SizeOf(data));
            Marshal.StructureToPtr(data, buff, false);

            SendMessage(hwnd, WM_COPYDATA, IntPtr.Zero, buff);

            Marshal.FreeHGlobal(buff);
        }

        public static string ReadString(Message e) {
            COPYDATASTRUCT data = Marshal.PtrToStructure<COPYDATASTRUCT>(e.LParam);
            return Marshal.PtrToStringUni(data.lpData, data.cbData / 2);
        }
    }
}
