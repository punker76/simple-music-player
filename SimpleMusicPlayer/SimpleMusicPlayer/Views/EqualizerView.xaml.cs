using System;
using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for EqualizerView.xaml
    /// </summary>
    public partial class EqualizerView : ChildWindow
    {
        public EqualizerView()
        {
            InitializeComponent();
            this.FocusedElement = this.CloseButton;
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
