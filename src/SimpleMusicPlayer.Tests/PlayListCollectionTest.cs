using System.Collections.Generic;
using System.Linq;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Interfaces;
using Xunit;

namespace SimpleMusicPlayer.Tests
{
    public class PlayListCollectionTest
    {
        private readonly PlayListCollection thePlayListCollection;

        public PlayListCollectionTest()
        {
            this.thePlayListCollection = new PlayListCollection(Helpers.GetSomeMediaFiles());
        }

        [Fact]
        public void CheckPlayListIndexAfterCreatingNewCollection()
        {
            Assert.Equal(Helpers.MediaFilesTestCount, this.thePlayListCollection.Count);
            CheckTheIndicesForAllMediaFilesInPlayList();
        }

        [Fact]
        public void CheckPlayListIndexAfterAddingAndRemovingFiles()
        {
            Assert.Equal(Helpers.MediaFilesTestCount, this.thePlayListCollection.Count);

            this.thePlayListCollection.AddItems(Helpers.GetSomeMediaFiles().Take(5));
            this.thePlayListCollection.RemoveAt(2);
            this.thePlayListCollection.RemoveAt(5);

            Assert.Equal(Helpers.MediaFilesTestCount + 5 - 2, this.thePlayListCollection.Count);
            CheckTheIndicesForAllMediaFilesInPlayList();
        }

        [Fact]
        public void CheckPlayListIndexAfterRemovingFiles()
        {
            Assert.Equal(Helpers.MediaFilesTestCount, this.thePlayListCollection.Count);

            this.thePlayListCollection.RemoveAt(2);
            this.thePlayListCollection.RemoveAt(5);

            Assert.Equal(Helpers.MediaFilesTestCount - 2, this.thePlayListCollection.Count);
            CheckTheIndicesForAllMediaFilesInPlayList();
        }

        [Fact]
        public void CheckPlayListIndexAfterBulkRemovingFiles()
        {
            Assert.Equal(Helpers.MediaFilesTestCount, this.thePlayListCollection.Count);

            var items2Remove = new List<IMediaFile>();
            for (var i = 3; i < 7; i++)
            {
                items2Remove.Add(this.thePlayListCollection[i]);
            }
            this.thePlayListCollection.RemoveItems(items2Remove);

            Assert.Equal(Helpers.MediaFilesTestCount - items2Remove.Count, this.thePlayListCollection.Count);
            CheckTheIndicesForAllMediaFilesInPlayList();
        }

        [Fact]
        public void CheckPlayListIndexAfterInsertingNewFiles()
        {
            Assert.Equal(Helpers.MediaFilesTestCount, this.thePlayListCollection.Count);

            this.thePlayListCollection.AddItems(Helpers.GetSomeMediaFiles().Take(5), 0);

            Assert.Equal(Helpers.MediaFilesTestCount + 5, this.thePlayListCollection.Count);
            CheckTheIndicesForAllMediaFilesInPlayList();
        }

        /// <summary>
        /// The inserted files should be at the right position.
        /// </summary>
        [Fact]
        public void CheckFilesInPlayListAfterInsertingNewFiles()
        {
            Assert.Equal(Helpers.MediaFilesTestCount, this.thePlayListCollection.Count);

            var newFiles = Helpers.GetSomeMediaFiles().Take(5).ToList();
            this.thePlayListCollection.AddItems(newFiles, 0);

            Assert.Equal(newFiles, this.thePlayListCollection.Take(5));
        }

        private void CheckTheIndicesForAllMediaFilesInPlayList()
        {
            for (var i = 0; i < this.thePlayListCollection.Count; i++)
            {
                Assert.Equal(i + 1, this.thePlayListCollection[i].PlayListIndex);
            }
        }
    }
}