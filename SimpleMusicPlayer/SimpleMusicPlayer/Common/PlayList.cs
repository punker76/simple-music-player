using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleMusicPlayer.Models;

namespace SimpleMusicPlayer.Common
{
    public class PlayList
    {
        [JsonIgnore]
        public const string PlayListFileName = "playlist.smppl";

        public List<MediaFile> Files { get; set; }

        public static async Task<PlayList> LoadPlayListAsync()
        {
            return await Task.Factory.StartNew(() => LoadPlayList());
        }

        private static PlayList LoadPlayList()
        {
            if (!File.Exists(PlayListFileName))
            {
                return null;
            }
            try
            {
                using (var fs = File.Open(PlayListFileName, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        using (var jr = new JsonTextReader(sr))
                        {
                            var serializer = new JsonSerializer();
                            var playList = serializer.Deserialize<PlayList>(jr);
                            return playList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            return null;
        }

        public static async void SavePlayListAsync(IEnumerable files)
        {
            var pl = new PlayList { Files = files.OfType<MediaFile>().ToList() };
            try
            {
                using (var fs = File.Open(PlayListFileName, FileMode.OpenOrCreate))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        using (var jw = new JsonTextWriter(sw))
                        {
                            jw.Formatting = Formatting.None;
                            var serializer = new JsonSerializer();
                            serializer.Serialize(jw, pl);

                            jw.Flush();
                            await sw.FlushAsync();
                            await fs.FlushAsync();

                            jw.Close();
                            sw.Close();
                            fs.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}