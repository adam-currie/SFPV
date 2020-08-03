using System.Windows;

namespace SimpleFrame {

    public partial class PhotoWindow : Window {

        new internal PhotoWindowViewModel DataContext {
            get => (PhotoWindowViewModel)base.DataContext;
            set => base.DataContext = value;
        }

        internal PhotoWindow(PhotoWindowViewModel? model = null) {
            DataContext = model ?? new PhotoWindowViewModel();
            InitializeComponent();
        }
    }
}
