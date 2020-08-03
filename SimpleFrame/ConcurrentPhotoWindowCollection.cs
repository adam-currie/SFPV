using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SimpleFrame {

    /// <summary>
    /// Thread-Safe collection of photo windows that can be closed from other threads.
    /// </summary>
    internal class ConcurrentPhotoWindowCollection {
        private const int CLOSE_FOREVER_OFF = 0;
        private const int CLOSE_FOREVER_ON_PERSIST = 1;
        private const int CLOSE_FOREVER_ON_DONT_PERSIST = 2;

        //would have used an enum here but can't because Interlocked doesn't like it
        private volatile int closeAllForever = CLOSE_FOREVER_OFF;
        private readonly ThreadLocal<LinkedList<PhotoWindow>> threadLocalWindows
            = new ThreadLocal<LinkedList<PhotoWindow>>(() => new LinkedList<PhotoWindow>(), true);

        public bool ClosedForever
            => closeAllForever != CLOSE_FOREVER_OFF;

        public void Add(PhotoWindow window) {
            /*
             * Dispatching to window thread so that it will be in the correct thread-local collection,
             * also this makes the combination of adding the window and hooking up the closed event
             * effectively atomic since close and the closed event are only called on the ui thread.
             */
            window.Dispatcher.BeginInvoke(() => {
                LinkedList<PhotoWindow> windowsForThisThread = threadLocalWindows.Value!;

                int cachedCloseState;
                lock (windowsForThisThread) {
                    cachedCloseState = closeAllForever;
                    /*
                     * Need to check closeAllForever inside lock because we don't want
                     * a thread setting it and closing our thread local windows
                     * between this check and this add, otherwise the window will be
                     * added too late and not closed.
                     */
                    if (cachedCloseState == CLOSE_FOREVER_OFF)
                        windowsForThisThread.AddLast(window);
                }

                switch (cachedCloseState) {
                    case CLOSE_FOREVER_OFF:
                        window.Closed += WindowClosed;
                        break;
                    case CLOSE_FOREVER_ON_PERSIST:
                        window.Close(true);
                        break;
                    case CLOSE_FOREVER_ON_DONT_PERSIST:
                        window.Close(false);
                        break;
                }
            });
        }

        private void WindowClosed(object? s, EventArgs e) {
            lock (threadLocalWindows.Value!) {
                threadLocalWindows.Value.Remove((PhotoWindow)s!);
            }
        }

        public void CloseAll(bool persist) {
            foreach (LinkedList<PhotoWindow> windowsForOneThread in threadLocalWindows.Values) {
                lock (windowsForOneThread) {
                    foreach (PhotoWindow window in windowsForOneThread) {
                        window.Closed -= WindowClosed;
                        window.Dispatcher.BeginInvoke(() => { 
                            window.Close(persist); 
                        });
                    }
                    windowsForOneThread.Clear();
                }
            }
        }

        public void CloseAllForInstanceLifetime(bool persist) {
            int newState = persist ?
                CLOSE_FOREVER_ON_PERSIST :
                CLOSE_FOREVER_ON_DONT_PERSIST;
            if (0 == Interlocked.CompareExchange(ref closeAllForever, (int)newState, CLOSE_FOREVER_OFF))
                CloseAll(persist);
        }
    }
}
