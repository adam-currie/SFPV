using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SimpleFrame {

    /// <summary>
    /// Thread-Safe collection of windows that can be closed from other threads.
    /// </summary>
    internal partial class ConcurrentClosableWindowCollection {
        private volatile int closeAllForever = 0;//0=false
        private readonly ThreadLocal<LinkedList<Window>> threadLocalWindows
            = new ThreadLocal<LinkedList<Window>>(() => new LinkedList<Window>(), true);

        public bool ClosedForever
            => closeAllForever != 0;

        public void Add(Window window) {
            /*
             * Dispatching to window thread so that it will be in the correct thread-local collection,
             * also this makes the combination of adding the window and hooking up the closed event
             * effectively atomic since close and the closed event are only called on the ui thread.
             */
            window.Dispatcher.BeginInvoke(() => {
                LinkedList<Window> windowsForThisThread = threadLocalWindows.Value!;

                bool closing = false;
                lock (windowsForThisThread) {
                    closing = closeAllForever == 0;
                    /*
                     * Need to check closeAllForever inside lock because we don't want
                     * a thread setting it and closing our thread local windows
                     * between this check and this add, otherwise the window will be
                     * added too late and not closed.
                     */
                    if (!closing)
                        windowsForThisThread.AddLast(window);
                }

                if (closing) {
                    window.Dispatcher.BeginInvoke(window.Close);
                } else {
                    window.Closed += WindowClosed;
                }
            });
        }

        private void WindowClosed(object? s, EventArgs e) {
            lock (threadLocalWindows.Value!) {
                threadLocalWindows.Value.Remove((Window)s!);
            }
        }

        public void CloseAll() {
            foreach (LinkedList<Window> windowsForOneThread in threadLocalWindows.Values) {
                lock (windowsForOneThread) {
                    foreach (Window window in windowsForOneThread) {
                        window.Closed -= WindowClosed;
                        window.Dispatcher.BeginInvoke(window.Close);
                    }
                    windowsForOneThread.Clear();
                }
            }
        }

        public void CloseAllForInstanceLifetime() {
            if (0 == Interlocked.CompareExchange(ref closeAllForever, 1, 0))
                CloseAll();
        }
    }
}
