using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SchwabenCode.QuickIO;
using Splat;
using TinyIoC;

namespace SimpleMusicPlayer.Core
{
    public class PlayList
    {
        [JsonIgnore]
        public const string PlayListFileName = "playlist.smppl";

        public List<MediaFile> Files { get; set; }

        public static async Task<PlayList> LoadAsync()
        {
            try
            {
                var fileName = Path.Combine(TinyIoCContainer.Current.Resolve<AppHelper>().ApplicationPath, PlayListFileName);
                if (!QuickIOFile.Exists(fileName))
                {
                    return null;
                }
                LogHost.Default.Info("try loading play list from {0}", fileName);
                using (StreamReader file = await Task.Run(() => QuickIOFile.OpenText(fileName)))
                {
                    var serializer = new JsonSerializer();
                    return (PlayList)serializer.Deserialize(file, typeof(PlayList));
                }
            }
            catch (Exception ex)
            {
                LogHost.Default.ErrorException("could not load play list", ex);
            }
            return null;
        }

        public static bool Save(PlayList playList)
        {
            try
            {
                var fileName = Path.Combine(TinyIoCContainer.Current.Resolve<AppHelper>().ApplicationPath, PlayListFileName);
                LogHost.Default.Info("try saving play list to {0}", fileName);
                using (StreamWriter file = QuickIOFile.CreateText(fileName))
                {
                    file.AutoFlush = true;
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, playList);
                }
                LogHost.Default.Info("play list saved with {0} files", playList.Files.Count);
            }
            catch (Exception exception)
            {
                LogHost.Default.Error("could not save play list", exception);
                return false;
            }
            return true;
        }
    }
}