using System;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Core
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
        uint LengthMs { get; }
        uint CurrentPositionMs { get; set; }
    }
}