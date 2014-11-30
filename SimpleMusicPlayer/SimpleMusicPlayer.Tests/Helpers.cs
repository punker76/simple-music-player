using System.Collections.Generic;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Tests
{
    public static class Helpers
    {
        public const int MediaFilesTestCount = 10;

        public static IEnumerable<IMediaFile> GetSomeMediaFiles()
        {
            var theFiles = new List<IMediaFile>();
            for (var i = 0; i < MediaFilesTestCount; i++)
            {
                theFiles.Add(new MediaFile(string.Empty));
            }
            return theFiles;
        }
    }
}