using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SimpleMusicPlayer.Core
{
    public class PlayList
    {
        [JsonIgnore]
        public const string PlayListFileName = "playlist.smppl";

        public List<MediaFile> Files { get; set; }

        public static async Task<PlayList> LoadAsync()
        {
            if (!File.Exists(PlayListFileName))
            {
                return null;
            }
            try
            {
                using (StreamReader file = await Task.Run(() => File.OpenText(PlayListFileName)))
                {
                    var serializer = new JsonSerializer();
                    return (PlayList)serializer.Deserialize(file, typeof(PlayList));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                using (StreamWriter file = await Task.Run(() => File.CreateText(PlayListFileName)))
                {
                    file.AutoFlush = true;
                    var serializer = new JsonSerializer();
                    serializer.Serialize(file, this);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}