namespace SimpleFrame.DB {
    public class PhotoWindowData {

        //#pragma warning disable IDE0052 // here for entity framework
        //        [Key]
        //        public string Id { get; private set; }
        //#pragma warning restore IDE0052 

        public int Id { get; set; }

        public int Left { get; set; }

        public int Top { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public string? Frame { get; set; }

        public string? ImagePath { get; set; }

        //public PhotoWindowData() {
        //    Id = Guid.NewGuid().ToString();
        //}

    }
}
