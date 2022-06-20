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
		private readonly ManualResetEvent readyToDownload = new ManualResetEvent(true);
		private readonly System.Timers.Timer attemptTimer = new System.Timers.Timer();
		private readonly object cancelSync = new object();

		private bool isCancelled;
		private bool disposed;

		private int attemptNumber;

		private string localFileName;
		private string destinationFileName;
		private string destinationFolder;

		private Uri fileSource;
		private ThreadedStreamCopier streamCopier;
		private DownloadWebClient downloadWebClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileDownloader"/> class.
		/// </summary>
		public FileDownloader()
		{
			MaxAttempts = 60;
			DelayBetweenAttempts = TimeSpan.FromSeconds(3);
			SafeWaitTimeout = TimeSpan.FromSeconds(15);
			SourceStreamReadTimeout = TimeSpan.FromSeconds(5);

			disposed = false;

			attemptTimer.Elapsed += OnDownloadAttemptTimer;
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
			lock (cancelSync)
			{
				if (isCancelled)
				{
					return;
				}
				isCancelled = true;
			}

			if (streamCopier != null)
			{
				streamCopier.Cancel();
			}

			TriggerDownloadWebClientCancelAsync();

			readyToDownload.Set();
		}

		private void DeleteDownloadedFile()
		{
			try
			{
				FileTools.Delete(localFileName);
			}
			catch
			{
				// ignored
			}
		}

		public void DownloadFileAsync(Uri source, string destinationPath)
		{
			if (!readyToDownload.WaitOne(SafeWaitTimeout))
			{
				throw new Exception("Unable to start download because another request is still in progress.");
			}
			readyToDownload.Reset();

			fileSource = source;
			BytesReceived = 0;
			destinationFileName = destinationPath;
			destinationFolder = Path.GetDirectoryName(destinationPath);
			isCancelled = false;
			localFileName = string.Empty;

			DownloadStartTime = DateTime.Now;

			attemptNumber = 0;

			StartDownload();
		}

		private void OnDownloadAttemptTimer(object sender, EventArgs eventArgs)
		{
			StartDownload();
		}

		private void StartDownload()
		{
			if (IsCancelled())
			{
				return;
			}

			localFileName = Path.Combine(destinationFolder, destinationFileName);

			TotalBytesToReceive = -1;
			try
			{
				var webRequest = WebRequest.Create(fileSource);
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
			if (!FileTools.TryGetFileSize(localFileName, out var localFileSize))
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
				Download(fileSource, localFileName);
			}
			else
			{
				OnDownloadProgressChanged(this, new DownloadFileProgressChangedArgs(100, TotalBytesToReceive, TotalBytesToReceive));
				InvokeDownloadCompleted(FileDownloaderResult.Succeeded, localFileName, null, true);
				readyToDownload.Set();
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
			if (downloadWebClient == null)
				return;

			try
			{
				lock (this)
				{
					if (downloadWebClient != null)
					{
						downloadWebClient.DownloadFileCompleted -= OnDownloadCompleted;
						downloadWebClient.DownloadProgressChanged -= OnDownloadProgressChanged;
						downloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;
						downloadWebClient.CancelAsync();
						downloadWebClient.Dispose();
						downloadWebClient = null;
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
			if (++attemptNumber <= MaxAttempts)
			{
				attemptTimer.Interval = DelayBetweenAttempts.TotalMilliseconds;
				attemptTimer.AutoReset = false;
				attemptTimer.Start();
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
				downloadWebClient = CreateWebClient();
				downloadWebClient.OpenReadAsync(source, seekPosition);
			}
			catch (Exception e)
			{
				if (!AttemptDownload())
				{
					InvokeDownloadCompleted(FileDownloaderResult.Failed, localFileName, e);
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
				attemptNumber = 1;
			}

			BytesReceived = args.BytesReceived;
			TotalBytesToReceive = args.TotalBytesToReceive;

			if (DownloadProgressChanged != null)
				DownloadProgressChanged(sender, args);
		}

		private void InvokeDownloadCompleted(FileDownloaderResult result, string fileName, Exception error = null, bool fromCache = false)
		{
			var downloadTime = fromCache ? TimeSpan.Zero : DateTime.Now.Subtract(DownloadStartTime);
			if (streamCopier != null)
			{
				BytesReceived = streamCopier.Position;
			}

			if (DownloadFileCompleted != null)
				DownloadFileCompleted(this, new DownloadFileCompletedArgs(result, fileName, fileSource, downloadTime, TotalBytesToReceive, BytesReceived, error));
		}

		private void OnOpenReadCompleted(object sender, OpenReadCompletedEventArgs args)
		{
			var webClient = sender as DownloadWebClient;
			if (webClient == null)
				return;

			lock (cancelSync)
			{
				if (isCancelled || args.Cancelled)
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

					streamCopier = new ThreadedStreamCopier();
					streamCopier.OnFinished += OnStreamCopierFinished;
					streamCopier.OnProgressChanged += OnStreamCopierProgressChanged;
					streamCopier.CopyAsync(args.Result, destinationStream, TotalBytesToReceive);
				}
			}
		}

		private Stream CreateDestinationStream(bool append)
		{
			FileStream destinationStream = null;
			try
			{
				var destinationDirectory = Path.GetDirectoryName(localFileName);
				if (destinationDirectory != null && !Directory.Exists(destinationDirectory))
				{
					Directory.CreateDirectory(destinationDirectory);
				}

				destinationStream = new FileStream(localFileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
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
				OnDownloadCompleted(downloadWebClient, new AsyncCompletedEventArgs(ex, false, null));
			}
			return destinationStream;
		}

		private void OnStreamCopierProgressChanged(long bytesReceived)
		{
			if (isCancelled)
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
			streamCopier.OnProgressChanged -= OnStreamCopierProgressChanged;
			streamCopier.OnFinished -= OnStreamCopierFinished;

			try
			{
				OnDownloadCompleted(downloadWebClient, new AsyncCompletedEventArgs(error, result == StreamCopierResult.Cancelled, null));
			}
			finally
			{
				streamCopier.Dispose();
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
				InvokeDownloadCompleted(FileDownloaderResult.Failed, localFileName);
				return;
			}

			if (args.Cancelled)
			{
				DeleteDownloadedFile();

				InvokeDownloadCompleted(FileDownloaderResult.Cancelled, localFileName);
				readyToDownload.Set();
				return;
			}
			else if (args.Error != null)
			{
				if (!AttemptDownload())
				{
					InvokeDownloadCompleted(FileDownloaderResult.Failed, null, args.Error);
					readyToDownload.Set();
					return;
				}
			}
			else
			{
				////we may have the destination file not immediately closed after downloading
				WaitFileClosed(localFileName, TimeSpan.FromSeconds(3));

				InvokeDownloadCompleted(FileDownloaderResult.Succeeded, localFileName, null);
				readyToDownload.Set();
				return;
			}
		}

		private void TriggerDownloadWebClientCancelAsync()
		{
			if (downloadWebClient != null)
			{
				downloadWebClient.CancelAsync();
				downloadWebClient.OpenReadCompleted -= OnOpenReadCompleted;
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

		private bool IsCancelled()
		{
			lock (cancelSync)
			{
				if (isCancelled)
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
			if (!disposed)
			{
				if (disposing)
				{
					if (readyToDownload.WaitOne(TimeSpan.FromMinutes(10)))
					{
						if (streamCopier != null)
						{
							streamCopier.Dispose();
							streamCopier = null;
						}
						if (downloadWebClient != null)
						{
							downloadWebClient.Dispose();
							downloadWebClient = null;
						}
						readyToDownload.Close();
						attemptTimer.Dispose();
					}
				}
				disposed = true;
			}
		}
	}

}
