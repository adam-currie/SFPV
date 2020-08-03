using SimpleFrame.DB;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using WinForms = System.Windows.Forms;

namespace SimpleFrame {

    public partial class App : Application {
        private const string LOCK_PATH = "lock";
        private const string ICON_PATH = "icon.ico";

        private WinForms.NotifyIcon? notifyIcon;//won't be null after initialization

        private ConcurrentPhotoWindowCollection photoWindows = new ConcurrentPhotoWindowCollection();
        private static Lazy<Icon> notifyIconIcon = new Lazy<Icon>(() =>
            Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
        );

        protected override void OnStartup(StartupEventArgs e) {
            Process mainProcess;

            if (TryLockAsMainProcess(out mainProcess)) {
                StartAsMainProcess(e.Args);
            } else {
                if (e.Args != null) {
                    for (int i = 0; i < e.Args.Length; i++) {
                        WmCopyDataHelper.SendString(mainProcess, e.Args[i]);
                    }
                }
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e) {
            photoWindows.CloseAllForInstanceLifetime(true);
            notifyIcon?.Dispose();
            base.OnExit(e);
        }

        private void StartAsMainProcess(string[] args) {
            notifyIcon = StartTrayIcon();
            
            new SpongeWindow().WndProcCalled += (s, e) => {
                //open up files sent to us
                if (e.Msg == WmCopyDataHelper.WM_COPYDATA) {
                    OpenFile(WmCopyDataHelper.ReadString(e));
                }
            };

            //open windows from last time
            using (var db = new WindowDbContext()) {
                foreach (var windowData in db.Windows) {
                    OpenPhotoWindowOnNewThread(windowData);
                }
            }

            if (args != null) {
                for (int i = 0; i < args.Length; i++) {
                    OpenFile(args[i]);
                }
            }

            //todo: only if ((no windows opened from last time || from args) && started manually by user)
            {
                OpenPhotoWindowOnNewThread();
            }
        }

        /// <summary>
        ///     Opens a new window on it's own thread and shows it.
        /// </summary>
        /// <remarks>
        ///     Make sure to access this window using Window.Dispatcher.BeginInvoke.
        /// </remarks>
        /// <param name="path">Optional photo to open in new window.</param>
        /// <returns>The new Window.</returns>
        private void OpenPhotoWindowOnNewThread(PhotoWindowData? data = null) {
            Thread thread = new Thread(() => {
                PhotoWindow window = (data != null) ? new PhotoWindow(new PhotoWindowViewModel(data)) : new PhotoWindow();
                window.Closed += (s, e) =>
                    window.Dispatcher.InvokeShutdown();
                photoWindows.Add(window);
                window.Show();
                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private WinForms.NotifyIcon StartTrayIcon() {
            var icon = new WinForms.NotifyIcon();

            icon.BalloonTipText = "todo: balloon tip.";
            icon.BalloonTipTitle = "Simple Frame";
            //icon.Text = "Simple Frame";
            icon.Icon = notifyIconIcon.Value;

            var strip = new WinForms.ContextMenuStrip();

            WinForms.ToolStripItem closeAll = new WinForms.ToolStripMenuItem("Close All");
            closeAll.Click += (s, e) => {
                photoWindows.CloseAll(false);
            };
            strip.Items.Add(closeAll);

            WinForms.ToolStripItem quit = new WinForms.ToolStripMenuItem("Quit");
            quit.Click += (s, e) => {
                Shutdown();
            };
            strip.Items.Add(quit);

            strip.Items.Add(new WinForms.ToolStripSeparator());

            icon.ContextMenuStrip = strip;

            icon.Visible = true;

            return icon;
        }


        private bool TryLockAsMainProcess(out Process exclusiveProcess) {

#pragma warning disable CS8625 // only here because compiler thinks the might not be initialized below...
            exclusiveProcess = null;
#pragma warning restore CS8625

            bool retry;
            bool weWin = false;

            do {
                retry = false;

                try {
                    var lockFileStream = File.Open(LOCK_PATH, FileMode.Create, FileAccess.Write, FileShare.Read);
                    exclusiveProcess = Process.GetCurrentProcess();
                    var bytes = Encoding.UTF8.GetBytes(exclusiveProcess.Id.ToString());
                    lockFileStream.Write(bytes, 0, bytes.Length);
                    lockFileStream.Flush();//todo: worry about a partial write here
                    weWin = true;

                } catch (Exception ex) {//todo: more specific

                    using (var fs = File.Open(LOCK_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        using (var reader = new StreamReader(fs)) {
                            int id = int.Parse(reader.ReadToEnd());
                            try {
                                exclusiveProcess = Process.GetProcessById(id);
                            } catch (ArgumentException) {
                                //process might have changed
                                retry = true;
                            }
                        }
                    }

                }

            } while (retry);

            return weWin;
        }

        private void OpenFile(string uri) {
            throw new NotImplementedException();
        }

    }

}
