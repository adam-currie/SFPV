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
using PhotoFrames;

namespace SimpleFrame {
    internal class PhotoPreviewLoader : ReadOnlyObservableCollection<PhotoPreview> {
        private readonly ObservableCollection<PhotoPreview> collection;
        private volatile CancellableTask? loadTask;
        private readonly Dispatcher dispatcher;
        private readonly PhotoPreview defaultPreview =
            new PhotoPreview(DefaultFrame.Value.Thumbnail, DefaultFrame.Value.Path);

        public PhotoPreviewLoader(Dispatcher dispatcher) 
            : this(new ObservableCollection<PhotoPreview>(), dispatcher) { }

        private PhotoPreviewLoader(ObservableCollection<PhotoPreview> collection, Dispatcher dispatcher) 
            : base(collection) {
            this.collection = collection;
            this.dispatcher = dispatcher;
        }

        /// <summary>
        /// Load or reload previews asynchronously.
        /// </summary>
        public async Task LoadAsync() {
            var cancelSource = new CancellationTokenSource();
            CancellableTask newLoadTask = new CancellableTask(() => {
                foreach (string? path in PhotoFiles.GetPaths()) {
                    cancelSource.Token.ThrowIfCancellationRequested();

                    BitmapImage? thumb;
                    using (FrameReader reader = new FrameReader(path)) {
                        try {
                            thumb = reader.ReadThumbnail();
                        } catch (Exception) {
                            thumb = null;
                        }
                    }

                    if (thumb!=null) {
                        var preview = new PhotoPreview(thumb, path);
                        dispatcher.Invoke(() => 
                            collection.Add(preview)
                        );
                    }
                }

            }, cancelSource);

            var prevTask = Interlocked.Exchange(ref loadTask, newLoadTask);
            if (prevTask != null) {
                await prevTask.Cancel();
            }

            collection.Clear();
            collection.Add(defaultPreview);
            newLoadTask.Start();
        }

    }
}