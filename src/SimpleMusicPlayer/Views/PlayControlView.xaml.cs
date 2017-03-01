using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ReactiveUI;
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

            this.Events().Loaded.Subscribe(_ =>
            {
                var thumb = GetThumb(this.positionSlider);
                if (thumb != null)
                {
                    var dragCompleted = thumb.Events().DragCompleted.DistinctUntilChanged();
                    dragCompleted.Select(d => (Slider)((Thumb)d.Source).TemplatedParent).Subscribe(slider =>
                    {
                        var be = slider.GetBindingExpression(RangeBase.ValueProperty);
                        be?.UpdateSource();
                    });
                    dragCompleted.Select(x => Unit.Default).InvokeCommand((PlayControlViewModel)this.DataContext, vm => vm.PlayerEngine.SetCurrentPositionMs);

                    var dragdelta = thumb.Events().DragStarted.DistinctUntilChanged();
                    dragdelta.Subscribe(e =>
                    {
                        var vm = this.DataContext as PlayControlViewModel;
                        if (vm != null && vm.PlayerEngine.CurrentMediaFile != null)
                        {
                            vm.PlayerEngine.CanSetCurrentPositionMs = true;
                        }
                    });
                }
            });
        }

        private static Thumb GetThumb(Control slider)
        {
            var track = slider.Template.FindName("PART_Track", slider) as Track;
            return track?.Thumb;
        }
    }
}