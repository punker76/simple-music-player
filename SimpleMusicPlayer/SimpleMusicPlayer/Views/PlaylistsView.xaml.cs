using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ReactiveUI;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for PlayListsView.xaml
    /// </summary>
    public partial class PlayListsView : UserControl, IViewFor<PlayListsViewModel>
    {
        public PlayListsView()
        {
            this.InitializeComponent();

            this.Events().DataContextChanged.Subscribe(args => {
                var vm = (PlayListsViewModel)args.NewValue;
                this.ViewModel = vm;

                //this.Events().PreviewKeyDown.Subscribe(vm.HandlePreviewKeyDown);
                var previewKeyDown = this.Events().PreviewKeyDown;
                previewKeyDown.Where(x => x.Key == Key.Enter).InvokeCommand(vm.PlayCommand);
                previewKeyDown.Where(x => x.Key == Key.Delete).InvokeCommand(vm.DeleteCommand);

                this.Events().Loaded.Throttle(TimeSpan.FromMilliseconds(500), RxApp.MainThreadScheduler).InvokeCommand(vm.StartUpCommand);

                var window = Window.GetWindow(this);
                if (window != null)
                {
                    vm.CalcPlayListItemTemplateByActualWidth(window.ActualWidth);
                    window.Events().SizeChanged.Subscribe(e => vm.CalcPlayListItemTemplateByActualWidth(e.NewSize.Width));
                }
            });
        }

        public PlayListsViewModel ViewModel
        {
            get { return (PlayListsViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(PlayListsViewModel), typeof(PlayListsView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = (PlayListsViewModel)value; }
        }
    }
}