using System.Collections.Generic;
using System.Linq;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;
using Xunit;

namespace SimpleMusicPlayer.Tests
{
  public class PlayListCollectionTest
  {
    [Fact]
    public void CheckPlayListIndexAfterCreatingNewCollection()
    {
      var thePlayListCollection = new PlayListCollection(Helpers.GetSomeMediaFiles());

      Assert.Equal(Helpers.MediaFilesTestCount, thePlayListCollection.Count);
      CheckTheIndicesForAllMediaFiles(thePlayListCollection);
    }

    [Fact]
    public void CheckPlayListIndexAfterAddingAndRemovingFiles()
    {
      var thePlayListCollection = new PlayListCollection(Helpers.GetSomeMediaFiles());

      Assert.Equal(Helpers.MediaFilesTestCount, thePlayListCollection.Count);
      CheckTheIndicesForAllMediaFiles(thePlayListCollection);

      thePlayListCollection.AddItems(Helpers.GetSomeMediaFiles().Take(5));
      thePlayListCollection.RemoveAt(2);
      thePlayListCollection.RemoveAt(5);

      Assert.Equal(Helpers.MediaFilesTestCount + 5 - 2, thePlayListCollection.Count);
      CheckTheIndicesForAllMediaFiles(thePlayListCollection);
    }

    [Fact]
    public void CheckPlayListIndexAfterRemovingFiles()
    {
      var thePlayListCollection = new PlayListCollection(Helpers.GetSomeMediaFiles());

      Assert.Equal(Helpers.MediaFilesTestCount, thePlayListCollection.Count);
      CheckTheIndicesForAllMediaFiles(thePlayListCollection);

      thePlayListCollection.RemoveAt(2);
      thePlayListCollection.RemoveAt(5);

      Assert.Equal(Helpers.MediaFilesTestCount - 2, thePlayListCollection.Count);
      CheckTheIndicesForAllMediaFiles(thePlayListCollection);
    }

    [Fact]
    public void CheckPlayListIndexAfterBulkRemovingFiles()
    {
      var thePlayListCollection = new PlayListCollection(Helpers.GetSomeMediaFiles());

      Assert.Equal(Helpers.MediaFilesTestCount, thePlayListCollection.Count);
      CheckTheIndicesForAllMediaFiles(thePlayListCollection);

      var items2Remove = new List<IMediaFile>();
      for (var i = 3; i < 7; i++)
      {
        items2Remove.Add(thePlayListCollection[i]);
      }
      thePlayListCollection.RemoveItems(items2Remove);

      Assert.Equal(Helpers.MediaFilesTestCount - items2Remove.Count, thePlayListCollection.Count);
      CheckTheIndicesForAllMediaFiles(thePlayListCollection);
    }

    public void CheckTheIndicesForAllMediaFiles(PlayListCollection thePlayListCollection)
    {
      for (var i = 0; i < thePlayListCollection.Count; i++)
      {
        Assert.Equal(i + 1, thePlayListCollection[i].PlayListIndex);
      }
    }
  }
}