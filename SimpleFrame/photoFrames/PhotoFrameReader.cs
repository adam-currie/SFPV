using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;

namespace SimpleFrame {
    internal class PhotoFrameReader : IDisposable {
        private readonly ZipArchive archive;

        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a new FrameReader.
        /// </summary>
        /// <param name="source">the source stream</param>
        /// <param name="leaveOpen">If true, leave source open when disposing this FrameReader.</param>
        internal PhotoFrameReader(FileStream source, bool leaveOpen = false) =>
            archive = new ZipArchive(source, ZipArchiveMode.Read, leaveOpen);

        internal PhotoFrameReader(string path)
            : this(new FileStream(path, FileMode.Open)) { } //todo: exception

        internal PhotoFrameData ReadFrame() {
            throw new NotImplementedException("todo");
        }

        //todo: exceptions
        internal BitmapImage ReadThumbnail() {

            ZipArchiveEntry thumbEntry = archive.GetEntry("thumbnail.png");
            using (StreamReader reader = new StreamReader(thumbEntry.Open())) {
                using (Stream s = thumbEntry.Open()) {
                    using (var memStream = new MemoryStream()) {
                        s.CopyTo(memStream);
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memStream;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!IsDisposed) {
                if (disposing) {
                    archive.Dispose();
                }
                IsDisposed = true;
            }
        }

        public void Dispose() => Dispose(true);

    }
}
