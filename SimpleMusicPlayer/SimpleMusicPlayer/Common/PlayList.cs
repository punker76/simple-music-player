using System.Collections;
using System.Collections.Generic;
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

    public static async Task<PlayList> GetPlayListAsync() {
      if (File.Exists(PlayListFileName)) {
        var fileText = File.ReadAllText(PlayListFileName);
        var playList = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<PlayList>(fileText));
        return playList;
      }
      return null;
    }

    public static async void SavePlayListAsync(IEnumerable files) {
      await Task.Factory.StartNew(() => {
                                    var pl = new PlayList { Files = files.OfType<MediaFile>().ToList() };
                                    var playListAsJson = JsonConvert.SerializeObject(pl, Formatting.None);
                                    File.WriteAllText(PlayListFileName, playListAsJson);
                                  });
    }
  }
}