using System.IO;
using System.Windows.Media.Imaging;

namespace SimpleFrame {
    internal class PhotoFramePreview {
        public BitmapImage Thumbnail { get; }
        public string? Path { get; }

        public PhotoFramePreview(BitmapImage thumbnail, string? path) {
            Thumbnail = thumbnail;
            Path = path;
        }
    }
}