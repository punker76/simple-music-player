using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using ReactiveUI;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.Core.Interfaces;
using SimpleMusicPlayer.Core.Player;
using Splat;
using TinyIoC;

namespace SimpleMusicPlayer.ViewModels
{
    public class PlayListsViewModel : ReactiveObject, IDropTarget, IKeyHandler, IEnableLogger
    {
        private readonly PlayerEngine playerEngine;
        private readonly PlayerSettings playerSettings;

        public PlayListsViewModel()
        {
            this.FileSearchWorker = new FileSearchWorker("PlayList", MediaFile.GetMediaFileViewModel);
            var container = TinyIoCContainer.Current;
            this.playerEngine = container.Resolve<PlayerEngine>();
            this.playerSettings = container.Resolve<PlayerSettings>();
            this.SelectedPlayListFiles = new ObservableCollection<IMediaFile>();

            this.PlayCommand = new DelegateCommand(this.Play, this.CanPlay);
            this.DeleteCommand = new DelegateCommand(this.DeleteSelectedFiles, this.CanDeleteSelectedFiles);

            this.StartUpCommand = ReactiveCommand.CreateFromTask(x => this.StartUpAsync());

            // handle command line args from another instance
            this.WhenAnyValue(x => x.CommandLineArgs)
                .Where(list => list != null && list.Skip(1).Any())
                .Select(list => list.Skip(1).ToList())
                .SelectMany(list => this.HandleCommandLineArgsAsync(list).ToObservable())
                .Subscribe();
        }

        private ReactiveList<string> commandLineArgs;

        public ReactiveList<string> CommandLineArgs
        {
            get => this.commandLineArgs;
            set => this.RaiseAndSetIfChanged(ref commandLineArgs, value);
        }

        public FileSearchWorker FileSearchWorker { get; private set; }

        private IEnumerable firstSimplePlaylistFiles;

        public IEnumerable FirstSimplePlaylistFiles
        {
            get => this.firstSimplePlaylistFiles;
            set => this.RaiseAndSetIfChanged(ref firstSimplePlaylistFiles, value);
        }

        private IMediaFile selectedPlayListFile;

        public IMediaFile SelectedPlayListFile
        {
            get => this.selectedPlayListFile;
            set => this.RaiseAndSetIfChanged(ref selectedPlayListFile, value);
        }

        private IEnumerable<IMediaFile> selectedPlayListFiles;

        public IEnumerable<IMediaFile> SelectedPlayListFiles
        {
            get => this.selectedPlayListFiles;
            set => this.RaiseAndSetIfChanged(ref selectedPlayListFiles, value);
        }

        private bool observeListBoxItemContainerGenerator;

        public bool ObserveListBoxItemContainerGenerator
        {
            get => this.observeListBoxItemContainerGenerator;
            set => this.RaiseAndSetIfChanged(ref observeListBoxItemContainerGenerator, value);
        }

        private int scrollIndex;

        public int ScrollIndex
        {
            get => this.scrollIndex;
            set => this.RaiseAndSetIfChanged(ref scrollIndex, value);
        }

        public ICommand DeleteCommand { get; }

        private bool CanDeleteSelectedFiles()
        {
            return this.FirstSimplePlaylistFiles != null
                   && this.SelectedPlayListFiles != null
                   && this.SelectedPlayListFiles.Any();
        }

        private void DeleteSelectedFiles()
        {
            var filesCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (filesCollView != null)
            {
                this.ObserveListBoxItemContainerGenerator = true;

                var currentPlayingFile = filesCollView.CurrentItem as IMediaFile;
                var filesColl = ((QuickFillObservableCollection<IMediaFile>)filesCollView.SourceCollection);
                var files2Delete = this.SelectedPlayListFiles.OrderBy(f => f.PlayListIndex).ToList();
                ((IList)this.SelectedPlayListFiles).Clear();
                var selectedIndex = files2Delete.First().PlayListIndex - 1;
                filesColl.RemoveItems(files2Delete);
                if (currentPlayingFile != null && files2Delete.Contains(currentPlayingFile))
                {
                    // mh, nothing yet, maybe the player should be stoped...
                }

                selectedIndex = Math.Min(selectedIndex, filesColl.Count - 1);
                if (selectedIndex >= 0)
                {
                    var newSelFile = filesColl[selectedIndex];
                    ((IList)this.SelectedPlayListFiles).Add(newSelFile);
                }
            }
        }

        public ICommand PlayCommand { get; }

        private bool CanPlay()
        {
            return this.FirstSimplePlaylistFiles != null
                   && this.FirstSimplePlaylistFiles.OfType<IMediaFile>().Any();
        }

        private void Play()
        {
            var file = this.SelectedPlayListFile;
            if (file != null && this.SetCurrentPlayListFile(file))
            {
                this.playerEngine.Play(file);
            }
        }

        public void ResetCurrentItemAndSelection()
        {
            var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (fileCollView != null)
            {
                fileCollView.MoveCurrentTo(null);
                this.SelectedPlayListFile = null;
            }
        }

        public bool IsLastPlayListFile()
        {
            var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (fileCollView != null)
            {
                return fileCollView.CurrentPosition == fileCollView.SourceCollection.OfType<IMediaFile>().Count() - 1;
            }

            return false;
        }

        public IMediaFile GetCurrentPlayListFile()
        {
            var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (fileCollView != null)
            {
                var currentFile = fileCollView.CurrentItem ?? this.SelectedPlayListFile;
                if (currentFile == null)
                {
                    if (this.playerSettings.PlayerEngine.ShuffleMode)
                    {
                        return this.GetRandomPlayListFile();
                    }
                    else if (fileCollView.MoveCurrentToFirst())
                    {
                        return fileCollView.CurrentItem as IMediaFile;
                    }
                }

                return currentFile as IMediaFile;
            }

            return null;
        }

        private bool SetCurrentPlayListFile(IMediaFile file)
        {
            if (file != null)
            {
                var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
                if (fileCollView != null)
                {
                    return fileCollView.MoveCurrentTo(file);
                }
            }

            return false;
        }

        public IMediaFile GetPrevPlayListFile()
        {
            var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (fileCollView != null)
            {
                if (this.playerSettings.PlayerEngine.ShuffleMode)
                {
                    return this.GetRandomPlayListFile();
                }
                else
                {
                    if (fileCollView.MoveCurrentToPrevious() || fileCollView.MoveCurrentToLast())
                    {
                        return fileCollView.CurrentItem as IMediaFile;
                    }
                }
            }

            return null;
        }

        public IMediaFile GetNextPlayListFile()
        {
            var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (fileCollView != null)
            {
                if (this.playerSettings.PlayerEngine.ShuffleMode)
                {
                    return this.GetRandomPlayListFile();
                }
                else
                {
                    if (fileCollView.MoveCurrentToNext() || fileCollView.MoveCurrentToFirst())
                    {
                        return fileCollView.CurrentItem as IMediaFile;
                    }
                }
            }

            return null;
        }

        public IMediaFile GetRandomPlayListFile()
        {
            var fileCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (fileCollView != null)
            {
                var count = fileCollView.SourceCollection.OfType<IMediaFile>().Count();
                var r = new Random(Environment.TickCount);
                var pos = r.Next(0, count);

                if (count == 1)
                {
                    return fileCollView.CurrentItem as IMediaFile;
                }

                if (pos == fileCollView.CurrentPosition)
                {
                    while (pos == fileCollView.CurrentPosition)
                    {
                        pos = r.Next(0, count);
                    }
                }

                if (fileCollView.MoveCurrentToPosition(pos))
                {
                    return fileCollView.CurrentItem as IMediaFile;
                }
            }

            return null;

            /* new Random() already uses the current time. It is equivalent to new Random(Environment.TickCount).
             * But this is an implementation detail and might change in future versions of .net
             * I'd recommend using new Random() and only provide a fixed seed if you want to get a reproducible sequence of pseudo random values.
             * Since you need a known seed just use Environment.TickCount just like MS does. And then transmit it to the other program instances as seed.
             * If you create multiple instances of Random in a short interval (could be 16ms) they can be seeded to the same value,
             * and thus create the same pseudo-random sequence. But that's most likely not a problem here.
             * This common pitfall is caused by windows updating the current time(DateTime.Now/.UtcNow)
             * and the TickCount(Environment.TickCount) only every few milliseconds.
             * The exact interval depends on the version of windows and on what other programs are running.
             * Typical intervals where they don't change are 16ms or 1ms.
             */
        }

        public bool HandleKeyDown(KeyEventArgs args)
        {
            var handled = false;
            switch (args.Key)
            {
                case Key.Enter:
                    handled = this.PlayCommand.CanExecute(null);
                    if (handled)
                    {
                        this.PlayCommand.Execute(null);
                    }

                    break;
                case Key.Delete:
                    handled = this.DeleteCommand.CanExecute(null);
                    if (handled)
                    {
                        this.DeleteCommand.Execute(null);
                    }

                    break;
            }

            return handled;
        }

        public void DragEnter(IDropInfo dropInfo)
        {
            // nothing here
        }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;

            var dataObject = dropInfo.Data as IDataObject;
            // look for drag&drop new files
            if (dataObject != null && dataObject.GetDataPresent(DataFormats.FileDrop))
            {
                dropInfo.Effects = !this.FileSearchWorker.IsWorking ? DragDropEffects.Copy : DragDropEffects.None;
            }
            else
            {
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void DragLeave(IDropInfo dropInfo)
        {
            // nothing here
        }

        public void Drop(IDropInfo dropInfo)
        {
            var dataObject = dropInfo.Data as DataObject;
            // look for drag&drop new files
            if (dataObject != null && dataObject.ContainsFileDropList())
            {
                var task = this.HandleDropActionAsync(dropInfo, dataObject.GetFileDropList());
                task.ContinueWith(t =>
                {
                    //  handle t.Exception
                    this.Log().ErrorException("There was something wrong while handling dropped data.", t.Exception);
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            else
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
                var mediaFile = dropInfo.Data as IMediaFile;
                if (mediaFile != null && mediaFile.State != PlayerState.Stop)
                {
                    this.SetCurrentPlayListFile(mediaFile);
                }
            }
        }

        private async Task HandleDropActionAsync(IDropInfo dropInfo, IList fileOrDirDropList)
        {
            if (!this.FileSearchWorker.IsWorking)
            {
                var files = await this.FileSearchWorker.StartSearchAsync(fileOrDirDropList)
                    .ContinueWith(task => task.Result.OrderBy(f => f.FirstPerformer)
                        .ThenBy(f => f.Album)
                        .ThenBy(f => f.Disc)
                        .ThenBy(f => f.Track));

                var currentFilesCollView = this.FirstSimplePlaylistFiles as ICollectionView;

                if (currentFilesCollView == null)
                {
                    var filesColl = new PlayListCollection(files);
                    var filesCollView = CollectionViewSource.GetDefaultView(filesColl);
                    this.FirstSimplePlaylistFiles = filesCollView;
                    ((ICollectionView)this.FirstSimplePlaylistFiles).MoveCurrentTo(null);
                }
                else
                {
                    var insertIndex = dropInfo.InsertIndex;
                    var destinationList = (QuickFillObservableCollection<IMediaFile>)(dropInfo.TargetCollection.TryGetList());
                    destinationList.AddItems(files, insertIndex);
                }
            }
        }

        public async Task HandleCommandLineArgsAsync(IList args)
        {
            // TODO take another search worker for multiple added files via command line (possible lost the command line files while searching...)
            if (!this.FileSearchWorker.IsWorking)
            {
                this.Log().Info("handle {0} command line args", args.Count);

                var files = await this.FileSearchWorker.StartSearchAsync(args);

                var currentFilesCollView = this.FirstSimplePlaylistFiles as ICollectionView;

                var newScrollIndex = 0;

                if (currentFilesCollView == null)
                {
                    var filesColl = new PlayListCollection(files);
                    var filesCollView = CollectionViewSource.GetDefaultView(filesColl);
                    this.FirstSimplePlaylistFiles = filesCollView;
                    ((ICollectionView)this.FirstSimplePlaylistFiles).MoveCurrentTo(null);

                    this.Play();
                }
                else
                {
                    var filesColl = (QuickFillObservableCollection<IMediaFile>)currentFilesCollView.SourceCollection;
                    newScrollIndex = filesColl.Count;
                    var insertIndex = filesColl.Count;
                    filesColl.AddItems(files, insertIndex);

                    var file = files.FirstOrDefault();
                    if (file != null)
                    {
                        currentFilesCollView.MoveCurrentTo(file);
                        this.playerEngine.Play(file);
                    }
                }

                this.Log().Debug("set scroll index to {0}", newScrollIndex);
                this.ScrollIndex = newScrollIndex;

                this.CommandLineArgs = null;
            }
        }

        public ReactiveCommand<Unit, Unit> StartUpCommand { get; private set; }

        public async Task StartUpAsync()
        {
            var playList = await PlayList.LoadAsync();
            if (playList != null)
            {
                this.Log().Info("show loaded play list with {0} files", playList.Files.Count);
                var filesColl = new PlayListCollection(playList.Files);
                var filesCollView = CollectionViewSource.GetDefaultView(filesColl);
                this.FirstSimplePlaylistFiles = filesCollView;
                ((ICollectionView)this.FirstSimplePlaylistFiles).MoveCurrentTo(null);
            }

            var args = await Task.Run(() => Environment.GetCommandLineArgs().Skip(1).ToList());
            if (args.Any())
            {
                await this.HandleCommandLineArgsAsync(args);
            }
        }

        public bool SavePlayList()
        {
            var currentFilesCollView = this.FirstSimplePlaylistFiles as ICollectionView;
            if (currentFilesCollView != null)
            {
                var pl = new PlayList { Files = currentFilesCollView.SourceCollection.OfType<MediaFile>().ToList() };
                return PlayList.Save(pl);
            }

            return false;
        }
    }
}