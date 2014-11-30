using FMOD;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Core.Player
{
    internal class ChannelInfo
    {
        public FMOD.Channel Channel { get; set; }
        public IMediaFile File { get; set; }
        public DSP FaderDSP { get; set; }

        public void CleanUp()
        {
            if (this.Channel != null)
            {
                this.Channel.setVolume(0f).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                if (this.FaderDSP != null)
                {
                    this.Channel.removeDSP(this.FaderDSP).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                }
                this.Channel.setCallback(null).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                this.Channel = null;
            }

            this.File.State = PlayerState.Stop;
            this.File = null;
        }
    }
}