namespace SimpleMusicPlayer.Interfaces
{
  public interface IMediaFile
  {
    string FullFileName { get; set; }
    int PlayListIndex { get; set; }
    object PlayList { get; set; }
  }
}