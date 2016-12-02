using System.Collections.Generic;
using SimpleMusicPlayer.Core.Interfaces;

namespace SimpleMusicPlayer.Core
{
    public class MedialibCollection : QuickFillObservableCollection<IMediaFile>
    {
        public MedialibCollection(IEnumerable<IMediaFile> files)
            : base(files)
        {
        }
    }
}