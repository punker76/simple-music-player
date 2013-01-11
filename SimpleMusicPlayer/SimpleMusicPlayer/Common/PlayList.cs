using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Common
{
  public class PlayList
  {
    [JsonIgnore]
    public const string PlayListFileName = "playlist.smppl";

    public List<MediaFileViewModel> Files { get; set; }

    public static async Task<PlayList> GetPlayListAsync() {
      if (File.Exists(PlayListFileName)) {
        var fileText = File.ReadAllText(PlayListFileName);
        var playList = await JsonConvert.DeserializeObjectAsync<PlayList>(fileText);
        return playList;
      }
      return null;
    }
  }
}