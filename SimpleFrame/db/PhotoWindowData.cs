namespace SimpleFrame.DB {
    public class PhotoWindowData : IPhotoWindowData {

        public int Id { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public string? Frame { get; set; }

        public string? ImagePath { get; set; }

        public PhotoWindowData() { }

        public PhotoWindowData(string path) {
            ImagePath = path;
        }

    }
}
