using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using SchwabenCode.QuickIO;
using SimpleMusicPlayer.Core.Interfaces;
using Splat;

namespace SimpleMusicPlayer.Core
{
    public class FileSearchWorker : ReactiveObject, IEnableLogger
    {
        private readonly Regex searchPattern = new Regex(@"\.mp3|\.wma|\.ogg|\.wav", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly string name;
        // action for media file creation
        private readonly Func<string, IMediaFile> createMediaFileFunc;

        public FileSearchWorker(string aName, Func<string, IMediaFile> createMediaFileFunc)
        {
            this.name = aName;
            this.createMediaFileFunc = createMediaFileFunc;

            this.WhenAnyValue(x => x.IsWorking)
                .Subscribe(b => this.Log().Debug("({0}) IsWorking={1}", this.name, b));

            this.StopSearchCmd = ReactiveCommand.Create(
                () => this.CancelToken.Cancel(),
                this.WhenAnyValue(x => x.IsWorking, x => x.CancelToken,
                    (isworking, canceltoken) => isworking && canceltoken != null));
        }

        private Task<IEnumerable<IMediaFile>> mainTask;
        public Task<IEnumerable<IMediaFile>> MainTask
        {
            get => this.mainTask;
            private set => this.RaiseAndSetIfChanged(ref mainTask, value);
        }

        private bool isWorking;
        public bool IsWorking
        {
            get => this.isWorking;
            private set => this.RaiseAndSetIfChanged(ref isWorking, value);
        }

        private CancellationTokenSource cancelToken;
        public CancellationTokenSource CancelToken
        {
            get => this.cancelToken;
            private set => this.RaiseAndSetIfChanged(ref cancelToken, value);
        }

        public ReactiveCommand<Unit, Unit> StopSearchCmd { get; private set; }

        public async Task<IEnumerable<IMediaFile>> StartSearchAsync(IList filesOrDirsCollection)
        {
            this.IsWorking = true;
            // create the cancellation token source
            this.CancelToken = new CancellationTokenSource();
            // create the cancellation token
            var token = this.CancelToken.Token;

            this.MainTask = Task<IEnumerable<IMediaFile>>.Factory
              .StartNew(() => {
                  var results = new ConcurrentQueue<IMediaFile>();

                  // get audio files from input collection
                  var rawFiles = filesOrDirsCollection.OfType<string>().Where(this.IsAudioFile).OrderBy(s => s).ToList();
                  foreach (var rawFile in rawFiles.TakeWhile(rawDir => !token.IsCancellationRequested))
                  {
                      var mf = this.GetMediaFile(rawFile);
                      if (mf != null)
                      {
                          results.Enqueue(mf);
                      }
                  }

                  // handle all directories from input collection
                  var directories = new List<string>();
                  foreach (var source in filesOrDirsCollection.OfType<string>().Except(rawFiles).Where(IsDirectory).TakeWhile(source => !token.IsCancellationRequested))
                  {
                      directories.AddRange(GetSubFolders(token, source));
                  }

                  var orderedDirs = directories.Distinct().OrderBy(s => s);
                  this.Log().Debug("search for files in {0} directories (sub directories included)", orderedDirs.Count());

                  foreach (var rawDir in orderedDirs.TakeWhile(rawDir => !token.IsCancellationRequested))
                  {
                      this.DoFindFiles(token, rawDir, results);
                  }

                  return results;
              }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            var mediaFiles = await this.MainTask;

            this.IsWorking = false;

            return mediaFiles;
        }

        private IEnumerable<string> GetSubFolders(CancellationToken token, string source)
        {
            var directories = new List<string>();
            try
            {
                directories.Add(source);
                var allSubFolders = QuickIODirectory.EnumerateDirectories(source);
                foreach (var subFolder in allSubFolders.TakeWhile(rawDir => !token.IsCancellationRequested))
                {
                    directories.AddRange(GetSubFolders(token, subFolder.FullName));
                }
            }
            catch (Exception e)
            {
                this.Log().ErrorException("EnumerateDirectories", e);
            }
            return directories;
        }

        private void DoFindFiles(CancellationToken token, string dir, ConcurrentQueue<IMediaFile> results)
        {
            try
            {
                var allFiles = QuickIODirectory.EnumerateFiles(dir, "*", SearchOption.TopDirectoryOnly)
                                               .Where(f => this.IsAudioFile(f.Name));
                foreach (var file in allFiles.TakeWhile(rawDir => !token.IsCancellationRequested))
                {
                    var mf = this.GetMediaFile(file.FullName);
                    if (mf != null)
                    {
                        results.Enqueue(mf);
                    }
                }
            }
            catch (Exception exception)
            {
                this.Log().ErrorException("EnumerateFiles", exception);
            }
        }

        private IMediaFile GetMediaFile(string fileName)
        {
            if (this.createMediaFileFunc != null)
            {
                try
                {
                    var mediaFile = this.createMediaFileFunc(fileName);
                    return mediaFile;
                }
                catch (Exception e)
                {
                    this.Log().ErrorException("Uups, there is something wrong with " + fileName, e);
                }
            }
            return null;
        }

        private bool IsAudioFile(string fileName)
        {
            var ext = Path.GetExtension(fileName);
            return !string.IsNullOrEmpty(ext) && searchPattern.IsMatch(ext);
        }

        private static bool IsDirectory(string dirName)
        {
            return QuickIODirectory.Exists(dirName);
        }
    }
}