using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;
using TagLib;

namespace SimpleMusicPlayer.Models
{
    public class MediaFile : ViewModelBase, IMediaFile
    {
        private const string UnknownTag = "<Unknown>";
        private string fullFileName;
        private string fileName;
        private string grouping;
        private string title;
        private string titleSort;
        private string conductor;
        private string album;
        private string albumSort;
        private string comment;
        private string copyright;
        private uint bpm;
        private uint year;
        private uint track;
        private uint trackCount;
        private uint disc;
        private uint discCount;
        private TimeSpan duration;
        private bool isVBR;
        private int audioBitrate;
        private int audioSampleRate;
        private string firstAlbumArtist;
        private string firstPerformerAndTitle;
        private string firstPerformerAndAlbum;
        private string firstAlbumArtistSort;
        private string firstComposer;
        private string firstComposerSort;
        private string firstGenre;
        private string firstPerformer;
        private string firstPerformerSort;
        private int playListIndex;
        private PlayerState state;

        public MediaFile(string fileName)
        {
            this.FullFileName = fileName;
            this.FileName = Path.GetFileName(fileName);
        }

        public static IMediaFile GetMediaFileViewModel(string fileName)
        {
            try
            {
                using (TagLib.File file = TagLib.File.Create(fileName))
                {
                    var mf = new MediaFile(fileName);

                    // ALBUM -> iTunes=Album, WMP10=Album, Winamp=Album
                    mf.album = file.Tag.Album;
                    if (string.IsNullOrWhiteSpace(mf.album))
                    {
                        mf.album = UnknownTag;
                    }
                    mf.albumSort = file.Tag.AlbumSort;

                    // ALBUMARTIST
                    var albumArtists = file.Tag.AlbumArtists.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    var albumArtistsSort = file.Tag.AlbumArtistsSort.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    mf.firstAlbumArtist = albumArtists.Count > 1 ? string.Join("/", albumArtists) : file.Tag.FirstAlbumArtist;
                    mf.firstAlbumArtistSort = albumArtistsSort.Count > 1 ? string.Join("/", albumArtistsSort) : file.Tag.FirstAlbumArtistSort;

                    // ARTIST/Performer
                    var performers = file.Tag.Performers.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    var performersSort = file.Tag.PerformersSort.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    mf.firstPerformer = performers.Count > 1 ? string.Join("/", performers) : file.Tag.FirstPerformer;
                    if (string.IsNullOrWhiteSpace(mf.firstPerformer))
                    {
                        mf.firstPerformer = UnknownTag;
                    }
                    mf.firstPerformerSort = performersSort.Count > 1 ? string.Join("/", performersSort) : file.Tag.FirstPerformerSort;
                    if (string.IsNullOrWhiteSpace(mf.firstPerformerSort))
                    {
                        mf.firstPerformerSort = UnknownTag;
                    }

                    // BPM
                    mf.bpm = file.Tag.BeatsPerMinute;

                    // COMMENT
                    mf.comment = file.Tag.Comment;

                    // COMPOSER
                    var composers = file.Tag.Composers.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    var composersSort = file.Tag.ComposersSort.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    mf.firstComposer = composers.Count > 1 ? string.Join("/", composers) : file.Tag.FirstComposer;
                    mf.firstComposerSort = composersSort.Count > 1 ? string.Join("/", composersSort) : file.Tag.FirstComposerSort;

                    // CONDUCTOR
                    mf.conductor = file.Tag.Conductor;

                    // COPYRIGHT
                    mf.copyright = file.Tag.Copyright;

                    // TITLE
                    mf.title = file.Tag.Title;
                    mf.titleSort = file.Tag.TitleSort;

                    // GENRE
                    var genres = file.Tag.Genres.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    mf.firstGenre = genres.Count > 1 ? string.Join("/", genres) : file.Tag.FirstGenre;
                    if (string.IsNullOrWhiteSpace(mf.firstGenre))
                    {
                        mf.firstGenre = UnknownTag;
                    }

                    mf.track = file.Tag.Track;
                    mf.trackCount = file.Tag.TrackCount;
                    mf.disc = file.Tag.Disc;
                    mf.discCount = file.Tag.DiscCount;
                    mf.year = file.Tag.Year;
                    mf.grouping = file.Tag.Grouping;

                    var isFirstPerformerEmpty = string.IsNullOrWhiteSpace(mf.firstPerformer);
                    var isTitleEmpty = string.IsNullOrWhiteSpace(mf.title);
                    if (!isFirstPerformerEmpty && !isTitleEmpty)
                    {
                        mf.firstPerformerAndTitle = string.Concat(mf.firstPerformer, " - ", mf.title);
                    }
                    else if (!isFirstPerformerEmpty)
                    {
                        mf.firstPerformerAndTitle = mf.firstPerformer;
                    }
                    else if (!isTitleEmpty)
                    {
                        mf.firstPerformerAndTitle = mf.title;
                    }
                    else
                    {
                        mf.firstPerformerAndTitle = Path.GetFileNameWithoutExtension(mf.fileName);
                    }

                    var isAlbumEmpty = string.IsNullOrWhiteSpace(mf.album);
                    if (!isFirstPerformerEmpty && !isAlbumEmpty)
                    {
                        mf.firstPerformerAndAlbum = string.Concat(mf.firstPerformer, " - ", mf.album);
                    }
                    else if (!isFirstPerformerEmpty)
                    {
                        mf.firstPerformerAndAlbum = mf.firstPerformer;
                    }
                    else if (!isAlbumEmpty)
                    {
                        mf.firstPerformerAndAlbum = mf.album;
                    }
                    else
                    {
                        mf.firstPerformerAndAlbum = Path.GetFileNameWithoutExtension(mf.fileName);
                    }

                    if (file.Properties.MediaTypes != TagLib.MediaTypes.None)
                    {
                        mf.duration = file.Properties.Duration;
                        var codec = file.Properties.Codecs.FirstOrDefault(c => c is TagLib.Mpeg.AudioHeader);
                        mf.isVBR = codec != null && (((TagLib.Mpeg.AudioHeader)codec).VBRIHeader.Present || ((TagLib.Mpeg.AudioHeader)codec).XingHeader.Present);

                        mf.audioBitrate = file.Properties.AudioBitrate;
                        mf.audioSampleRate = file.Properties.AudioSampleRate;
                    }

                    return mf;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Fail to parse file: {0}, {1}", fileName, e);
                return null;
            }
        }

        public string FirstPerformerSort
        {
            get { return this.firstPerformerSort; }
            set
            {
                if (Equals(value, this.firstPerformerSort))
                {
                    return;
                }
                this.firstPerformerSort = value;
                this.OnPropertyChanged(() => this.FirstPerformerSort);
            }
        }

        public string FirstPerformer
        {
            get { return this.firstPerformer; }
            set
            {
                if (Equals(value, this.firstPerformer))
                {
                    return;
                }
                this.firstPerformer = value;
                this.OnPropertyChanged(() => this.FirstPerformer);
            }
        }

        public string FirstGenre
        {
            get { return this.firstGenre; }
            set
            {
                if (Equals(value, this.firstGenre))
                {
                    return;
                }
                this.firstGenre = value;
                this.OnPropertyChanged(() => this.FirstGenre);
            }
        }

        public string FirstComposerSort
        {
            get { return this.firstComposerSort; }
            set
            {
                if (Equals(value, this.firstComposerSort))
                {
                    return;
                }
                this.firstComposerSort = value;
                this.OnPropertyChanged(() => this.FirstComposerSort);
            }
        }

        public string FirstComposer
        {
            get { return this.firstComposer; }
            set
            {
                if (Equals(value, this.firstComposer))
                {
                    return;
                }
                this.firstComposer = value;
                this.OnPropertyChanged(() => this.FirstComposer);
            }
        }

        public string FirstAlbumArtistSort
        {
            get { return this.firstAlbumArtistSort; }
            set
            {
                if (Equals(value, this.firstAlbumArtistSort))
                {
                    return;
                }
                this.firstAlbumArtistSort = value;
                this.OnPropertyChanged(() => this.FirstAlbumArtistSort);
            }
        }

        public string FirstAlbumArtist
        {
            get { return this.firstAlbumArtist; }
            set
            {
                if (Equals(value, this.firstAlbumArtist))
                {
                    return;
                }
                this.firstAlbumArtist = value;
                this.OnPropertyChanged(() => this.FirstAlbumArtist);
            }
        }

        public string FirstPerformerAndTitle
        {
            get { return this.firstPerformerAndTitle; }
            set
            {
                if (Equals(value, this.firstPerformerAndTitle))
                {
                    return;
                }
                this.firstPerformerAndTitle = value;
                this.OnPropertyChanged(() => this.FirstPerformerAndTitle);
            }
        }

        public string FirstPerformerAndAlbum
        {
            get { return this.firstPerformerAndAlbum; }
            set
            {
                if (Equals(value, this.firstPerformerAndAlbum))
                {
                    return;
                }
                this.firstPerformerAndAlbum = value;
                this.OnPropertyChanged(() => this.FirstPerformerAndAlbum);
            }
        }

        public TimeSpan Duration
        {
            get { return this.duration; }
            set
            {
                if (Equals(value, this.duration))
                {
                    return;
                }
                this.duration = value;
                this.OnPropertyChanged(() => this.Duration);
            }
        }

        public bool IsVBR
        {
            get { return this.isVBR; }
            set
            {
                if (Equals(value, this.isVBR))
                {
                    return;
                }
                this.isVBR = value;
                this.OnPropertyChanged(() => this.IsVBR);
            }
        }

        public int AudioBitrate
        {
            get { return this.audioBitrate; }
            set
            {
                if (Equals(value, this.audioBitrate))
                {
                    return;
                }
                this.audioBitrate = value;
                this.OnPropertyChanged(() => this.AudioBitrate);
            }
        }

        public int AudioSampleRate
        {
            get { return this.audioSampleRate; }
            set
            {
                if (Equals(value, this.audioSampleRate))
                {
                    return;
                }
                this.audioSampleRate = value;
                this.OnPropertyChanged(() => this.AudioSampleRate);
            }
        }

        public uint DiscCount
        {
            get { return this.discCount; }
            set
            {
                if (Equals(value, this.discCount))
                {
                    return;
                }
                this.discCount = value;
                this.OnPropertyChanged(() => this.DiscCount);
            }
        }

        public uint Disc
        {
            get { return this.disc; }
            set
            {
                if (Equals(value, this.disc))
                {
                    return;
                }
                this.disc = value;
                this.OnPropertyChanged(() => this.Disc);
            }
        }

        public uint TrackCount
        {
            get { return this.trackCount; }
            set
            {
                if (Equals(value, this.trackCount))
                {
                    return;
                }
                this.trackCount = value;
                this.OnPropertyChanged(() => this.TrackCount);
            }
        }

        public uint Track
        {
            get { return this.track; }
            set
            {
                if (Equals(value, this.track))
                {
                    return;
                }
                this.track = value;
                this.OnPropertyChanged(() => this.Track);
            }
        }

        public uint Year
        {
            get { return this.year; }
            set
            {
                if (Equals(value, this.year))
                {
                    return;
                }
                this.year = value;
                this.OnPropertyChanged(() => this.Year);
            }
        }

        public uint BPM
        {
            get { return this.bpm; }
            set
            {
                if (Equals(value, this.bpm))
                {
                    return;
                }
                this.bpm = value;
                this.OnPropertyChanged(() => this.BPM);
            }
        }

        public string Copyright
        {
            get { return this.copyright; }
            set
            {
                if (Equals(value, this.copyright))
                {
                    return;
                }
                this.copyright = value;
                this.OnPropertyChanged(() => this.Copyright);
            }
        }

        public string Comment
        {
            get { return this.comment; }
            set
            {
                if (Equals(value, this.comment))
                {
                    return;
                }
                this.comment = value;
                this.OnPropertyChanged(() => this.Comment);
            }
        }

        public string AlbumSort
        {
            get { return this.albumSort; }
            set
            {
                if (Equals(value, this.albumSort))
                {
                    return;
                }
                this.albumSort = value;
                this.OnPropertyChanged(() => this.AlbumSort);
            }
        }

        public string Album
        {
            get { return this.album; }
            set
            {
                if (Equals(value, this.album))
                {
                    return;
                }
                this.album = value;
                this.OnPropertyChanged(() => this.Album);
            }
        }

        public string Conductor
        {
            get { return this.conductor; }
            set
            {
                if (Equals(value, this.conductor))
                {
                    return;
                }
                this.conductor = value;
                this.OnPropertyChanged(() => this.Conductor);
            }
        }

        public string TitleSort
        {
            get { return this.titleSort; }
            set
            {
                if (Equals(value, this.titleSort))
                {
                    return;
                }
                this.titleSort = value;
                this.OnPropertyChanged(() => this.TitleSort);
            }
        }

        public string Title
        {
            get { return this.title; }
            set
            {
                if (Equals(value, this.title))
                {
                    return;
                }
                this.title = value;
                this.OnPropertyChanged(() => this.Title);
            }
        }

        public string Grouping
        {
            get { return this.grouping; }
            set
            {
                if (Equals(value, this.grouping))
                {
                    return;
                }
                this.grouping = value;
                this.OnPropertyChanged(() => this.Grouping);
            }
        }

        public string FullFileName
        {
            get { return this.fullFileName; }
            set
            {
                if (Equals(value, this.fullFileName))
                {
                    return;
                }
                this.fullFileName = value;
                this.OnPropertyChanged(() => this.FullFileName);
            }
        }

        public string FileName
        {
            get { return this.fileName; }
            set
            {
                if (Equals(value, this.fileName))
                {
                    return;
                }
                this.fileName = value;
                this.OnPropertyChanged(() => this.FileName);
            }
        }

        [JsonIgnore]
        public PlayerState State
        {
            get { return this.state; }
            set
            {
                if (Equals(value, this.state))
                {
                    return;
                }
                this.state = value;
                this.OnPropertyChanged(() => this.State);
            }
        }

        [Browsable(false)]
        [JsonIgnore]
        public BitmapImage Cover
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.FullFileName) || !System.IO.File.Exists(this.FullFileName))
                {
                    return null;
                }
                try
                {
                    using (var file = TagLib.File.Create(this.FullFileName))
                    {
                        var pictures = file.Tag.Pictures;
                        if (pictures != null)
                        {
                            var pic = pictures.FirstOrDefault(p => p.Type == PictureType.FrontCover);
                            if (pic != null)
                            {
                                var bi = new BitmapImage();
                                bi.BeginInit();
                                bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                                bi.CacheOption = BitmapCacheOption.OnDemand;
                                bi.StreamSource = new MemoryStream(pic.Data.Data);
                                bi.EndInit();
                                bi.Freeze();
                                return bi;
                            }
                            else
                            {
                                var path2Image = Path.GetDirectoryName(this.FullFileName);
                                var cover = !string.IsNullOrEmpty(path2Image) ? Directory.EnumerateFiles(path2Image, "folder.*").FirstOrDefault() : null;
                                if (!string.IsNullOrEmpty(cover))
                                {
                                    var bi = new BitmapImage();
                                    bi.BeginInit();
                                    bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                                    bi.CacheOption = BitmapCacheOption.OnDemand;
                                    bi.UriSource = new Uri(cover, UriKind.RelativeOrAbsolute);
                                    bi.EndInit();
                                    bi.Freeze();
                                    return bi;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Fail to load cover: {0}, {1}", this.FullFileName, e);
                }
                return null;
            }
        }

        [JsonIgnore]
        public int PlayListIndex
        {
            get { return this.playListIndex; }
            set
            {
                if (Equals(value, this.playListIndex))
                {
                    return;
                }
                this.playListIndex = value;
                this.OnPropertyChanged(() => this.PlayListIndex);
            }
        }

        //[Browsable(false)]
        //[JsonIgnore]
        //public object PlayList { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} - {2} {3:m\\:ss}", this.Track, this.FirstAlbumArtist, this.Title, this.Duration);
        }
    }
}