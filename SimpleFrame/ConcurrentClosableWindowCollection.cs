using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SimpleFrame {

    internal class ConcurrentClosableWindowCollection : ConcurrentClosableWindowCollection<Window> { }

    /// <summary>
    /// Thread-Safe collection of windows that can be closed from other threads.
    /// </summary>
    internal class ConcurrentClosableWindowCollection<T> where T : Window {
        private volatile int closeAllForever = 0;//0=false
        private readonly ThreadLocal<LinkedList<T>> threadLocalWindows
            = new ThreadLocal<LinkedList<T>>(() => new LinkedList<T>(), true);

        public bool ClosedForever
            => closeAllForever != 0;

        public void Add(T window) {
            /*
             * Dispatching to window thread so that it will be in the correct thread-local collection,
             * also this makes the combination of adding the window and hooking up the closed event
             * effectively atomic since close and the closed event are only called on the ui thread.
             */
            window.Dispatcher.BeginInvoke(() => {
                LinkedList<T> windowsForThisThread = threadLocalWindows.Value!;

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
                threadLocalWindows.Value.Remove((T)s!);
            }
        }

        public void CloseAll(Action<T>? beforeClosing = null, Action<T>? afterClosing = null) {
            foreach (LinkedList<T> windowsForOneThread in threadLocalWindows.Values) {
                lock (windowsForOneThread) {
                    foreach (T window in windowsForOneThread) {
                        beforeClosing?.Invoke(window);
                        window.Closed -= WindowClosed;
                        window.Dispatcher.BeginInvoke(() => { 
                            window.Close(); 
                            afterClosing?.Invoke(window);
                        });
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
