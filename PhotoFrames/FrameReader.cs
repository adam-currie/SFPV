using System;
using System.IO;
using System.IO.Compression;
using System.Windows.Media.Imaging;
using System.Xml;

namespace PhotoFrames {
    public sealed class FrameReader : IDisposable {
        private readonly ZipArchive archive;
        private readonly string? path;

        internal bool IsDisposed { get; private set; }

        /// <summary>
        /// Creates a new FrameReader.
        /// </summary>
        public FrameReader(string path) 
            : this(new FileStream(path, FileMode.Open), path) { }

        public FrameReader(Stream source, string? path = null) {//todo: exceptions
            this.path = path;
            archive = new ZipArchive(source, ZipArchiveMode.Read, false);
        }

        //todo: exceptions
        public FrameData ReadFrame() {
            FrameData.Builder builder = new FrameData.Builder();

            builder.Top = new FrameData.Section(ReadImage("top.png"));
            builder.Bottom = new FrameData.Section(ReadImage("bottom.png"));
            builder.Left = new FrameData.Section(ReadImage("left.png"));
            builder.Right = new FrameData.Section(ReadImage("right.png"));
            builder.TopLeft = new FrameData.Section(ReadImage("top-left.png"));
            builder.TopRight = new FrameData.Section(ReadImage("top-right.png"));
            builder.BottomRight = new FrameData.Section(ReadImage("bottom-right.png"));
            builder.BottomLeft = new FrameData.Section(ReadImage("bottom-left.png"));
            builder.Thumbnail = ReadImage("thumbnail.png");

            builder.Path = path;

            ZipArchiveEntry metaEntry = archive.GetEntry("meta.xml");
            using (StreamReader reader = new StreamReader(metaEntry.Open())) {
                using (Stream s = metaEntry.Open()) {
                    using (XmlReader xml = XmlReader.Create(s)) {
                        while (xml.Read()) {
                            //todo: corners
                            if (xml.NodeType == XmlNodeType.Element && xml.Name == "side") {
                                switch (xml.GetAttribute("name")){
                                    case "top": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.Top.YOffset = offset;
                                        builder.Top.Repeating = "true".Equals(xml.GetAttribute("repeating"));
                                        break;
                                    }
                                    case "bottom": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.Bottom.YOffset = offset;
                                        builder.Bottom.Repeating = "true".Equals(xml.GetAttribute("repeating"));
                                        break;
                                    }
                                    case "left": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.Left.XOffset = offset;
                                        builder.Left.Repeating = "true".Equals(xml.GetAttribute("repeating"));
                                        break;
                                    }
                                    case "right": {
                                        if (int.TryParse(xml.GetAttribute("offset"), out int offset))
                                            builder.Right.XOffset = offset;
                                        builder.Right.Repeating = "true".Equals(xml.GetAttribute("repeating"));
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
        public BitmapImage ReadThumbnail()
            => ReadImage("thumbnail.png");

        private BitmapImage ReadImage(string path) {
            ZipArchiveEntry thumbEntry = archive.GetEntry(path);
            //todo: refactor for performance
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

        private void Dispose(bool disposing) {
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
