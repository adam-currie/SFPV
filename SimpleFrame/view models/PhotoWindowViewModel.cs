using GalaSoft.MvvmLight.Command;
using PhotoFrames;
using SimpleFrame.DB;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SimpleFrame {
    internal class PhotoWindowViewModel : ViewModelBase {
        private AutoSavingPhotoWindowData data;

        public bool Persistence {
            get => data.Persistence;
            set => UpdateProperty((x) => data.Persistence = x, data.Persistence, value);
        }

        private ImageSource? _imageSource;
        public ImageSource? ImageSource {
            get => _imageSource;
            private set => UpdateProperty(ref _imageSource, value);
        }

        public PhotoPreviewLoader FrameSelectionList { get; }

        private PhotoPreview? _selectedFramePreview;
        public PhotoPreview? SelectedFramePreview {
            get => _selectedFramePreview;
            set {
                if (UpdateProperty(ref _selectedFramePreview, value) && value != null) {
                    if (value.Path == null) {
                        FrameData = DefaultFrame.Value;
                    } else {
                        try {
                            using (var reader = new FrameReader(value.Path)) {
                                //todo: async with CancellableTask, one task at a time
                                FrameData = reader.ReadFrame();
                            }
                        } catch {//todo: more specific
                                 //todo: show error message
                        }
                    }
                }
            }
        }

        private FrameData _frameData;
        public FrameData FrameData {
            get => _frameData;
            private set {
                if (FrameData != value) {
                    if (PrevFrameData == null) PrevFrameData = value;
                    _frameData = value;
                    data.Frame = value.Path;
                    OnPropertyChanged();
                }
            }
        }

        public System.Windows.Size SizeInsideFrame {
            get => new System.Windows.Size(data.Width, data.Height);
            set => UpdateProperty(
                (x) => {
                    data.Width = value.Width;
                    data.Height = value.Height;
                }
                , SizeInsideFrame, value);
        }

        //when this is not null it stores the previous frame data until the user decides on what they are changing it to
        private FrameData? PrevFrameData { get; set; }

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

        public PhotoWindowViewModel(PhotoWindowData windowData, bool persistent = true) {
            data = new AutoSavingPhotoWindowData(windowData);

            if (data.ImagePath != null)
                _ = ExplicitlyLoadImage(data.ImagePath);

            if (data.Frame == null) {
                _frameData = DefaultFrame.Value;
            } else {
                using (var reader = new FrameReader(data.Frame)) {
                    _frameData = reader.ReadFrame();//todo: exceptions
                }
            }

            //todo: run some of this init code in the background

            //todo: view model base should provide this dispatcher (among other things)
            FrameSelectionList = new PhotoPreviewLoader(Dispatcher.CurrentDispatcher);

            ReloadFrameSelectionCommand = new RelayCommand(
                () => _ = FrameSelectionList.LoadAsync()
            );

            CancelFrameCommand = new RelayCommand(() => {
                if (PrevFrameData != null) {
                    FrameData = PrevFrameData;
                    PrevFrameData = null;
                }
            });

            AcceptFrameCommand = new RelayCommand(() => {
                PrevFrameData = null;
                data.Frame = FrameData.Path;
            });
        }

        /// <summary>
        /// Loads the next image in the current directory with wrapping.
        /// Doesn't fail, if we loop all the way around and we can't load anything, nothing happens.
        /// </summary>
        private async Task LoadNextImage() {
            throw new NotImplementedException("todo");//dont forget to set data.ImageSource
        }

        /// <summary>
        /// Loads an image and does the following(unlike non-explicit loads):
        /// <list type="bullet">
        ///     <item>
        ///         <description>Supplies information to user on failure.</description>
        ///     </item>
        ///     <item>
        ///         <description>Resizes the window to fit the image.</description>
        ///     </item>
        /// </list>
        /// If this fails <see cref="ImageSource"/> is set to null.
        /// </summary>
        private async Task ExplicitlyLoadImage(string path) {
            try {
                ImageSource = await LoadBitmapImageAsync(path);
                data.ImagePath = path;
                SizeInsideFrame = new System.Windows.Size(ImageSource.Width, ImageSource.Height);
            } catch (Exception e) {
                ImageSource = null;
                data.ImagePath = null;
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
        private static async Task<BitmapImage> LoadBitmapImageAsync(string path) {
            return await Task.Run(() => {
                //todo: maybe pass uri around instead and have it validated much earlier
                BitmapImage bitmap = new BitmapImage(new Uri(path));
                bitmap.Freeze();
                return bitmap;
            }).ConfigureAwait(false);
        }

    }
}
