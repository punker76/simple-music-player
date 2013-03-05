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
using SimpleMusicPlayer.Models;
using SimpleMusicPlayer.ViewModels;

namespace SimpleMusicPlayer.Common
{
  public class FileSearchWorker : ViewModelBase
  {
    private readonly string[] extensions = new[] {".mp3", ".wma", ".ogg", ".wav"};
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
          var rawFiles = filesOrDirsCollection.OfType<string>().Where(this.IsAudioFile).OrderBy(s => s).ToList();
          foreach (var rawFile in rawFiles.TakeWhile(rawDir => !token.IsCancellationRequested)) {
            var mf = this.GetMediaFile(rawFile);
            if (mf != null) {
              results.Enqueue(mf);
            }
          }

          // handle all directories from input collection
          var directories = new List<string>();
          foreach (var source in filesOrDirsCollection.OfType<string>().Except(rawFiles).Where(IsDirectory).TakeWhile(source => !token.IsCancellationRequested)) {
            directories.Add(source);
            try {
              directories.AddRange(Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories).TakeWhile(dir => !token.IsCancellationRequested));
            } catch (Exception e) {
              // System.UnauthorizedAccessException
              Console.WriteLine(e);
            }
          }
          foreach (var rawDir in directories.Distinct().OrderBy(s => s).TakeWhile(rawDir => !token.IsCancellationRequested)) {
            this.doFindFiles(token, rawDir, results);
          }

          return results;
        }, token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

      var mediaFiles = await this.mainTask;
      this.IsWorking = false;
      return mediaFiles;
    }

    private void doFindFiles(CancellationToken token, string dir, ConcurrentQueue<IMediaFile> results) {
      foreach (var extension in this.extensions.TakeWhile(rawDir => !token.IsCancellationRequested)) {
        try {
          foreach (var file in Directory.EnumerateFiles(dir, "*" + extension).TakeWhile(rawDir => !token.IsCancellationRequested)) {
            var mf = this.GetMediaFile(file);
            if (mf != null) {
              results.Enqueue(mf);
            }
          }
        } catch (Exception e) {
          // System.UnauthorizedAccessException
          Console.WriteLine(e);
        }
      }
    }

    private IMediaFile GetMediaFile(string fileName) {
      if (this.IsAudioFile(fileName)) {
        try {
          var mf = MediaFile.GetMediaFileViewModel(fileName);
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