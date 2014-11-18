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

        private void PositionSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            var vm = this.DataContext as PlayControlViewModel;
            if (vm != null)
            {
                vm.PlayerEngine.DontUpdatePosition = true;
            }
        }

        private void PositionSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var vm = this.DataContext as PlayControlViewModel;
            if (vm != null)
            {
                BindingExpression be = ((Slider)sender).GetBindingExpression(RangeBase.ValueProperty);
                if (be != null)
                {
                    be.UpdateSource();
                }
                vm.PlayerEngine.DontUpdatePosition = false;
            }
        }
    }
}