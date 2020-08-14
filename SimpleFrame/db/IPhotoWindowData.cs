namespace SimpleFrame.DB {
    public interface IPhotoWindowData {
        string? Frame { get; set; }
        double Height { get; set; }
        int Id { get; set; }
        string? ImagePath { get; set; }
        int Left { get; set; }
        int Top { get; set; }
        double Width { get; set; }
    }
}