using System;
using System.Windows;
using MahApps.Metro.SimpleChildWindow;
using ReactiveUI;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for EqualizerView.xaml
    /// </summary>
    public partial class EqualizerView : ChildWindow, IViewFor<EqualizerViewModel>
    {
        public EqualizerView()
        {
            InitializeComponent();

            this.FocusedElement = this.CloseButton;

            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);

            this.WhenActivated(d => this.WhenAnyValue(x => x.ViewModel).Subscribe(vm => vm.CloseEqualizerCommand.Subscribe(_ => this.Close())));
        }

        public EqualizerViewModel ViewModel
        {
            get => (EqualizerViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(EqualizerViewModel), typeof(EqualizerView), new PropertyMetadata(null));

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (EqualizerViewModel)value;
        }
    }
}
