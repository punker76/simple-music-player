using System;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public interface IPlayerEngine
  {
    void Play(IMediaFile file);
    void Pause();
    void Stop();

    PlayerState State { get; }

    float Volume { get; set; }
    TimeSpan Length { get; }
    double CurrentPositionMs { get; set; }
  }
}