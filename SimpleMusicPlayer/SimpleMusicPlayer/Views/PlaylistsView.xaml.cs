using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for PlayListsView.xaml
    /// </summary>
    public partial class PlayListsView : UserControl
    {
        public PlayListsView()
        {
            this.InitializeComponent();

            this.PreviewKeyDown += (sender, e) => (this.DataContext as IKeyHandler).HandlePreviewKeyDown(sender, e);

            this.DataContextChanged += this.PlaylistsView_DataContextChanged;
        }

        private void PlaylistsView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs dea)
        {
            var vm = this.DataContext as PlayListsViewModel;
            if (vm != null)
            {
                // for the first, i need a connection for scrolling to the first playable media file
                vm.ListBoxPlayList = this.ListBoxPlayList;

                this.Loaded += (o, args) => {
                    vm.LoadPlayListAsync();
                    vm.HandleCommandLineArgsAsync(Environment.GetCommandLineArgs().ToList());
                };

                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.SizeChanged += (s, e) => vm.CalcPlayListItemTemplateByActualWidth(window.ActualWidth);
                }
            }
        }
    }
}