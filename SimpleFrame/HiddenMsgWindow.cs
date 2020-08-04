using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

namespace SimpleFrame {
    internal class HiddenMsgWindow : Window {

        /*
         * we need to id our window and look it up because hidden windows(no taskbar icon) 
         * cannot be the main window of a process
         */
        private const string TITLE = "HiddenMsgWindow";

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        private IntPtr hwnd;
        private bool wndProcInitialized = false;

        private event EventHandler<Message>? _WndProcCalled;
        public event EventHandler<Message>? WndProcCalled {
            add {
                if (!wndProcInitialized) InitializeWndProc();
                _WndProcCalled += value;
            }
            remove => _WndProcCalled -= value;
        }

        public HiddenMsgWindow() {
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            Title = TITLE;
        }


        private void InitializeWndProc() {
            if (wndProcInitialized) 
                return;

            hwnd = new WindowInteropHelper(this).EnsureHandle();
            var source = HwndSource.FromHwnd(hwnd);
            source.AddHook(WndProc);

            Closed += (s, e) => source.RemoveHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            _WndProcCalled?.Invoke(this, Message.Create(hwnd, msg, wParam, lParam));
            return IntPtr.Zero;
        }

        /// <summary>
        /// Finds the first message window in the target process.
        /// </summary>
        /// <param name="process">Process to look in.</param>
        /// <returns>The hWnd of the message window, or zero if none found.</returns>
        public static IntPtr FindMsgWindowForProcess(Process process) {
            IntPtr msgHwnd = IntPtr.Zero;

            EnumWindows(delegate (IntPtr hwnd, IntPtr param) {
                uint pid = 0;
                GetWindowThreadProcessId(hwnd, out pid);
                if (pid == process.Id) {
                    int length = GetWindowTextLength(hwnd);
                    StringBuilder sb = new StringBuilder(length + 1);
                    GetWindowText(hwnd, sb, sb.Capacity);
                    string text = sb.ToString();

                    if (text.Equals(TITLE)) {
                        msgHwnd = hwnd;
                        return false;
                    }
                }
                //keep enumerating
                return true;
            }, IntPtr.Zero);

            return msgHwnd;
        }
    }
}