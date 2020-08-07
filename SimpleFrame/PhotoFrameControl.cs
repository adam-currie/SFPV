using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleFrame {
    public class PhotoFrameControl : ContentControl {
        static PhotoFrameControl() {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PhotoFrameControl), 
                new FrameworkPropertyMetadata(typeof(PhotoFrameControl)));
        }

        public static readonly DependencyProperty FrameProperty =
            DependencyProperty.Register(
            "Frame", 
            typeof(PhotoFrameData),
            typeof(PhotoFrameControl),
            new FrameworkPropertyMetadata(OnFramePropertyChanged));

        private static void OnFramePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            //debug
            return;
        }

        public PhotoFrameData Frame {
            get { return (PhotoFrameData)GetValue(FrameProperty); }
            set { SetValue(FrameProperty, value); }
        }

    }
}
