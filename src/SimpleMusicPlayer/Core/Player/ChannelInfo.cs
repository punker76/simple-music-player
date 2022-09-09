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
        private FMOD.System system;
        private Action playNextFileAction;
        private FMOD.CHANNELCONTROL_CALLBACK channelEndCallback;

        public ChannelInfo(Channel channel, IMediaFile file, Action playNextFileAction)
        {
            this.Channel = channel;
            this.Channel.getSystemObject(out system).ERRCHECK();
            this.File = file;
            this.playNextFileAction = playNextFileAction;
            this.channelEndCallback = new FMOD.CHANNELCONTROL_CALLBACK(ChannelEndCallback);
            this.Channel.setCallback(this.channelEndCallback).ERRCHECK();
            this.Volume = 0f;
        }

        public FMOD.Channel Channel { get; private set; }

        public IMediaFile File { get; private set; }

        private RESULT ChannelEndCallback(IntPtr channelcontrol, CHANNELCONTROL_TYPE controltype, CHANNELCONTROL_CALLBACK_TYPE callbacktype, IntPtr commanddata1, IntPtr commanddata2)
        {
            if (callbacktype == CHANNELCONTROL_CALLBACK_TYPE.END)
            {
                // this must be thread safe
                var currentSynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
                var uiTask = Task.Factory.StartNew(() => {
                    var action = this.playNextFileAction;
                    if (action != null)
                    {
                        action();
                    }
                }, CancellationToken.None, TaskCreationOptions.None, currentSynchronizationContext);
            }
            return FMOD.RESULT.OK;
        }

        public void SetCurrentPositionMs(uint newPosition)
        {
            if (this.Channel.hasHandle())
            {
                bool paused;
                this.Channel.getPaused(out paused).ERRCHECK();
                this.Channel.setPaused(true).ERRCHECK();
                this.system.update().ERRCHECK();
                this.Channel.setPosition(newPosition, FMOD.TIMEUNIT.MS).ERRCHECK(FMOD.RESULT.ERR_INVALID_HANDLE);
                this.Channel.setPaused(paused).ERRCHECK();
                this.system.update().ERRCHECK();
            }
        }

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
            get => this.volume;
            set
            {
                if (this.Channel.hasHandle() == false || Equals(value, this.volume))
                {
                    return;
                }
                this.volume = value;
                this.Channel.setVolume(value).ERRCHECK();
                this.system.update().ERRCHECK();
            }
        }

        public void Pause()
        {
            if (this.Channel.hasHandle())
            {
                bool paused;
                this.Channel.getPaused(out paused).ERRCHECK();

                var newPaused = !paused;
                this.Channel.setPaused(newPaused).ERRCHECK();
                this.system.update().ERRCHECK();

                this.File.State = newPaused ? PlayerState.Pause : PlayerState.Play;
            }
        }

        public void CleanUp()
        {
            if (this.Channel.hasHandle())
            {
                this.Channel.setVolume(0f).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                this.Channel.setPaused(true).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                this.Channel.setCallback(null).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                this.Channel.clearHandle();
            }
            this.channelEndCallback = null;
            this.File.State = PlayerState.Stop;
            this.File = null;
            this.playNextFileAction = null;
        }
    }
}