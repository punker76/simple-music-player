using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.Core.Player;
using TinyIoC;

namespace SimpleMusicPlayer.ViewModels
{
    public class MedialibViewModel : ReactiveObject
    {
        public MedialibViewModel()
        {
            this.PlayerSettings = TinyIoCContainer.Current.Resolve<PlayerSettings>();
            this.FileSearchWorker = new FileSearchWorker("Medialib", MediaFile.GetMediaFileViewModel);
            this.MediaFiles = CollectionViewSource.GetDefaultView(new MedialibCollection(null));

            // Do a selection/filtering when nothing new has been changed for 400 ms and it isn't
            // an empty string... and don't filter for the same thing twice.

            this.ObservableForProperty(x => x.SelectedGenre)
              .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler)
              .Select(x => x.Value)
                //.Where(x => !string.IsNullOrWhiteSpace(x))
              .DistinctUntilChanged()
              .Subscribe(x => FilterByGenreSelection(x));

            this.ObservableForProperty(x => x.SelectedArtist)
              .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler)
              .Select(x => x.Value)
                //.Where(x => !string.IsNullOrWhiteSpace(x)/* && !string.IsNullOrEmpty(this.SelectedGenre)*/)
              .DistinctUntilChanged()
              .Subscribe(x => FilterByArtistSelection(this.SelectedGenre, x));

            this.ObservableForProperty(x => x.SelectedAlbum)
              .Throttle(TimeSpan.FromMilliseconds(400), RxApp.MainThreadScheduler)
              .Select(x => x.Value)
                //.Where(x => !string.IsNullOrWhiteSpace(x)/* && !string.IsNullOrEmpty(this.SelectedGenre) && !string.IsNullOrEmpty(this.SelectedArtist)*/)
              .DistinctUntilChanged()
              .Subscribe(x => FilterByAlbumSelection());

            this.AddDirectoryCommand = ReactiveCommand.CreateFromTask(
                x => AddDirectoryAsync(),
                this.WhenAny(x => x.FileSearchWorker.IsWorking, isworking => !isworking.Value));
        }

        public FileSearchWorker FileSearchWorker { get; private set; }

        public PlayerSettings PlayerSettings { get; private set; }

        public CustomWindowPlacementSettings WindowPlacementSettings { get; set; }

        private IEnumerable mediaFiles;

        public IEnumerable MediaFiles
        {
            get => this.mediaFiles;
            set => this.RaiseAndSetIfChanged(ref mediaFiles, value);
        }

        private IEnumerable genreList;

        public IEnumerable GenreList
        {
            get => this.genreList;
            set => this.RaiseAndSetIfChanged(ref genreList, value);
        }

        private string selectedGenre;

        public string SelectedGenre
        {
            get => this.selectedGenre;
            set => this.RaiseAndSetIfChanged(ref selectedGenre, value);
        }

        private IEnumerable artistList;

        public IEnumerable ArtistList
        {
            get => this.artistList;
            set => this.RaiseAndSetIfChanged(ref artistList, value);
        }

        private string selectedArtist;

        public string SelectedArtist
        {
            get => this.selectedArtist;
            set => this.RaiseAndSetIfChanged(ref selectedArtist, value);
        }

        private IEnumerable albumList;

        public IEnumerable AlbumList
        {
            get => this.albumList;
            set => this.RaiseAndSetIfChanged(ref albumList, value);
        }

        private string selectedAlbum;

        public string SelectedAlbum
        {
            get => this.selectedAlbum;
            set => this.RaiseAndSetIfChanged(ref selectedAlbum, value);
        }

        public ReactiveCommand<Unit, Unit> AddDirectoryCommand { get; protected set; }

        private async Task AddDirectoryAsync()
        {
            var owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.DataContext == this);
            var directories = FolderBrowserHelper.GetFolders(owner);
            if (directories.Any())
            {
                await this.HandleDropActionAsync(directories.ToList());
            }
        }

        public void OnDragOverAction(DragEventArgs e)
        {
            e.Effects = !this.FileSearchWorker.IsWorking && e.Data.GetDataPresent(DataFormats.FileDrop)
                ? DragDropEffects.Copy
                : DragDropEffects.None;
            Console.WriteLine(">> drag over >> {0}", DateTime.Now);
            e.Handled = true;
        }

        public async Task OnDropAction(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Get data object
                var dataObject = e.Data as DataObject;
                if (dataObject != null && dataObject.ContainsFileDropList())
                {
                    await this.HandleDropActionAsync(dataObject.GetFileDropList());
                }
            }
        }

        public async Task HandleDropActionAsync(IList fileOrDirDropList)
        {
            var files = await this.FileSearchWorker.StartSearchAsync(fileOrDirDropList);

            var collView = CollectionViewSource.GetDefaultView(new MedialibCollection(files));
            collView.Filter = o => {
                var m = (IMediaFile)o;
                if (!string.IsNullOrWhiteSpace(this.SelectedGenre) && !Equals(m.FirstGenre, this.SelectedGenre))
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(this.SelectedArtist) && !Equals(m.FirstPerformer, this.SelectedArtist))
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(this.SelectedAlbum) && !Equals(m.Album, this.SelectedAlbum))
                {
                    return false;
                }
                return true;
            };
            this.MediaFiles = collView;

            this.GenreList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.FirstGenre).OrderBy(g => g.Key).Select(g => g.Key));
            this.ArtistList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.FirstPerformer).OrderBy(g => g.Key).Select(g => g.Key));
            this.AlbumList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.Album).OrderBy(g => g.Key).Select(g => g.Key));

            //        this.MediaFiles = CollectionViewSource.GetDefaultView(new MedialibCollection(files));
            //        ((ICollectionView)this.MediaFiles).GroupDescriptions.Add(new PropertyGroupDescription("Album"));
        }

        public void FilterByGenreSelection(string genre)
        {
            var collView = this.MediaFiles as ICollectionView;
            if (collView == null)
            {
                return;
            }
            var files = collView.SourceCollection.OfType<IMediaFile>().ToList();
            if (!string.IsNullOrWhiteSpace(genre))
            {
                files = files.Where(m => Equals(m.FirstGenre, genre)).ToList();
            }
            this.ArtistList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.FirstPerformer).OrderBy(g => g.Key).Select(g => g.Key));
            this.AlbumList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.Album).OrderBy(g => g.Key).Select(g => g.Key));
            collView.Refresh();
        }

        public void FilterByArtistSelection(string genre, string artist)
        {
            var collView = this.MediaFiles as ICollectionView;
            if (collView == null)
            {
                return;
            }
            var files = collView.SourceCollection.OfType<IMediaFile>().ToList();
            if (!string.IsNullOrWhiteSpace(genre))
            {
                files = files.Where(m => Equals(m.FirstGenre, genre)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(artist))
            {
                files = files.Where(m => Equals(m.FirstPerformer, artist)).ToList();
            }
            this.AlbumList = new QuickFillObservableCollection<string>(files.GroupBy(m => m.Album).OrderBy(g => g.Key).Select(g => g.Key));
            collView.Refresh();
        }

        public void FilterByAlbumSelection()
        {
            var collView = this.MediaFiles as ICollectionView;
            if (collView == null)
            {
                return;
            }
            collView.Refresh();
        }
    }
}