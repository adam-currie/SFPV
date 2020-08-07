using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using System.Xml;

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

            ZipArchiveEntry metaEntry = archive.GetEntry("meta.xml");
            using (StreamReader reader = new StreamReader(metaEntry.Open())) {
                using (Stream s = metaEntry.Open()) {
                    using (XmlReader xml = XmlReader.Create(s)) {
                        while (xml.Read()) {
                            if (xml.NodeType == XmlNodeType.Element && xml.Name == "side") {
                                switch (xml.GetAttribute("name")){
                                    case "top": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.TopOffset = offset;
                                        builder.TopRepeating = "true".Equals(xml.GetAttribute("repeating"));
                                        break;
                                    }
                                    case "bottom": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.BottomOffset = offset;
                                        builder.BottomRepeating = "true".Equals(xml.GetAttribute("repeating"));
                                        break;
                                    }
                                    case "left": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.LeftOffset = offset;
                                        builder.LeftRepeating = "true".Equals(xml.GetAttribute("repeating"));
                                        break;
                                    }
                                    case "right": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.RightOffset = offset;
                                        builder.RightRepeating = "true".Equals(xml.GetAttribute("repeating"));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

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
