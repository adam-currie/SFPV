using GalaSoft.MvvmLight.Command;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using SimpleFrame.DB;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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
                OnPropertyChanged();
            }
        }

        private int Left {
            get => data.Left;
            set => UpdateDbBackedProperty((x) => data.Left = x, data.Left, value);
        }

        private int Top {
            get => data.Top;
            set => UpdateDbBackedProperty((x) => data.Top = x, data.Top, value);
        }

        private int Width {
            get => data.Width;
            set => UpdateDbBackedProperty((x) => data.Width = x, data.Width, value);
        }

        private int Height {
            get => data.Height;
            set => UpdateDbBackedProperty((x) => data.Height = x, data.Height, value);
        }

        private string? Frame {
            get => data.Frame;
            set => UpdateDbBackedProperty((x) => data.Frame = x, data.Frame, value);
        }

        private string? ImagePath {
            get => data.ImagePath;
            set => UpdateDbBackedProperty((x) => data.ImagePath = x, data.ImagePath, value);
        }

        private ImageSource? _imageSource;
        public ImageSource? ImageSource {
            get => _imageSource;
            private set => UpdateProperty(ref _imageSource, value);
        }

        public PhotoFramePreviewLoader FrameSelectionList { get; }

        private string? _explicitImageLoadErrorMsg;
        /// <summary>
        /// Error messages are minimalistic and don't include the path string.
        /// </summary>
        public string? ExplicitLoadImageErrorMsg {
            get => _explicitImageLoadErrorMsg;
            private set => UpdateProperty(ref _explicitImageLoadErrorMsg, value);
        }

        public PhotoWindowViewModel(PhotoWindowData data, bool persistent = true) {
            this.data = data;

            if (data.ImagePath != null)
                _ = ExplicitlyLoadImage(data.ImagePath);

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

            //todo: view model base should provide this dispatcher (among other things)
            FrameSelectionList = new PhotoFramePreviewLoader(Dispatcher.CurrentDispatcher);

            ReloadFrameSelectionCommand = new RelayCommand(
                () => _ = FrameSelectionList.LoadAsync()
            );
        }

        public ICommand ReloadFrameSelectionCommand { get; private set; } 

        /// <summary>
        /// Loads the next image in the current directory with wrapping.
        /// Doesn't fail, if we loop all the way around and we can't load anything, nothing happens.
        /// </summary>
        private async Task LoadNextImage() {
            throw new NotImplementedException("todo");
        }

        /// <summary>
        /// Loads an image and supplies information to user on failure.
        /// If this fails <see cref="ImageSource"/> is set to null.
        /// </summary>
        private async Task ExplicitlyLoadImage(string path) {
            try {
                ImageSource = await LoadBitmapImageAsync(path);
            } catch (Exception e) {
                ImageSource = null;
                ExplicitLoadImageErrorMsg = e switch
                {
                    UriFormatException _    => Resources.PhotoWindow_Error_InvalidFilePath,
                    FileNotFoundException _ => Resources.PhotoWindow_Error_FileNotFound,
                    _                       => Resources.PhotoWindow_Error_Unknown,
                    //todo: there's got to be more exceptions wtf, test this out
                };
            }
        }

        /// <summary>
        /// Loads an image file as a BitmapImage.
        /// </summary>
        /// <exception cref="FileNotFoundException">File not found.</exception>
        /// <exception cref="UriFormatException">File not found.</exception>
        /// <param name="path">Path of image to load.</param>
        /// <returns></returns>
        private async Task<BitmapImage> LoadBitmapImageAsync(string path) {
            return await Task.Run(() => {
                //todo: maybe pass uri around instead and have it validated much earlier
                BitmapImage bitmap = new BitmapImage(new Uri(path));
                bitmap.Freeze();
                return bitmap;
            }).ConfigureAwait(false);
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
