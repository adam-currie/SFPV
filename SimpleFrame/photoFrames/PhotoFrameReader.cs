using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;

namespace SimpleFrame {
    internal class PhotoFrameReader : IDisposable {
        private readonly ZipArchive archive;
        private readonly string? path;

        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a new FrameReader.
        /// </summary>
        /// <param name="path">file path, pass null for default frame</param>
        public PhotoFrameReader(string? path = null) {//todo: exceptions
            this.path = path;
            Stream source = (path == null) ?
                (Stream)new MemoryStream(Resources.DefaultPhotoFrame) :
                new FileStream(path, FileMode.Open);
            archive = new ZipArchive(source, ZipArchiveMode.Read, false);
        }

        //todo: exceptions
        internal PhotoFrameData ReadFrame() {
            PhotoFrameData.Builder builder = new PhotoFrameData.Builder();

            builder.Top = ReadImage("top.png");
            builder.Bottom = ReadImage("bottom.png");
            builder.Left = ReadImage("left.png");
            builder.Right = ReadImage("right.png");
            builder.TopLeft = ReadImage("top-left.png");
            builder.TopRight = ReadImage("top-right.png");
            builder.BottomRight = ReadImage("bottom-right.png");
            builder.BottomLeft = ReadImage("bottom-left.png");
            builder.Thumbnail = ReadImage("thumbnail.png");

            builder.Path = path;
            //todo: meta

            return builder.Build();
        }

        //todo: exceptions
        internal BitmapImage ReadThumbnail()
            => ReadImage("thumbnail.png");

        private BitmapImage ReadImage(string path) {
            ZipArchiveEntry thumbEntry = archive.GetEntry(path);
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
