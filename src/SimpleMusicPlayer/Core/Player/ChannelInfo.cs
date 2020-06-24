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
        private CHANNELCONTROL_CALLBACK channelEndCallback;

        public ChannelInfo(Channel channel, IMediaFile file, Action playNextFileAction)
        {
            Channel = channel;
            File = file;
            this.playNextFileAction = playNextFileAction;
            channelEndCallback = new CHANNELCONTROL_CALLBACK(ChannelEndCallback);
            channel.setCallback(channelEndCallback).ERRCHECK();

            Volume = 1f;
            channel.setVolume(1f).ERRCHECK();

            channel.getSystemObject(out system).ERRCHECK();

            channel.getCurrentSound(out var currentSound).ERRCHECK();

            currentSound.getFormat(out SOUND_TYPE type, out SOUND_FORMAT format, out int channels, out int bits)
                .ERRCHECK();
            system.getSoftwareFormat(out int samplerate, out SPEAKERMODE speakermode, out int numrawspeakers)
                .ERRCHECK();

            if (samplerate > 0 && bits > 0)
            {
                system.lockDSP().ERRCHECK();

                channel.getDSPClock(out ulong dspclock, out ulong parentclock).ERRCHECK();

                channel.setDelay(0, 0, false).ERRCHECK();

                // milliseconds * samplerate / 1000
                var fadeDelay = Convert.ToUInt64(Math.Round(8000 * samplerate / 1000f, MidpointRounding.AwayFromZero));

                // add a fade point at 'now' with zero volume
                channel.addFadePoint(parentclock, 0f).ERRCHECK();
                // add a fade point 8 seconds later at 1 volume
                channel.addFadePoint(parentclock + fadeDelay, 1f).ERRCHECK();

                // PCM = PCM Samples, related to milliseconds * samplerate / 1000.
                currentSound.getLength(out uint length, TIMEUNIT.PCM).ERRCHECK();

                currentSound.getLength(out uint lengthMs, TIMEUNIT.MS).ERRCHECK();
                var convertedLength = Convert.ToUInt64(Math.Round(lengthMs * samplerate / 1000f, MidpointRounding.AwayFromZero));

                // using length or convertedLength doesn't work
                // both are to early!

                // add a start fade point 8 seconds before end with full volume
                channel.addFadePoint(parentclock + length - fadeDelay, 1f).ERRCHECK();
                // add a fade point at the end of the track
                channel.addFadePoint(parentclock + length, 0f).ERRCHECK();
                // add a delayed stop command at the end of the track ('stopchannels = true')
                channel.setDelay(0, parentclock + length, true).ERRCHECK();

                system.unlockDSP().ERRCHECK();
            }

            system.update().ERRCHECK();
        }

        public Channel Channel { get; private set; }

        public IMediaFile File { get; private set; }

        private RESULT ChannelEndCallback(IntPtr channelcontrol, CHANNELCONTROL_TYPE controltype, CHANNELCONTROL_CALLBACK_TYPE callbacktype, IntPtr commanddata1, IntPtr commanddata2)
        {
            if (callbacktype == CHANNELCONTROL_CALLBACK_TYPE.END)
            {
                // this must be thread safe
                var currentSynchronizationContext = TaskScheduler.FromCurrentSynchronizationContext();
                var uiTask = Task.Factory.StartNew(() => {
                    var action = playNextFileAction;
                    if (action != null)
                    {
                        action();
                    }
                }, CancellationToken.None, TaskCreationOptions.None, currentSynchronizationContext);
            }
            return RESULT.OK;
        }

        public void SetCurrentPositionMs(uint newPosition)
        {
            if (Channel.hasHandle())
            {
                bool paused;
                Channel.getPaused(out paused).ERRCHECK();
                Channel.setPaused(true).ERRCHECK();
                system.update().ERRCHECK();
                Channel.setPosition(newPosition, TIMEUNIT.MS).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                Channel.setPaused(paused).ERRCHECK();
                system.update().ERRCHECK();
            }
        }

        public bool FadeVolume(float startVol, float endVol, float startPoint, float fadeLength, float currentTime)
        {
            if ((fadeLength > 0f) && (currentTime >= startPoint) && (currentTime <= startPoint + fadeLength))
            {
                var calcVolume = Math.Abs(((endVol - startVol) / fadeLength) * (currentTime - startPoint));
                if (startVol < endVol)
                {
                    Volume = calcVolume + startVol;
                }
                else
                {
                    Volume = startVol - calcVolume;
                }
                return true;
            }
            return false;
        }

        private float volume = -1f;

        public float Volume
        {
            get { return volume; }
            set
            {
                return;
                if (Channel.hasHandle() == false || Equals(value, volume))
                {
                    return;
                }
                volume = value;
                Channel.setVolume(value).ERRCHECK();
                system.update().ERRCHECK();
            }
        }

        public void Pause()
        {
            if (Channel.hasHandle())
            {
                bool paused;
                Channel.getPaused(out paused).ERRCHECK();

                var newPaused = !paused;
                Channel.setPaused(newPaused).ERRCHECK();
                system.update().ERRCHECK();

                File.State = newPaused ? PlayerState.Pause : PlayerState.Play;
            }
        }

        public void CleanUp()
        {
            if (Channel.hasHandle())
            {
                Channel.setVolume(0f).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                Channel.setPaused(true).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                Channel.setCallback(null).ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                Channel.stop().ERRCHECK(RESULT.ERR_INVALID_HANDLE);
                Channel.clearHandle();
            }
            channelEndCallback = null;
            File.State = PlayerState.Stop;
            File = null;
            playNextFileAction = null;
        }
    }
}