﻿using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace SimpleFrame {
    internal class MainNotifyIcon : IDisposable {
        NotifyIcon icon;

        public event EventHandler? Quit;
        public event EventHandler? CloseAll;

        public MainNotifyIcon() {
            icon = new NotifyIcon();

            icon.BalloonTipTitle = "SFPV";
            icon.Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);

            var strip = new ContextMenuStrip();

            ToolStripItem closeAll = new ToolStripMenuItem("Close All");
            closeAll.Click += CloseAll;
            strip.Items.Add(closeAll);

            ToolStripItem quit = new ToolStripMenuItem("Quit");
            quit.Click += Quit;
            strip.Items.Add(quit);

            strip.Items.Add(new ToolStripSeparator());

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
                typeof(NotifyIcon)
                    .GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.Invoke(icon, null);
            };
        }

        public void Dispose() {
            icon.Dispose();
        }
    }
}