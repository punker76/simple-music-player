using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Views
{
    /// <summary>
    /// Interaction logic for PlayControlView.xaml
    /// </summary>
    public partial class PlayControlView : UserControl
    {
        public PlayControlView()
        {
            this.InitializeComponent();
        }

        private void PositionSlider_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (Math.Abs(e.HorizontalChange) > 1 || Math.Abs(e.VerticalChange) > 1)
            {
                var vm = this.DataContext as PlayControlViewModel;
                if (vm != null)
                {
                    vm.PlayerEngine.CanSetCurrentPositionMs = true;
                }
            }
        }

        private void PositionSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            var vm = this.DataContext as PlayControlViewModel;
            if (vm != null)
            {
                BindingExpression be = ((Slider)sender).GetBindingExpression(RangeBase.ValueProperty);
                if (be != null)
                {
                    be.UpdateSource();
                }
                if (vm.PlayerEngine.SetCurrentPositionMs.CanExecute(null))
                {
                    vm.PlayerEngine.SetCurrentPositionMs.Execute(null);
                }
            }
        }
    }
}