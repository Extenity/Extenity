using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using Extenity.Parallel;
using ICSharpCode.SharpZipLib.Extensions;
using Timer = System.Timers.Timer;

namespace Extenity.WorldWideWeb
{

	public class HTTPDownloaderAsync
	{
		public readonly string BaseAddress;
		public readonly string TempFileNamePrefix;

		public HTTPDownloaderAsync(string baseAddress, string tempFileNamePrefix = "_inprogress.")
		{
			BaseAddress = baseAddress;
			TempFileNamePrefix = tempFileNamePrefix;
		}

		public DeferredExecutionController CreateDownloadJob(string remoteFileRelativePath, string localFileFullPath, bool extractCompressedFileAndDelete = false)
		{
			return DeferredExecution.Setup((sender, args) =>
			{
				var worker = (BackgroundWorker)sender;

				// Initialize paths
				localFileFullPath = localFileFullPath.FixDirectorySeparatorChars(); // To system defaults
				remoteFileRelativePath = remoteFileRelativePath.FixDirectorySeparatorChars('/'); // To HTTP
				var localFileName = Path.GetFileName(localFileFullPath);
				var localDirectoryPath = Path.GetDirectoryName(localFileFullPath).AddDirectorySeparatorToEnd();
				var localTempFileFullPath = Path.Combine(localDirectoryPath, TempFileNamePrefix + localFileName);
				var remoteFileFullPath = BaseAddress + remoteFileRelativePath;

				var waitEvent = new ManualResetEvent(false);
				Exception downloadError = null;

				// Get the object used to communicate with the server.
				var client = new WebClient();
				//client.UseDefaultCredentials
				//fileSizeRequest.Credentials = Credentials;
				client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
				client.Encoding = Encoding.UTF8;

				var timedOut = false;
				var timeoutTimer = new Timer();
				timeoutTimer.AutoReset = false;
				timeoutTimer.Enabled = false;
				timeoutTimer.Interval = 30000;
				timeoutTimer.Elapsed += (o, eventArgs) =>
				{
					timedOut = true;
					client.CancelAsync();
				};

				client.DownloadProgressChanged += (o, eventArgs) =>
				{
					timeoutTimer.Stop();
					timeoutTimer.Start();
					var downloadedSize = eventArgs.BytesReceived;
					var fileSize = eventArgs.TotalBytesToReceive;
					var humanReadableFileSize = fileSize.ToFileSizeString();

					var message = string.Format("{0} / {1}", downloadedSize.ToFileSizeString(), humanReadableFileSize);
					var ratio = (double)downloadedSize / fileSize;
					var progress = (int)(99f * ratio);
					if (progress < 0) progress = 0;
					if (progress > 99) progress = 99;
					worker.ReportProgress(progress, message);

					// Cancel if requested
					if (worker.CancellationPending)
					{
						client.CancelAsync();
						args.Cancel = true;
					}
				};

				client.DownloadFileCompleted += (o, eventArgs) =>
				{
					timeoutTimer.Stop();

					if (eventArgs.Error != null)
					{
						downloadError = eventArgs.Error;
						args.Cancel = true;
					}
					if (eventArgs.Cancelled)
					{
						args.Cancel = true;
					}
					waitEvent.Set();
				};

				// Create directory
				DirectoryTools.CreateFromFilePath(localTempFileFullPath);

				client.DownloadFileAsync(new Uri(remoteFileFullPath), localTempFileFullPath);
				timeoutTimer.Start();
				waitEvent.WaitOne();
				waitEvent = null;
				timeoutTimer.Stop();

				if (timedOut)
				{
					throw new Exception("Connection timed out.");
				}

				if (downloadError != null)
				{
					throw downloadError;
				}

				if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested, just before changing the file name

				// Rename downloaded file from temp filename to original filename
				worker.ReportProgress(99, "Changing temporary file name");
				if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested, just before changing the file name

				if (extractCompressedFileAndDelete)
				{
					// Extract downloaded file and delete
					SharpZipLibTools.ExtractFiles(File.OpenRead(localTempFileFullPath), localDirectoryPath);
					File.Delete(localTempFileFullPath);
				}
				else
				{
					// Delete if file already exists.
					if (File.Exists(localFileFullPath))
					{
						File.Delete(localFileFullPath);
					}
					// Rename downloaded file
					File.Move(localTempFileFullPath, localFileFullPath);
				}

				client.Dispose();
				client = null;
				worker.ReportProgress(100, "Done.");
			});
		}
	}

}
