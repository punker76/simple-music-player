using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SimpleMusicPlayer.Base;
using SimpleMusicPlayer.Interfaces;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Common
{
  public class FileSearchWorker : ViewModelBaseNotifyPropertyChanged
  {
    private string[] extensions = new[] {".mp3", ".wma", ".mp4", ".wav"};
    private Task<IEnumerable<IMediaFile>> mainTask;
    private CancellationTokenSource cancelToken;
    private bool isWorking;
    private ICommand stopSearchCmd;

    public bool IsWorking {
      get { return this.isWorking; }
      set {
        if (Equals(value, this.isWorking)) {
          return;
        }
        this.isWorking = value;
        this.OnPropertyChanged(() => this.IsWorking);
      }
    }

    public bool IsBusy {
      get { return this.mainTask != null && !this.mainTask.IsCompleted; }
    }

    public bool CanStartSearch() {
      return this.mainTask == null || this.mainTask.IsCompleted;
    }

    public ICommand StopSearchCmd {
      get { return this.stopSearchCmd ?? (this.stopSearchCmd = new DelegateCommand(this.StopSearch, this.CanStopSearch)); }
    }

    public bool CanStopSearch() {
      return this.mainTask != null && this.cancelToken != null && !this.mainTask.IsCompleted;
    }

    public void StopSearch() {
      this.cancelToken.Cancel();
    }

    public async Task<IEnumerable<IMediaFile>> StartSearchAsync(StringCollection filesOrDirsCollection) {
      this.IsWorking = true;
      // create the cancellation token source
      this.cancelToken = new CancellationTokenSource();
      // create the cancellation token
      var token = this.cancelToken.Token;

      this.mainTask = Task<IEnumerable<IMediaFile>>.Factory
        .StartNew(() => {
          var results = new ConcurrentQueue<IMediaFile>();

          // get audio files from input collection
          var rawFiles = filesOrDirsCollection
            .OfType<string>()
            .Where(this.IsAudioFile)
            .OrderBy(s => s)
            .ToList();

          foreach (var rawFile in rawFiles) {
            if (token.IsCancellationRequested) {
              break;
            }
            var mf = this.GetMediaFile(rawFile);
            if (mf != null) {
              results.Enqueue(mf);
            }
          }

          // handle all directories from input collection
          var directories = new List<string>();
          foreach (var source in filesOrDirsCollection.OfType<string>().Except(rawFiles).Where(IsDirectory)) {
            if (token.IsCancellationRequested) {
              break;
            }
            directories.AddRange(Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories)
                                   .Concat(new[] {source})
                                   .TakeWhile(dir => !token.IsCancellationRequested));
          }

          foreach (var rawDir in directories.Distinct().OrderBy(s => s)) {
            if (token.IsCancellationRequested) {
              break;
            }
            this.doFindFilesForExtensions(token, rawDir, results);
          }

          return results;
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

      var mediaFiles = await this.mainTask;
      this.IsWorking = false;
      return mediaFiles;
    }

    private void doFindFilesForExtensions(CancellationToken token, string dir, ConcurrentQueue<IMediaFile> results) {
      foreach (var extension in this.extensions) {
        if (token.IsCancellationRequested) {
          return;
        }
        this.doFindFiles(token, dir, extension, results);
      }
      //      Parallel.ForEach(this.extensions,
      //                       (ext, estate) => {
      //                         if (token.IsCancellationRequested) {
      //                           estate.Stop();
      //                           return;
      //                         }
      //                         this.doFindFiles(token, dir, ext, results);
      //                       });
    }

    private void doFindFiles(CancellationToken token, string dir, string ext, ConcurrentQueue<IMediaFile> results) {
      foreach (var file in Directory.EnumerateFiles(dir, "*" + ext)) {
        if (token.IsCancellationRequested) {
          return;
        }
        var mf = this.GetMediaFile(file);
        if (mf != null) {
          results.Enqueue(mf);
        }
      }
      //      Parallel.ForEach(Directory.EnumerateFiles(dir, "*" + ext),
      //                       (file, fstate) => {
      //                         if (token.IsCancellationRequested) {
      //                           fstate.Stop();
      //                           return;
      //                         }
      //                         var mf = this.GetMediaFile(file);
      //                         if (mf != null) {
      //                           results.Enqueue(mf);
      //                         }
      //                       });
    }

    private IMediaFile GetMediaFile(string fileName) {
      if (this.IsAudioFile(fileName)) {
        try {
          var mf = MediaFileViewModel.GetMediaFileViewModel(fileName);
          return mf;
        } catch (Exception e) {
          var em = e.Message;
          return null;
        }
      } else {
        return null;
      }
    }

    private bool IsAudioFile(string fileName) {
      var ext = Path.GetExtension(fileName);
      return !string.IsNullOrEmpty(ext) && this.extensions.Contains(ext.ToLower());
    }

    private static bool IsDirectory(string dirName) {
      return Directory.Exists(dirName);
    }

    private static FileSearchWorker instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static FileSearchWorker() {
    }

    private FileSearchWorker() {
    }

    public static FileSearchWorker Instance {
      get { return instance ?? (instance = new FileSearchWorker()); }
    }
  }
}