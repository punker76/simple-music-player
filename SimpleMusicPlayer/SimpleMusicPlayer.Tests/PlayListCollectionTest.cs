using SimpleMusicPlayer.Common;
using Xunit;

namespace SimpleMusicPlayer.Tests
{
  public class PlayListCollectionTest
  {
    [Fact]
    public void CheckIndexOfMediaFilesAfterCreatingNewPlayListCollection()
    {
      var thePlayListCollection = new PlayListCollection(Helpers.GetSomeMediaFiles());

      Assert.Equal(Helpers.MediaFilesTestCount, thePlayListCollection.Count);
    }

    public void CheckTheIndicesForAllMediaFiles(PlayListCollection thePlayListCollection)
    {
      for (var i = 0; i < Helpers.MediaFilesTestCount; i++)
      {
        Assert.Equal(i + 1, thePlayListCollection[i].PlayListIndex);
      }
    }
  }
}