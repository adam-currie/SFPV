using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace SimpleFrame {
    internal class PhotoFramePreviewLoader : ReadOnlyObservableCollection<PhotoFramePreview> {
        private readonly ObservableCollection<PhotoFramePreview> collection;
        private volatile CancellableTask? loadTask;
        private readonly Dispatcher dispatcher;

        public PhotoFramePreviewLoader(Dispatcher dispatcher) 
            : this(new ObservableCollection<PhotoFramePreview>(), dispatcher) { }

        private PhotoFramePreviewLoader(ObservableCollection<PhotoFramePreview> collection, Dispatcher dispatcher) 
            : base(collection) {
            this.collection = collection;
            this.dispatcher = dispatcher;

            //debug
            this.CollectionChanged += (s, e) => {
                return;
            };
        }

        /// <summary>
        /// Load or reload previews asynchronously.
        /// </summary>
        public async Task LoadAsync() {
            var cancelSource = new CancellationTokenSource();
            CancellableTask newTask = new CancellableTask(() => {
                var debug = PhotoFrameFiles.GetFiles();
                foreach (string path in PhotoFrameFiles.GetFiles()) {
                    cancelSource.Token.ThrowIfCancellationRequested();

                    BitmapImage? thumb;
                    using (PhotoFrameReader reader = new PhotoFrameReader(path)) {
                        try {
                            thumb = reader.ReadThumbnail();
                        } catch (Exception) {
                            thumb = null;
                        }
                    }

                    cancelSource.Token.ThrowIfCancellationRequested();

                    if (thumb!=null) {
                        var preview = new PhotoFramePreview(thumb, path);
                        dispatcher.Invoke(() => 
                            collection.Add(preview)
                        );
                    }
                }

            }, cancelSource);

            var prevTask = Interlocked.Exchange(ref loadTask, newTask);
            if (prevTask != null) {
                await prevTask.Cancel();
            }

            collection.Clear();
            newTask.Start();
        }

    }
}