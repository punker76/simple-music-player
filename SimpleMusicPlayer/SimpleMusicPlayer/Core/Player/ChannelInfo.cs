using System;
using System.Threading;
using System.Threading.Tasks;
using FMOD;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.FMODStudio;

namespace SimpleMusicPlayer.Core.Player
{
    internal class ChannelInfo
    {
        private PlayerEngine playerEngine;
        private FMOD.CHANNEL_CALLBACK channelEndCallback;

        public ChannelInfo(PlayerEngine playerEngine, Channel channel, IMediaFile file)
        {
            this.playerEngine = playerEngine;
            this.channelEndCallback = new FMOD.CHANNEL_CALLBACK(ChannelEndCallback);
            this.Channel = channel;
            this.File = file;
            this.Volume = 0f;
            channel.setCallback(this.channelEndCallback).ERRCHECK();
        }

        private RESULT ChannelEndCallback(IntPtr channelraw, CHANNELCONTROL_TYPE controltype, CHANNELCONTROL_CALLBACK_TYPE type, IntPtr commanddata1, IntPtr commanddata2)
        {
            if (type == CHANNELCONTROL_CALLBACK_TYPE.END)
            {
                // this must be thread safe
                var currentSynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
                var uiTask = Task.Factory.StartNew(() => {
                    var action = this.playerEngine.PlayNextFileAction;
                    if (action != null)
                    {
                        action();
                    }
                }, CancellationToken.None, TaskCreationOptions.None, currentSynchronizationContext);
            }
            return FMOD.RESULT.OK;
        }

        public FMOD.Channel Channel { get; private set; }

        public IMediaFile File { get; private set; }

        public bool FadeVolume(float startVol, float endVol, float startPoint, float fadeLength, float currentTime)
        {
            if ((fadeLength > 0f) && (currentTime >= startPoint) && (currentTime <= startPoint + fadeLength))
            {
                var calcVolume = Math.Abs(((endVol - startVol) / fadeLength) * (currentTime - startPoint));
                if (startVol < endVol)
                {
                    this.Volume = calcVolume + startVol;
                }
                else
                {
                    this.Volume = startVol - calcVolume;
                }
                return true;
            }
            return false;
        }

        private float volume = -1f;

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
            this.channelEndCallback = null;
            this.File.State = PlayerState.Stop;
            this.File = null;
            this.playerEngine = null;
        }
    }
}