using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleFrame {

    public class CancellableTask : Task {
        private readonly CancellationTokenSource cancelSource;

        /// <summary>
        /// Instantiates a new CancellableTask.
        /// </summary>
        /// <remarks>The Cancellation token will be automatically disposed when the task is disposed.</remarks>
        public CancellableTask(Action action, CancellationTokenSource cancelSource, TaskCreationOptions creationOptions = TaskCreationOptions.None)
            : base(action, cancelSource.Token, creationOptions) {
            this.cancelSource = cancelSource;
        }

        /// <summary>
        /// Instantiates a new CancellableTask with state.
        /// </summary>
        /// <remarks>The Cancellation token will be automatically disposed when the task is disposed.</remarks>
        public CancellableTask(Action<object?> action, object? state, CancellationTokenSource cancelSource, TaskCreationOptions creationOptions = TaskCreationOptions.None)
            : base(action, state, cancelSource.Token, creationOptions) {
            this.cancelSource = cancelSource;
        }

        /// <summary>
        /// Awaitable cancellation.
        /// </summary>
        /// <returns></returns>
        public async Task Cancel() {
            cancelSource.Cancel();
            await WhenAll(this);
        }

        protected override void Dispose(bool disposing) {
            cancelSource.Dispose();
            base.Dispose(disposing);
        }

    }
}
