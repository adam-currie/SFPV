using GalaSoft.MvvmLight.Command;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using SimpleFrame.DB;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Controls;
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

        private string? FramePath {
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

        private PhotoFramePreview? _selectedFramePreview;
        public PhotoFramePreview? SelectedFramePreview {
            get => _selectedFramePreview;
            set {
                if (UpdateProperty(ref _selectedFramePreview, value) && value != null) {
                    try {
                        using (var reader = new PhotoFrameReader(value.Path)) {
                            var nextPhotoFrameData = reader.ReadFrame();
                            if (PrevPhotoFrameData == null) {
                                PrevPhotoFrameData = PhotoFrameData;
                            }
                            PhotoFrameData = nextPhotoFrameData;
                        }
                    } catch {//todo: more specific
                        //todo: show error message
                    }
                }
            }
        }

        private PhotoFrameData _photoFrameData;
        public PhotoFrameData PhotoFrameData {
            get => _photoFrameData;
            private set => UpdateProperty(ref _photoFrameData, value);
        }
        
        //when this is not null it stores the previous frame data until the user decides on what they are changing it to
        private PhotoFrameData? PrevPhotoFrameData { get; set; }

        private string? _explicitImageLoadErrorMsg;
        /// <summary>
        /// Error messages are minimalistic and don't include the path string.
        /// </summary>
        public string? ExplicitLoadImageErrorMsg {
            get => _explicitImageLoadErrorMsg;
            private set => UpdateProperty(ref _explicitImageLoadErrorMsg, value);
        }

        public ICommand ReloadFrameSelectionCommand { get; private set; }
        public ICommand CancelFrameCommand { get; private set; }
        public ICommand AcceptFrameCommand { get; private set; }

        public PhotoWindowViewModel(bool persistent = true)
            : this(new PhotoWindowData(), persistent) { }

        public PhotoWindowViewModel(string pathToOpen, bool persistent = true)
            : this(new PhotoWindowData(pathToOpen), persistent) { }

        public PhotoWindowViewModel(PhotoWindowData data, bool persistent = true) {
            this.data = data;

            if (data.ImagePath != null)
                _ = ExplicitlyLoadImage(data.ImagePath);

            using (var reader = new PhotoFrameReader(data.Frame)) {
                _photoFrameData = reader.ReadFrame();//todo: exceptions
            }

            //todo: run some of this init code in the background

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

            CancelFrameCommand = new RelayCommand(() => {
                if (PrevPhotoFrameData != null) {
                    PhotoFrameData = PrevPhotoFrameData;
                    PrevPhotoFrameData = null;
                }
            });

            AcceptFrameCommand = new RelayCommand(() => {
                PrevPhotoFrameData = null;
                FramePath = PhotoFrameData.Path;
            });
        }

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
