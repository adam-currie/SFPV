using SimpleFrame.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleFrame.DB {
    internal class AutoSavingPhotoWindowData : IPhotoWindowData {
        private readonly PhotoWindowData data;
        private bool _persistence;

        public AutoSavingPhotoWindowData(PhotoWindowData data, bool persistence = true) {
            this.data = data;
            _persistence = persistence;

            if (_persistence == false) {
                //todo: maybe need to check existence first?
                using (var db = new WindowDbContext()) {
                    db.Remove(data);
                    db.SaveChanges();
                }
            } else {
                using (var db = new WindowDbContext()) {
                    //todo: maybe need to create first?
                    db.Windows.Update(data);
                    db.SaveChanges();
                }
            }
        }

        public bool Persistence {
            get => _persistence;
            set {
                if (_persistence == value) return;
                _persistence = value;
                if (_persistence == false) {
                    using (var db = new WindowDbContext()) {
                        db.Remove(data);
                        db.SaveChanges();
                    }
                } else {
                    using (var db = new WindowDbContext()) {
                        db.Update(data);
                        db.SaveChanges();
                    }
                }
            }
        }

        public string? Frame { 
            get => data.Frame; 
            set {
                if (!string.Equals(data.Frame, value)) {
                    data.Frame = value;
                    if (Persistence) {
                        using (var db = new WindowDbContext()) {
                            db.Windows.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public double Height { 
            get => data.Height;
            set {
                if (data.Height != value) {
                    data.Height = value;
                    if (Persistence) {
                        using (var db = new WindowDbContext()) {
                            db.Windows.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public int Id { 
            get => data.Id;
            set {
                if (data.Id != value) {
                    data.Id = value;
                    if (Persistence) {
                        using (var db = new WindowDbContext()) {
                            db.Windows.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public string? ImagePath { 
            get => data.ImagePath;
            set {
                if (!string.Equals(data.ImagePath, value)) {
                    data.ImagePath = value;
                    if (Persistence) {
                        using (var db = new WindowDbContext()) {
                            db.Windows.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public int Left { 
            get => data.Left;
            set {
                if (data.Left != value) {
                    data.Left = value;
                    if (Persistence) {
                        using (var db = new WindowDbContext()) {
                            db.Windows.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }

        public int Top { 
            get => data.Top;
            set {
                if (data.Top != value) {
                    data.Top = value;
                    if (Persistence) {
                        using (var db = new WindowDbContext()) {
                            db.Windows.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
        public double Width { 
            get => data.Width;
            set {
                if (data.Width != value) {
                    data.Width = value;
                    if (Persistence) {
                        using (var db = new WindowDbContext()) {
                            db.Windows.Update(data);
                            db.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
