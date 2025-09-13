/*
-- .NET File Downloader
This is a HTTP(S) file downloader with support of caching and resuming of partially downloaded files. 
The main goal of .NET File Downloader is to facilitate downloading of big files on bad internet connections. It supports resuming of partially downloaded files. So if the download is interrupted and restarted, only the remaining part of file would be downloaded again.
---- Why not simply use the file download functionality from the .NET Framework?
Unfortunately System.Net.WebClient.DownloadFile() has no support for resuming downloads. It might work fine for small files or over stable internet connections. Once you want to download bigger files via poor internet connections it is not sufficient anymore and downloads will fail.
--- License
FileDownloader is open source software, licensed under the terms of MIT license. See [LICENSE](LICENSE) for details.
-- Examples
--- Create simple fileDownloader
```C#
    IFileDownloader fileDownloader = new FileDownloader.FileDownloader();
```
--- Create fileDownloader with download cache
```C#
    IFileDownloader fileDownloader = new FileDownloader.FileDownloader(new DownloadCacheImplementation());
```
--- Start download
```C#
    IFileDownloader fileDownloader = new FileDownloader.FileDownloader();
    fileDownloader.DownloadFileCompleted += DownloadFileCompleted;
    fileDownloader.DownloadFileAsync(picture, dowloadDestinationPath);
    
    void DownloadFileCompleted(object sender, DownloadFileCompletedArgs eventArgs)
    {
            if (eventArgs.State == CompletedState.Succeeded)
            {
                //download completed
            }
            else if (eventArgs.State == CompletedState.Failed)
            {
                //download failed
            }
    }    
```
--- Report download progress
```C#
    FileDownlaoder fileDownloader = new FileDownloader.FileDownloader();
    fileDownloader.DownloadProgressChanged += OnDownloadProgressChanged;
    fileDownloader.DownloadFileAsync(picture, dowloadDestinationPath);

    void OnDownloadProgressChanged(object sender, DownloadFileProgressChangedArgs args)
    {
		Console.WriteLine("Downloaded {0} of {1} bytes", args.BytesReceived, args.TotalBytesToReceive)
    }

```

--- Cancel download
```C#
    fileDownloader.CancelDownloadAsync()
```
--- Resume download

 In order to resume download, IDownloadCache should be provided to the FileDownloader constructor. DownloadCache is used to store partially downloaded files. To resume a download, simply call one of the download file methods with the same URL.

-- How to build FileDownlader
At least Visual Studio 2013 and .NET Framework 3.5 are required to build. There are no other dependencies. 
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.ParallelToolbox;

namespace Extenity.WWWToolbox.FileDownloader
{

	public enum FileDownloaderResult
	{
		Succeeded,
		Cancelled,
		Failed
	}

	/// <summary>
	/// Class used for downloading files. The .NET WebClient is used for downloading.
	/// </summary>
	public class FileDownloader : IDisposable
	{
		private readonly ManualResetEvent ReadyToDownload = new ManualResetEvent(true);
		private readonly System.Timers.Timer AttemptTimer = new System.Timers.Timer();
		private readonly object CancelSync = new object();

		private bool IsCancelled;
		private bool Disposed;

		private int AttemptNumber;

		private string LocalFileName;
		private string DestinationFileName;
		private string DestinationFolder;

		private Uri FileSource;
		private ThreadedStreamCopier StreamCopier;
		private DownloadWebClient DownloadWebClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileDownloader"/> class.
		/// </summary>
		public FileDownloader()
		{
			MaxAttempts = 60;
			DelayBetweenAttempts = TimeSpan.FromSeconds(3);
			SafeWaitTimeout = TimeSpan.FromSeconds(15);
			SourceStreamReadTimeout = TimeSpan.FromSeconds(5);

			Disposed = false;

			AttemptTimer.Elapsed += OnDownloadAttemptTimer;
		}

		/// <summary>
		/// Fired when download is finished, even if it's failed.
		/// </summary>
		public event EventHandler<DownloadFileCompletedArgs> DownloadFileCompleted;

		/// <summary>
		/// Fired when download progress is changed.
		/// </summary>
		public event EventHandler<DownloadFileProgressChangedArgs> DownloadProgressChanged;

		/// <summary>
		/// Gets or sets the delay between download attempts. Default is 3 seconds. 
		/// </summary>
		public TimeSpan DelayBetweenAttempts { get; set; }

		/// <summary>
		/// Gets or sets the maximum waiting timeout for pending request to be finished. Default is 15 seconds.
		/// </summary>
		public TimeSpan SafeWaitTimeout { get; set; }

		/// <summary>
		/// Gets or sets the timeout for source stream. Default is 5 seconds.
		/// </summary>
		public TimeSpan SourceStreamReadTimeout { get; set; }

		/// <summary>
		/// Gets or sets the maximum number of download attempts. Default is 60.
		/// </summary>
		public int MaxAttempts { get; set; }

		/// <summary>
		/// Gets the total bytes received so far
		/// </summary>
		public long BytesReceived { get; private set; }

		/// <summary>
		/// Gets the total bytes to receive
		/// </summary>
		public long TotalBytesToReceive { get; private set; }

		/// <summary>
		/// Gets or sets the time when download was started
		/// </summary>
		public DateTime DownloadStartTime { get; private set; }

		/// <summary>
		/// Cancel current download
		/// </summary>
		public void CancelDownloadAsync()
		{
			lock (CancelSync)
			{
				if (IsCancelled)
				{
					return;
				}
				IsCancelled = true;
			}

			if (StreamCopier != null)
			{
				StreamCopier.Cancel();
			}

			TriggerDownloadWebClientCancelAsync();

			ReadyToDownload.Set();
		}

		private void DeleteDownloadedFile()
		{
			try
			{
				FileTools.Delete(LocalFileName);
			}
			catch
			{
				// ignored
			}
		}

		public void DownloadFileAsync(Uri source, string destinationPath)
		{
			if (!ReadyToDownload.WaitOne(SafeWaitTimeout))
			{
				throw new Exception("Unable to start download because another request is still in progress.");
			}
			ReadyToDownload.Reset();

			FileSource = source;
			BytesReceived = 0;
			DestinationFileName = destinationPath;
			DestinationFolder = Path.GetDirectoryName(destinationPath);
			IsCancelled = false;
			LocalFileName = string.Empty;

			DownloadStartTime = DateTime.Now;

			AttemptNumber = 0;

			StartDownload();
		}

		private void OnDownloadAttemptTimer(object sender, EventArgs eventArgs)
		{
			StartDownload();
		}

		private void StartDownload()
		{
			if (IsCancelledLocked())
			{
				return;
			}

			LocalFileName = Path.Combine(DestinationFolder, DestinationFileName);

			TotalBytesToReceive = -1;
			try
			{
				var webRequest = WebRequest.Create(FileSource);
				webRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
				webRequest.Method = WebRequestMethods.Http.Head;

				using (var webResponse = webRequest.GetResponse())
				{
					var headers = webResponse.Headers;
					if (headers != null)
					{
						TotalBytesToReceive = headers.GetContentLength();
					}
				}
			}
			catch (Exception exception)
			{
				InvokeDownloadCompleted(FileDownloaderResult.Failed, null, new Exception("Failed to start download.", exception));
				return;
			}

			if (TotalBytesToReceive < 0)
			{
				TotalBytesToReceive = 0;
				InvokeDownloadCompleted(FileDownloaderResult.Failed, null, new Exception("Server returned no Content-Length header which prevents partial downloads."));
				return;
			}
			else
			{
				ResumeDownload();
			}
		}

		private void ResumeDownload()
		{
			if (!FileTools.TryGetFileSize(LocalFileName, out var localFileSize))
			{
				localFileSize = 0;
				// Handle this case in future. Now in case of error we simply proceed with downloadedFileSize=0
			}

			if (localFileSize > TotalBytesToReceive)
			{
				DeleteDownloadedFile();
			}

			if (localFileSize != TotalBytesToReceive)
			{
				Download(FileSource, LocalFileName);
			}
			else
			{
				OnDownloadProgressChanged(this, new DownloadFileProgressChangedArgs(100, TotalBytesToReceive, TotalBytesToReceive));
				InvokeDownloadCompleted(FileDownloaderResult.Succeeded, LocalFileName, null, true);
				ReadyToDownload.Set();
				return;
			}
		}

		private DownloadWebClient CreateWebClient()
		{
			var webClient = new DownloadWebClient();
			webClient.DownloadFileCompleted += OnDownloadCompleted;
			webClient.DownloadProgressChanged += OnDownloadProgressChanged;
			webClient.OpenReadCompleted += OnOpenReadCompleted;
			return webClient;
		}

		private void TryCleanupExistingDownloadWebClient()
		{
			if (DownloadWebClient == null)
				return;

			try
			{
				lock (this)
				{
					if (DownloadWebClient != null)
					{
						DownloadWebClient.DownloadFileCompleted -= OnDownloadCompleted;
						DownloadWebClient.DownloadProgressChanged -= OnDownloadProgressChanged;
						DownloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;
						DownloadWebClient.CancelAsync();
						DownloadWebClient.Dispose();
						DownloadWebClient = null;
					}
				}
			}
			catch
			{
				// ignored
			}
		}

		private bool AttemptDownload()
		{
			if (++AttemptNumber <= MaxAttempts)
			{
				AttemptTimer.Interval = DelayBetweenAttempts.TotalMilliseconds;
				AttemptTimer.AutoReset = false;
				AttemptTimer.Start();
				return true;
			}
			return false;
		}

		private void Download(Uri source, string fileDestination)
		{
			try
			{
				if (!FileTools.TryGetFileSize(fileDestination, out var seekPosition))
				{
					seekPosition = 0;
				}

				TryCleanupExistingDownloadWebClient();
				DownloadWebClient = CreateWebClient();
				DownloadWebClient.OpenReadAsync(source, seekPosition);
			}
			catch (Exception e)
			{
				if (!AttemptDownload())
				{
					InvokeDownloadCompleted(FileDownloaderResult.Failed, LocalFileName, e);
				}
			}
		}

		private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
		{
			var e = new DownloadFileProgressChangedArgs(args.ProgressPercentage, args.BytesReceived, args.TotalBytesToReceive);

			OnDownloadProgressChanged(sender, e);
		}

		private void OnDownloadProgressChanged(object sender, DownloadFileProgressChangedArgs args)
		{
			if (BytesReceived < args.BytesReceived)
			{
				////bytes growing? we have connection!
				AttemptNumber = 1;
			}

			BytesReceived = args.BytesReceived;
			TotalBytesToReceive = args.TotalBytesToReceive;

			if (DownloadProgressChanged != null)
				DownloadProgressChanged(sender, args);
		}

		private void InvokeDownloadCompleted(FileDownloaderResult result, string fileName, Exception error = null, bool fromCache = false)
		{
			var downloadTime = fromCache ? TimeSpan.Zero : DateTime.Now.Subtract(DownloadStartTime);
			if (StreamCopier != null)
			{
				BytesReceived = StreamCopier.Position;
			}

			if (DownloadFileCompleted != null)
				DownloadFileCompleted(this, new DownloadFileCompletedArgs(result, fileName, FileSource, downloadTime, TotalBytesToReceive, BytesReceived, error));
		}

		private void OnOpenReadCompleted(object sender, OpenReadCompletedEventArgs args)
		{
			var webClient = sender as DownloadWebClient;
			if (webClient == null)
				return;

			lock (CancelSync)
			{
				if (IsCancelled || args.Cancelled)
					return;

				if (args.Error != null)
				{
					InvokeDownloadCompleted(FileDownloaderResult.Failed, null, new Exception("Download failed.", args.Error));
					return;
				}

				if (!webClient.HasResponse)
				{
					InvokeDownloadCompleted(FileDownloaderResult.Failed, null, new Exception("Server did not returned any response which prevents partial downloads."));
					return;
				}

				var appendExistingChunk = webClient.IsPartialResponse;
				var destinationStream = CreateDestinationStream(appendExistingChunk);
				if (destinationStream != null)
				{
					args.Result.TrySetStreamReadTimeout((int)SourceStreamReadTimeout.TotalMilliseconds);

					StreamCopier = new ThreadedStreamCopier();
					StreamCopier.OnFinished += OnStreamCopierFinished;
					StreamCopier.OnProgressChanged += OnStreamCopierProgressChanged;
					StreamCopier.CopyAsync(args.Result, destinationStream, TotalBytesToReceive);
				}
			}
		}

		private Stream CreateDestinationStream(bool append)
		{
			FileStream destinationStream = null;
			try
			{
				var destinationDirectory = Path.GetDirectoryName(LocalFileName);
				if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
				{
					Directory.CreateDirectory(destinationDirectory);
				}

				destinationStream = new FileStream(LocalFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
				if (append)
				{
					destinationStream.Seek(0, SeekOrigin.End);
				}
				else
				{
					destinationStream.SetLength(0);
				}
			}
			catch (Exception ex)
			{
				if (destinationStream != null)
				{
					destinationStream.Dispose();
					destinationStream = null;
				}
				OnDownloadCompleted(DownloadWebClient, new AsyncCompletedEventArgs(ex, false, null));
			}
			return destinationStream;
		}

		private void OnStreamCopierProgressChanged(long bytesReceived)
		{
			if (IsCancelled)
			{
				return;
			}

			if (TotalBytesToReceive == 0)
			{
				return;
			}
			var progress = bytesReceived / TotalBytesToReceive;
			var progressPercentage = (int)(progress * 100);

			OnDownloadProgressChanged(this, new DownloadFileProgressChangedArgs(progressPercentage, bytesReceived, TotalBytesToReceive));
		}

		private void OnStreamCopierFinished(StreamCopierResult result, Exception error)
		{
			StreamCopier.OnProgressChanged -= OnStreamCopierProgressChanged;
			StreamCopier.OnFinished -= OnStreamCopierFinished;

			try
			{
				OnDownloadCompleted(DownloadWebClient, new AsyncCompletedEventArgs(error, result == StreamCopierResult.Cancelled, null));
			}
			finally
			{
				StreamCopier.Dispose();
			}
		}

		/// <summary>
		/// OnDownloadCompleted event handler
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="args">AsyncCompletedEventArgs instance</param>
		protected void OnDownloadCompleted(object sender, AsyncCompletedEventArgs args)
		{
			var webClient = sender as DownloadWebClient;
			if (webClient == null)
			{
				InvokeDownloadCompleted(FileDownloaderResult.Failed, LocalFileName);
				return;
			}

			if (args.Cancelled)
			{
				DeleteDownloadedFile();

				InvokeDownloadCompleted(FileDownloaderResult.Cancelled, LocalFileName);
				ReadyToDownload.Set();
				return;
			}
			else if (args.Error != null)
			{
				if (!AttemptDownload())
				{
					InvokeDownloadCompleted(FileDownloaderResult.Failed, null, args.Error);
					ReadyToDownload.Set();
					return;
				}
			}
			else
			{
				////we may have the destination file not immediately closed after downloading
				WaitFileClosed(LocalFileName, TimeSpan.FromSeconds(3));

				InvokeDownloadCompleted(FileDownloaderResult.Succeeded, LocalFileName, null);
				ReadyToDownload.Set();
				return;
			}
		}

		private void TriggerDownloadWebClientCancelAsync()
		{
			if (DownloadWebClient != null)
			{
				DownloadWebClient.CancelAsync();
				DownloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;
			}
		}

		private void WaitFileClosed(string fileName, TimeSpan waitTimeout)
		{
			var waitCounter = TimeSpan.Zero;
			while (waitCounter < waitTimeout)
			{
				try
				{
					var fileHandle = File.Open(fileName, FileMode.Open, FileAccess.Read);
					fileHandle.Close();
					fileHandle.Dispose();
					Thread.Sleep(500);
					return;
				}
				catch (Exception)
				{
					waitCounter = waitCounter.Add(TimeSpan.FromMilliseconds(500));
					Thread.Sleep(500);
				}
			}
		}

		private bool IsCancelledLocked()
		{
			lock (CancelSync)
			{
				if (IsCancelled)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Do the actual dispose
		/// </summary>
		/// <param name="disposing">True if called from Dispose</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					if (ReadyToDownload.WaitOne(TimeSpan.FromMinutes(10)))
					{
						if (StreamCopier != null)
						{
							StreamCopier.Dispose();
							StreamCopier = null;
						}
						if (DownloadWebClient != null)
						{
							DownloadWebClient.Dispose();
							DownloadWebClient = null;
						}
						ReadyToDownload.Close();
						AttemptTimer.Dispose();
					}
				}
				Disposed = true;
			}
		}
	}

}
