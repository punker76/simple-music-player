using FMOD;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Core.Player
{
    internal class ChannelInfo
    {
        public FMOD.Channel Channel { get; set; }
        public IMediaFile File { get; set; }

        private float volume = -1;

        public float Volume
        {
            get { return this.volume; }
            set
            {
                if (this.Channel == null || Equals(value, this.volume))
                {
                    return;
                }
                this.volume = value;

                this.Channel.setVolume(value).ERRCHECK();
            }
        }

        public void CleanUp()
        {
            if (this.Channel != null)
            {
                this.Channel.setVolume(0f).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                this.Channel.setCallback(null).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                this.Channel = null;
            }

            this.File.State = PlayerState.Stop;
            this.File = null;
        }
    }
}