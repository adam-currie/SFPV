using System;
using System.Windows.Forms;

/// <summary>
/// https://tyrrrz.me/blog/wndproc-in-wpf
/// </summary>
namespace SimpleFrame {
    public sealed class SpongeWindow : NativeWindow {
        public event EventHandler<Message>? WndProcCalled;

        public SpongeWindow() {
            CreateHandle(new CreateParams());
        }

        protected override void WndProc(ref Message m) {
            WndProcCalled?.Invoke(this, m);
            base.WndProc(ref m); // don't forget this line
        }
    }
}
