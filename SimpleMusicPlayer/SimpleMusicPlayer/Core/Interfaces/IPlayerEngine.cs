using System;
using SimpleMusicPlayer.Core.Player;

namespace SimpleMusicPlayer.Core.Interfaces
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