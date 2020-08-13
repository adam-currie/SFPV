using System.IO;
using System.Windows.Media.Imaging;

namespace SimpleFrame {
    internal class PhotoPreview {
        public BitmapSource Thumbnail { get; }
        public string? Path { get; }

        public PhotoPreview(BitmapSource thumbnail, string? path) {
            Thumbnail = thumbnail;
            Path = path;
        }
    }
}