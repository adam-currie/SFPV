using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SimpleFrame {

    public partial class PhotoWindow : Window {
        //need to track this because we can't override Close()
        private bool persistOnClose = false;

        new internal PhotoWindowViewModel DataContext {
            get => (PhotoWindowViewModel)base.DataContext;
            set => base.DataContext = value;
        }

        internal PhotoWindow(PhotoWindowViewModel? model = null) {
            DataContext = model ?? new PhotoWindowViewModel();
            InitializeComponent();
        }

        internal void Close(bool persistence) {
            persistOnClose = persistence;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e) {
            DataContext.Persistence = persistOnClose;
            base.OnClosing(e);
        }

        private void FrameSelection_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if ((bool)e.NewValue == true)
                _ = DataContext.FrameSelectionList.LoadAsync();
        }

    }
}
