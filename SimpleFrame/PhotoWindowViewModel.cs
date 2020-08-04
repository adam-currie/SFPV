using SimpleFrame.DB;
using System;
using System.Linq;

namespace SimpleFrame {
    internal class PhotoWindowViewModel : ViewModelBase {

        private PhotoWindowData data;

        private bool _persistence;
        public bool Persistence {
            get => _persistence;
            set {
                if (_persistence == value) return;
                _persistence = value;
                if (_persistence == false) {
                    //todo: maybe need to check existence first?
                    using (var db = new WindowDbContext()) {
                        db.Remove(data);
                        db.SaveChanges();
                    }
                } else {
                    using (var db = new WindowDbContext()) {
                        //todo: maybe need to create first?
                        db.Update(data);
                        db.SaveChanges();
                    }
                }
                OnPropertyChanged();
            }
        }

        public int Left {
            get => data.Left;
            set => UpdateDbBackedProperty((x) => data.Left = x, data.Left, value);
        }

        public int Top {
            get => data.Top;
            set => UpdateDbBackedProperty((x) => data.Top = x, data.Top, value);
        }

        public int Width {
            get => data.Width;
            set => UpdateDbBackedProperty((x) => data.Width = x, data.Width, value);
        }

        public int Height {
            get => data.Height;
            set => UpdateDbBackedProperty((x) => data.Height = x, data.Height, value);
        }

        public string? Frame {
            get => data.Frame;
            set => UpdateDbBackedProperty((x) => data.Frame = x, data.Frame, value);
        }

        public string? ImagePath {
            get => data.ImagePath;
            set => UpdateDbBackedProperty((x) => data.ImagePath = x, data.ImagePath, value);
        }

        public PhotoWindowViewModel(PhotoWindowData data, bool persistent = true) {
            this.data = data;

            _persistence = persistent;
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

        public PhotoWindowViewModel(bool persistent = true)
            : this(new PhotoWindowData(), persistent) { }

        public PhotoWindowViewModel(string pathToOpen, bool persistent = true)
            : this(new PhotoWindowData(pathToOpen), persistent) { }

        /// <summary>
        /// Wraps UpdateProperty to also update the db if persistence is enabled.
        /// </summary>
        private bool UpdateDbBackedProperty<T>(Action<T> changer, T original, T value) {
            bool changed = (UpdateProperty(changer, original, value));
            if (Persistence) {
                using (var db = new WindowDbContext()) {
                    db.Windows.Update(data);
                    db.SaveChanges();
                }
            }
            return changed;
        }

    }
}
