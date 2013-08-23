using System.Windows.Input;

namespace SimpleMusicPlayer.Interfaces
{
  public interface IKeyHandler
  {
    bool HandleKeyDown(Key key);
  }
}