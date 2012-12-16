using System;
using System.ComponentModel;
using System.IO;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Common;
using SimpleMusicPlayer.Interfaces;

namespace SimpleMusicPlayer.ViewModels
{
  public class MediaFileViewModel : ViewModelBaseNotifyPropertyChanged, IMediaFile
  {
    private string fullFileName;
    private string fileName;
    private string grouping;
    private string title;
    private string titleSort;
    private string[] albumArtists;
    private string[] albumArtistsSort;
    private string[] performers;
    private string[] performersSort;
    private string[] composers;
    private string[] composersSort;
    private string conductor;
    private string album;
    private string albumSort;
    private string comment;
    private string copyright;
    private string[] genres;
    private uint bpm;
    private uint year;
    private uint track;
    private uint trackCount;
    private uint disc;
    private uint discCount;
    private TimeSpan duration;
    private string firstAlbumArtist;
    private string firstPerformerAndTitle;
    private string firstAlbumArtistSort;
    private string firstComposer;
    private string firstComposerSort;
    private string firstGenre;
    private string firstPerformer;
    private string firstPerformerSort;
    private int playListIndex;
    private PlayerState state;

    public MediaFileViewModel(string fileName) {
      this.FullFileName = fileName;
      this.FileName = Path.GetFileName(fileName);
    }

    public static IMediaFile GetMediaFileViewModel(string fileName) {
      try {
        using (TagLib.File file = TagLib.File.Create(fileName)) {
          var mf = new MediaFileViewModel(fileName);

          mf.Grouping = file.Tag.Grouping;
          mf.Title = file.Tag.Title;
          mf.TitleSort = file.Tag.TitleSort;
          mf.AlbumArtists = file.Tag.AlbumArtists;
          mf.AlbumArtistsSort = file.Tag.AlbumArtistsSort;
          mf.Performers = file.Tag.Performers;
          mf.PerformersSort = file.Tag.PerformersSort;
          mf.Composers = file.Tag.Composers;
          mf.ComposersSort = file.Tag.ComposersSort;
          mf.Conductor = file.Tag.Conductor;
          mf.Album = file.Tag.Album;
          mf.AlbumSort = file.Tag.AlbumSort;
          mf.Comment = file.Tag.Comment;
          mf.Copyright = file.Tag.Copyright;
          mf.Genres = file.Tag.Genres;
          mf.BPM = file.Tag.BeatsPerMinute;
          mf.Year = file.Tag.Year;
          mf.Track = file.Tag.Track;
          mf.TrackCount = file.Tag.TrackCount;
          mf.Disc = file.Tag.Disc;
          mf.DiscCount = file.Tag.DiscCount;

          mf.FirstAlbumArtist = file.Tag.FirstAlbumArtist;
          mf.FirstAlbumArtistSort = file.Tag.FirstAlbumArtistSort;
          mf.FirstComposer = file.Tag.FirstComposer;
          mf.FirstComposerSort = file.Tag.FirstComposerSort;
          mf.FirstGenre = file.Tag.FirstGenre;
          mf.FirstPerformer = file.Tag.FirstPerformer;
          mf.FirstPerformerSort = file.Tag.FirstPerformerSort;

          var isFirstPerformerEmpty = string.IsNullOrWhiteSpace(mf.FirstPerformer);
          var isTitleEmpty = string.IsNullOrWhiteSpace(mf.Title);
          if (!isFirstPerformerEmpty && !isTitleEmpty) {
            mf.FirstPerformerAndTitle = string.Format("{0} - {1}", mf.FirstPerformer, mf.Title);
          } else if (!isFirstPerformerEmpty) {
            mf.FirstPerformerAndTitle = mf.FirstPerformer;
          } else if (!isTitleEmpty) {
            mf.FirstPerformerAndTitle = mf.Title;
          }

          if (file.Properties.MediaTypes != TagLib.MediaTypes.None) {
            mf.Duration = file.Properties.Duration;
          }

          return mf;
        }
      } catch (Exception e) {
        var em = e.Message;
        Console.WriteLine("Fail to parse file: {0}, {1}", fileName, e.ToString());
        return null;
      }
    }

    public string FirstPerformerSort {
      get { return this.firstPerformerSort; }
      set {
        if (Equals(value, this.firstPerformerSort)) {
          return;
        }
        this.firstPerformerSort = value;
        this.OnPropertyChanged(() => this.FirstPerformerSort);
      }
    }

    public string FirstPerformer {
      get { return this.firstPerformer; }
      set {
        if (Equals(value, this.firstPerformer)) {
          return;
        }
        this.firstPerformer = value;
        this.OnPropertyChanged(() => this.FirstPerformer);
      }
    }

    public string FirstGenre {
      get { return this.firstGenre; }
      set {
        if (Equals(value, this.firstGenre)) {
          return;
        }
        this.firstGenre = value;
        this.OnPropertyChanged(() => this.FirstGenre);
      }
    }

    public string FirstComposerSort {
      get { return this.firstComposerSort; }
      set {
        if (Equals(value, this.firstComposerSort)) {
          return;
        }
        this.firstComposerSort = value;
        this.OnPropertyChanged(() => this.FirstComposerSort);
      }
    }

    public string FirstComposer {
      get { return this.firstComposer; }
      set {
        if (Equals(value, this.firstComposer)) {
          return;
        }
        this.firstComposer = value;
        this.OnPropertyChanged(() => this.FirstComposer);
      }
    }

    public string FirstAlbumArtistSort {
      get { return this.firstAlbumArtistSort; }
      set {
        if (Equals(value, this.firstAlbumArtistSort)) {
          return;
        }
        this.firstAlbumArtistSort = value;
        this.OnPropertyChanged(() => this.FirstAlbumArtistSort);
      }
    }

    public string FirstAlbumArtist {
      get { return this.firstAlbumArtist; }
      set {
        if (Equals(value, this.firstAlbumArtist)) {
          return;
        }
        this.firstAlbumArtist = value;
        this.OnPropertyChanged(() => this.FirstAlbumArtist);
      }
    }

    public string FirstPerformerAndTitle {
      get { return this.firstPerformerAndTitle; }
      set {
        if (Equals(value, this.firstPerformerAndTitle)) {
          return;
        }
        this.firstPerformerAndTitle = value;
        this.OnPropertyChanged(() => this.FirstPerformerAndTitle);
      }
    }

    public TimeSpan Duration {
      get { return this.duration; }
      set {
        if (Equals(value, this.duration)) {
          return;
        }
        this.duration = value;
        this.OnPropertyChanged(() => this.Duration);
      }
    }

    public uint DiscCount {
      get { return this.discCount; }
      set {
        if (Equals(value, this.discCount)) {
          return;
        }
        this.discCount = value;
        this.OnPropertyChanged(() => this.DiscCount);
      }
    }

    public uint Disc {
      get { return this.disc; }
      set {
        if (Equals(value, this.disc)) {
          return;
        }
        this.disc = value;
        this.OnPropertyChanged(() => this.Disc);
      }
    }

    public uint TrackCount {
      get { return this.trackCount; }
      set {
        if (Equals(value, this.trackCount)) {
          return;
        }
        this.trackCount = value;
        this.OnPropertyChanged(() => this.TrackCount);
      }
    }

    public uint Track {
      get { return this.track; }
      set {
        if (Equals(value, this.track)) {
          return;
        }
        this.track = value;
        this.OnPropertyChanged(() => this.Track);
      }
    }

    public uint Year {
      get { return this.year; }
      set {
        if (Equals(value, this.year)) {
          return;
        }
        this.year = value;
        this.OnPropertyChanged(() => this.Year);
      }
    }

    public uint BPM {
      get { return this.bpm; }
      set {
        if (Equals(value, this.bpm)) {
          return;
        }
        this.bpm = value;
        this.OnPropertyChanged(() => this.BPM);
      }
    }

    public string[] Genres {
      get { return this.genres; }
      set {
        if (Equals(value, this.genres)) {
          return;
        }
        this.genres = value;
        this.OnPropertyChanged(() => this.Genres);
      }
    }

    public string Copyright {
      get { return this.copyright; }
      set {
        if (Equals(value, this.copyright)) {
          return;
        }
        this.copyright = value;
        this.OnPropertyChanged(() => this.Copyright);
      }
    }

    public string Comment {
      get { return this.comment; }
      set {
        if (Equals(value, this.comment)) {
          return;
        }
        this.comment = value;
        this.OnPropertyChanged(() => this.Comment);
      }
    }

    public string AlbumSort {
      get { return this.albumSort; }
      set {
        if (Equals(value, this.albumSort)) {
          return;
        }
        this.albumSort = value;
        this.OnPropertyChanged(() => this.AlbumSort);
      }
    }

    public string Album {
      get { return this.album; }
      set {
        if (Equals(value, this.album)) {
          return;
        }
        this.album = value;
        this.OnPropertyChanged(() => this.Album);
      }
    }

    public string Conductor {
      get { return this.conductor; }
      set {
        if (Equals(value, this.conductor)) {
          return;
        }
        this.conductor = value;
        this.OnPropertyChanged(() => this.Conductor);
      }
    }

    public string[] ComposersSort {
      get { return this.composersSort; }
      set {
        if (Equals(value, this.composersSort)) {
          return;
        }
        this.composersSort = value;
        this.OnPropertyChanged(() => this.ComposersSort);
      }
    }

    public string[] Composers {
      get { return this.composers; }
      set {
        if (Equals(value, this.composers)) {
          return;
        }
        this.composers = value;
        this.OnPropertyChanged(() => this.Composers);
      }
    }

    public string[] PerformersSort {
      get { return this.performersSort; }
      set {
        if (Equals(value, this.performersSort)) {
          return;
        }
        this.performersSort = value;
        this.OnPropertyChanged(() => this.PerformersSort);
      }
    }

    public string[] Performers {
      get { return this.performers; }
      set {
        if (Equals(value, this.performers)) {
          return;
        }
        this.performers = value;
        this.OnPropertyChanged(() => this.Performers);
      }
    }

    public string[] AlbumArtistsSort {
      get { return this.albumArtistsSort; }
      set {
        if (Equals(value, this.albumArtistsSort)) {
          return;
        }
        this.albumArtistsSort = value;
        this.OnPropertyChanged(() => this.AlbumArtistsSort);
      }
    }

    public string[] AlbumArtists {
      get { return this.albumArtists; }
      set {
        if (Equals(value, this.albumArtists)) {
          return;
        }
        this.albumArtists = value;
        this.OnPropertyChanged(() => this.AlbumArtists);
      }
    }

    public string TitleSort {
      get { return this.titleSort; }
      set {
        if (Equals(value, this.titleSort)) {
          return;
        }
        this.titleSort = value;
        this.OnPropertyChanged(() => this.TitleSort);
      }
    }

    public string Title {
      get { return this.title; }
      set {
        if (Equals(value, this.title)) {
          return;
        }
        this.title = value;
        this.OnPropertyChanged(() => this.Title);
      }
    }

    public string Grouping {
      get { return this.grouping; }
      set {
        if (Equals(value, this.grouping)) {
          return;
        }
        this.grouping = value;
        this.OnPropertyChanged(() => this.Grouping);
      }
    }

    public string FullFileName {
      get { return this.fullFileName; }
      set {
        if (Equals(value, this.fullFileName)) {
          return;
        }
        this.fullFileName = value;
        this.OnPropertyChanged(() => this.FullFileName);
      }
    }

    public string FileName {
      get { return this.fileName; }
      set {
        if (Equals(value, this.fileName)) {
          return;
        }
        this.fileName = value;
        this.OnPropertyChanged(() => this.FileName);
      }
    }

    public PlayerState State {
      get { return this.state; }
      set {
        if (Equals(value, this.state)) {
          return;
        }
        this.state = value;
        this.OnPropertyChanged(() => this.State);
      }
    }

    public int PlayListIndex {
      get { return this.playListIndex; }
      set {
        if (Equals(value, this.playListIndex)) {
          return;
        }
        this.playListIndex = value;
        this.OnPropertyChanged(() => this.PlayListIndex);
      }
    }

    [Browsable(false)]
    public object PlayList { get; set; }

    public override string ToString() {
      return string.Format("{0} {1} - {2} {3:m\\:ss}", this.Track, this.FirstAlbumArtist, this.Title, this.Duration);
    }
  }
}