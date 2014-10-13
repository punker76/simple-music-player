using SimpleMusicPlayer.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace SimpleMusicPlayer.ViewModels
{
  public class PlayControlInfoViewModel : ViewModelBase
  {
    private PlayInfoViewModel playInfoViewModel;
    private PlayControlViewModel playControlViewModel;

    public PlayControlInfoViewModel(Dispatcher dispatcher)
    {
    }

    public PlayInfoViewModel PlayInfoViewModel
    {
      get { return this.playInfoViewModel; }
      set
      {
        if (Equals(value, this.playInfoViewModel)) {
          return;
        }
        this.playInfoViewModel = value;
        this.OnPropertyChanged(() => this.PlayInfoViewModel);
      }
    }

    public PlayControlViewModel PlayControlViewModel
    {
      get { return this.playControlViewModel; }
      set
      {
        if (Equals(value, this.playControlViewModel)) {
          return;
        }
        this.playControlViewModel = value;
        this.OnPropertyChanged(() => this.PlayControlViewModel);
      }
    }
  }
}
