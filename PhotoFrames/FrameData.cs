using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoFrames {
    //todo: must enforce that pixel format is one channel per byte(regardless if its RGB, RGBA, greyscale, whatever)
    public class FrameData {
        public string? Path { get; }
        internal PixelFormat Format { get; }
        internal BitmapPalette Palette { get; }
        internal int TopMargin { get; }
        internal int LeftMargin { get; }
        internal int BottomMargin { get; }
        internal int RightMargin { get; }

        internal Section Top { get; }
        internal Section Bottom { get; }
        internal Section Left { get; }
        internal Section Right { get; }
        internal Section TopLeft { get; }
        internal Section TopRight { get; }
        internal Section BottomLeft { get; }
        internal Section BottomRight { get; }

        public BitmapSource Thumbnail { get; }
        public int BytesPerPixel => Top.BytesPerPixel;
        internal FrameData(string? path, 
            Section top, Section bottom, Section left, Section right, 
            Section topLeft, Section topRight, Section bottomLeft, Section bottomRight, 
            BitmapSource thumbnail) {
            Path = path;
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
            TopLeft = topLeft;
            TopRight = topRight;
            BottomLeft = bottomLeft;
            BottomRight = bottomRight;
            Thumbnail = thumbnail;

            Format = Top.Image.Format;
            Palette = Top.Image.Palette;
            if((from s in new Section[]{TopLeft, Left, BottomLeft, Bottom, BottomRight, Right, TopRight} 
                select s.Image)
                .Any(x => Format!=x.Format || Palette!=x.Palette))
                    throw new FormatException("pixel format of frame images does not match");

            int Max(int a, int b, int c) {
                int n = a;
                if (b > n) n = b;
                if (c > n) n = c;
                return n;
            }

            LeftMargin = Max(
                TopLeft.Width - TopLeft.XOffset,
                BottomLeft.Width - BottomLeft.XOffset,
                Left.Width - Left.XOffset);

            TopMargin = Max(
                TopLeft.Height - TopLeft.YOffset,
                TopRight.Height - TopRight.YOffset,
                Top.Height - Top.YOffset);

            RightMargin = Max(
                TopRight.Width + TopRight.XOffset,
                BottomRight.Width + BottomRight.XOffset,
                Right.Width + Right.XOffset);

            BottomMargin = Max(
                BottomLeft.Height + BottomLeft.YOffset,
                BottomRight.Height + BottomRight.YOffset,
                Bottom.Height + Bottom.YOffset);
        }

        private static byte[] GetPixels(BitmapSource img) {
            int stride = img.PixelWidth * ((img.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[img.PixelHeight * stride];
            img.CopyPixels(pixels, stride, 0);
            return pixels;
        }

        public class Section {
            private readonly int bufferSize;
            internal BitmapSource Image { get; }
            internal IntPtr Pixels { get; }
            internal int Width { get; }
            internal int Height { get; }

            ///<summary> Always zero for horizontal sides. </summary>
            internal int XOffset { get; set; }

            ///<summary> Always zero for vertical sides. </summary>
            internal int YOffset { get; set; }

            ///<summary> Always false for corners. </summary>
            internal bool Repeating { get; set; }
            internal int BytesPerPixel { get; }

            internal Section(BitmapSource image, int xOffset = 0, int yOffset = 0, bool repeating = false) {
                if (!image.IsFrozen) throw new ArgumentException("image must be frozen");
                Image = image;
                XOffset = xOffset;
                YOffset = yOffset;
                Repeating = repeating;
                Width = Image.PixelWidth;
                Height = Image.PixelHeight;
                BytesPerPixel = (Image.Format.BitsPerPixel + 7) / 8;

                byte[] bytes = GetPixels(Image);
                bufferSize = Marshal.SizeOf(bytes[0]) * bytes.Length;
                Pixels = Marshal.AllocHGlobal(bufferSize);
                GC.AddMemoryPressure(bufferSize);
                Marshal.Copy(GetPixels(Image), 0, Pixels, bytes.Length);
            }

            ~Section() {
                if (Pixels != IntPtr.Zero) {
                    Marshal.FreeHGlobal(Pixels);
                    GC.RemoveMemoryPressure(bufferSize);
                }
            }

            internal static Section CreateCorner(BitmapSource image, int xOffset, int yOffset)
                => new Section(image, xOffset, yOffset, false);

            internal static Section CreateHorizontalSide(BitmapSource image, int yOffset, bool repeating)
                => new Section(image, 0, yOffset, repeating);

            internal static Section CreateVerticalSide(BitmapSource image, int xOffset, bool repeating)
                => new Section(image, xOffset, 0, repeating);
        }

        internal class Builder {
            internal string? Path { get; set; }

            internal Section? Top { get; set; }
            internal Section? Bottom { get; set; }
            internal Section? Left { get; set; }
            internal Section? Right { get; set; }
            internal Section? TopLeft { get; set; }
            internal Section? TopRight { get; set; }
            internal Section? BottomLeft { get; set; }
            internal Section? BottomRight { get; set; }
            internal BitmapSource? Thumbnail { get; set; }

            /// <summary>
            /// Builds the instance.
            /// </summary>
            /// <exception cref="ArgumentException">
            /// One or more parameters were not set.
            /// </exception>
            /// <returns>new instance</returns>
            internal FrameData Build() {
                if (Top == null) throw new ArgumentException(nameof(Top));
                if (Bottom == null) throw new ArgumentException(nameof(Bottom));
                if (Left == null) throw new ArgumentException(nameof(Left));
                if (Right == null) throw new ArgumentException(nameof(Right));
                if (TopLeft == null) throw new ArgumentException(nameof(TopLeft));
                if (TopRight == null) throw new ArgumentException(nameof(TopRight));
                if (BottomLeft == null) throw new ArgumentException(nameof(BottomLeft));
                if (BottomRight == null) throw new ArgumentException(nameof(BottomRight));
                if (Thumbnail == null) throw new ArgumentException(nameof(Thumbnail));

                return new FrameData(
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