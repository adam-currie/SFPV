using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleFrame {
    public class PhotoFrameData {

        public ImageSource Top { get; }
        public ImageSource Bottom { get; }
        public ImageSource Left { get; }
        public ImageSource Right { get; }
        public int TopOffset { get; }
        public int BottomOffset { get; }
        public int LeftOffset { get; }
        public int RightOffset { get; }
        public bool TopRepeating { get; }
        public bool BottomRepeating { get; }
        public bool LeftRepeating { get; }
        public bool RightRepeating { get; }
        public ImageSource TopLeft { get; }
        public ImageSource BottomRight { get; }
        public ImageSource BottomLeft { get; }
        public ImageSource TopRight { get; }
        public ImageSource Thumbnail { get; }
        public string? Path { get; }

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
            string? path,
            int topOffset,
            int bottomOffset,
            int leftOffset,
            int rightOffset,
            bool topRepeating,
            bool bottomRepeating,
            bool leftRepeating,
            bool rightRepeating
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
            TopOffset = topOffset;
            BottomOffset = bottomOffset;
            LeftOffset = leftOffset;
            RightOffset = rightOffset;
            Path = path;
            BottomRepeating = bottomRepeating;
            TopRepeating = topRepeating;
            LeftRepeating = leftRepeating;
            RightRepeating = rightRepeating;
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

            public int TopOffset { get; set; } = 0;
            public int LeftOffset { get; set; } = 0;
            public int RightOffset { get; set; } = 0;
            public int BottomOffset { get; set; } = 0;
            public bool TopRepeating { get; set; } = false;
            public bool BottomRepeating { get; set; } = false;
            public bool LeftRepeating { get; set; } = false;
            public bool RightRepeating { get; set; } = false;


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
                    path: Path,
                    topOffset: TopOffset,
                    bottomOffset: BottomOffset,
                    rightOffset: RightOffset,
                    leftOffset: LeftOffset,
                    topRepeating: TopRepeating,
                    bottomRepeating: BottomRepeating,
                    leftRepeating: LeftRepeating,
                    rightRepeating: RightRepeating
                );
            }

        }
        
    }
}