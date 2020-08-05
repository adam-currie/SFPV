﻿using SimpleFrame.DB;
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

        private FileStream? lockFileStream;//need to reference this to keep it locked
        private HiddenMsgWindow? msgWindow;

        private WinForms.NotifyIcon? notifyIcon;//won't be null after initialization

        private ConcurrentPhotoWindowCollection photoWindows = new ConcurrentPhotoWindowCollection();
        private static Lazy<Icon> notifyIconIcon = new Lazy<Icon>(() =>
            Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
        );

        protected override void OnStartup(StartupEventArgs e) {
            Process mainProcess;

            msgWindow = new HiddenMsgWindow();
            msgWindow.WndProcCalled += (s, e) => {
                //open up files sent to us
                if (e.Msg == WmCopyDataHelper.WM_COPYDATA) {
                    string path = WmCopyDataHelper.ReadString(e);
                    //todo: validate?
                    OpenPhotoWindowOnNewThread( new PhotoWindowData(path));
                }
            };

            if (TryLockAsMainProcess(out mainProcess)) {
                StartAsMainProcess(e.Args);
            } else {
                if (e.Args.Length > 0) {
                    IntPtr targetHwnd = HiddenMsgWindow.FindMsgWindowForProcess(mainProcess);
                    if (targetHwnd == IntPtr.Zero)
                        throw new Exception("Failed to open file(s) in main process.");

                    //we aren't the main process so send args over to that
                    for (int i = 0; i < e.Args.Length; i++) {
                        WmCopyDataHelper.SendString(targetHwnd, e.Args[i]);
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

            bool openedSomething = false;

            //open windows from last time
            using (var db = new WindowDbContext()) {
                foreach (var windowData in db.Windows) {
                    OpenPhotoWindowOnNewThread(windowData);
                    openedSomething = true;
                }
            }

            //open images from args
            if (args != null) {
                for (int i = 0; i < args.Length; i++) {
                    //todo: validate path
                    OpenPhotoWindowOnNewThread(new PhotoWindowData(args[i]));
                    openedSomething = true;
                }
            }

            //if nothing else opened and the user started us
            if (!openedSomething) {//todo: and started by user
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
                PhotoWindow window = (data != null) ? 
                    new PhotoWindow(new PhotoWindowViewModel(data)) : 
                    new PhotoWindow();
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

            icon.Click += (s, e) => {
                /*
                 * For some weird there are no public methods on NotifyIcon or ContextMenuStrip
                 * to show the menu "for the taskbar" with the same behavior as a right click.
                 * This private method does the job and it's pretty safe to expect it to exist since it's
                 * been around forever and win forms is already old and not going to change.
                 * We check for null anyway just in case.
                 */
                typeof(WinForms.NotifyIcon)
                    .GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(icon, null);
            };

            return icon;
        }


        private bool TryLockAsMainProcess(out Process exclusiveProcess) {

#pragma warning disable CS8625 // only here because dumb compiler thinks this might not be initialized below
            exclusiveProcess = null;
#pragma warning restore CS8625

            bool retry;
            int retries = 100;
            do {
                retry = false;

                try {
                    lockFileStream = File.Open(LOCK_PATH, FileMode.Create, FileAccess.Write, FileShare.Read);
                    exclusiveProcess = Process.GetCurrentProcess();
                    var bytes = Encoding.UTF8.GetBytes(exclusiveProcess.Id.ToString());
                    lockFileStream.Write(bytes, 0, bytes.Length);
                    lockFileStream.Flush();
                } catch (Exception ex) {//todo: more specific

                    using (var fs = File.Open(LOCK_PATH, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                        using (var reader = new StreamReader(fs)) {
                            int id = int.Parse(reader.ReadToEnd());
                            try {
                                exclusiveProcess = Process.GetProcessById(id);
                            } catch (ArgumentException) {
                                //process might have changed
                                retry = true;
                                if (--retries <= 0) {
                                    throw new Exception("Failed to acquire lock file(\"" + LOCK_PATH + "\") and cannot locate the main process holding it.");
                                }
                            }
                        }
                    }

                }

            } while (retry);

            return lockFileStream != null;
        }

    }

}
