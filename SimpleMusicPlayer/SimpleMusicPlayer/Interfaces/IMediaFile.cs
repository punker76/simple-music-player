using System.Windows.Media.Imaging;
using SimpleMusicPlayer.Common;

namespace SimpleMusicPlayer.Interfaces
{
  public interface IMediaFile
  {
    string FullFileName { get; set; }
    int PlayListIndex { get; set; }
    object PlayList { get; set; }

    PlayerState State { get; set; }

    string Title { get; set; }
    BitmapImage Cover { get; }
    string Album { get; set; }
    string FirstPerformer { get; set; }
  }
}