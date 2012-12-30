using System;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.Common
{
  public interface IPlayerEngine
  {
    bool Initializied { get; }

    void Play(IMediaFile file);
    void Pause();
    void Stop();

    PlayerState State { get; }

    Equalizer Equalizer { get; }

    float Volume { get; set; }
    TimeSpan Length { get; }
    double CurrentPositionMs { get; set; }
  }
}