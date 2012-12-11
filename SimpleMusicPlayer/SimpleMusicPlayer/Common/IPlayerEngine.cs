using System;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public interface IPlayerEngine
  {
    void Play(IMediaFile file);
    void Pause();

    float Volume { get; set; }
    TimeSpan Length { get; }
    TimeSpan CurrentPosition { get; set; }
    TimeSpan RemainingPosition { get; set; }
  }
}