using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleFrame {
    public class PhotoFrameData {

        public ImageSource Top { get; private set; }
        public ImageSource Bottom { get; private set; }
        public ImageSource Left { get; private set; }
        public ImageSource Right { get; private set; }
        public ImageSource TopLeft { get; private set; }
        public ImageSource BottomRight { get; private set; }
        public ImageSource BottomLeft { get; private set; }
        public ImageSource TopRight { get; private set; }
        public ImageSource Thumbnail { get; private set; }
        public string? Path { get; private set; }

        private PhotoFrameData(
            ImageSource top,
            ImageSource bottom,
            ImageSource left,
            ImageSource right,
            ImageSource topLeft,
            ImageSource bottomRight,
            ImageSource bottomLeft,
            ImageSource topRight,
            ImageSource thumbnail,
            string? path
            ) {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Thumbnail = thumbnail;
            Path = path;
        }

        internal class Builder {
            public ImageSource? Top { get; set; }
            public ImageSource? Bottom { get; set; }
            public ImageSource? Left { get; set; }
            public ImageSource? Right { get; set; }
            public ImageSource? TopLeft { get; set; }
            public ImageSource? BottomRight { get; set; }
            public ImageSource? BottomLeft { get; set; }
            public ImageSource? TopRight { get; set; }
            public ImageSource? Thumbnail { get; set; }

            private string? _path;
            private bool pathSet = false;
            public string? Path {
                get => _path;
                set { _path = value; pathSet = true; }
            }


            /// <summary>
            /// Builds the instance.
            /// </summary>
            /// <exception cref="ArgumentException">
            /// One or more parameters were not set.
            /// </exception>
            /// <returns>new instance</returns>
            public PhotoFrameData Build() {
                if (Top == null) throw new ArgumentException(nameof(Top));
                if (Bottom == null) throw new ArgumentException(nameof(Bottom));
                if (Left == null) throw new ArgumentException(nameof(Left));
                if (Right == null) throw new ArgumentException(nameof(Right));
                if (TopLeft == null) throw new ArgumentException(nameof(TopLeft));
                if (TopRight == null) throw new ArgumentException(nameof(TopRight));
                if (BottomLeft == null) throw new ArgumentException(nameof(BottomLeft));
                if (BottomRight == null) throw new ArgumentException(nameof(BottomRight));
                if (Thumbnail == null) throw new ArgumentException(nameof(Thumbnail));
                if (!pathSet) throw new ArgumentException(nameof(Path));

                return new PhotoFrameData(
                    top: Top, 
                    bottom: Bottom, 
                    left: Left, 
                    right: Right, 
                    topLeft: TopLeft, 
                    topRight: TopRight, 
                    bottomLeft: BottomLeft, 
                    bottomRight: BottomRight,
                    thumbnail: Thumbnail,
                    path: Path
                );
            }

        }
        
    }
}