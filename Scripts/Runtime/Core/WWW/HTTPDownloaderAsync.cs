using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Extenity.FileSystemToolbox;
using Extenity.ParallelToolbox;
using Extenity.WWWToolbox.FileDownloader;
using Timer = System.Timers.Timer;

namespace Extenity.WWWToolbox
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

		public string BuildLocalTempFileFullPath(string localFileFullPath)
		{
			localFileFullPath = localFileFullPath.FixDirectorySeparatorChars(); // To system defaults
			var localFileName = Path.GetFileName(localFileFullPath);
			var localDirectoryPath = Path.GetDirectoryName(localFileFullPath).AddDirectorySeparatorToEnd();
			return Path.Combine(localDirectoryPath, TempFileNamePrefix + localFileName);
		}

		public DeferredExecutionController CreateDownloadJob(string remoteFileRelativePath, string localFileFullPath, bool extractCompressedFileAndDelete = false, Func<Stream, bool> hashCalculator = null)
		{
			return DeferredExecution.Setup((sender, args) =>
			{
				var worker = (BackgroundWorker)sender;

				// Initialize paths
				localFileFullPath = localFileFullPath.FixDirectorySeparatorChars(); // To system defaults
				remoteFileRelativePath = remoteFileRelativePath.FixDirectorySeparatorChars('/'); // To HTTP
				var localTempFileFullPath = BuildLocalTempFileFullPath(localFileFullPath);
				var remoteFileFullPath = BaseAddress + remoteFileRelativePath;

				var waitEvent = new ManualResetEvent(false);
				Exception downloadError = null;

				// Get the object used to communicate with the server.
				{
					// These were from the previous implementation that uses WebClient.
					//var client = new WebClient();
					////client.UseDefaultCredentials
					////fileSizeRequest.Credentials = Credentials;
					//client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
					//client.Encoding = Encoding.UTF8;
				}
				var fileDownloader = new FileDownloader.FileDownloader();

				var timedOut = false;
				var timeoutTimer = new Timer();
				timeoutTimer.AutoReset = false;
				timeoutTimer.Enabled = false;
				timeoutTimer.Interval = 30000;
				timeoutTimer.Elapsed += (o, eventArgs) =>
				{
					timedOut = true;
					fileDownloader.CancelDownloadAsync();
				};

				fileDownloader.DownloadProgressChanged += (o, eventArgs) =>
				{
					timeoutTimer.Stop();
					timeoutTimer.Start();
					var downloadedSize = eventArgs.BytesReceived;
					var fileSize = eventArgs.TotalBytesToReceive;
					var humanReadableFileSize = fileSize.ToFileSizeString();

					var message = $"{downloadedSize.ToFileSizeString()} / {humanReadableFileSize}";
					var ratio = (double)downloadedSize / fileSize;
					var progress = (int)(99f * ratio);
					if (progress < 0) progress = 0;
					if (progress > 99) progress = 99;
					worker.ReportProgress(progress, message);

					// Cancel if requested
					if (worker.CancellationPending)
					{
						fileDownloader.CancelDownloadAsync();
						args.Cancel = true;
					}
				};

				fileDownloader.DownloadFileCompleted += (o, eventArgs) =>
				{
					timeoutTimer.Stop();

					if (eventArgs.Error != null || eventArgs.Result == FileDownloaderResult.Failed)
					{
						downloadError = eventArgs.Error;
						args.Cancel = true;
					}
					if (eventArgs.Result == FileDownloaderResult.Cancelled)
					{
						args.Cancel = true;
					}
					waitEvent.Set();
				};

				// Create directory
				DirectoryTools.CreateFromFilePath(localTempFileFullPath);

				fileDownloader.DownloadFileAsync(new Uri(remoteFileFullPath), localTempFileFullPath);
				timeoutTimer.Start();
				waitEvent.WaitOne();
				waitEvent = null;
				timeoutTimer.Stop();
				fileDownloader.Dispose();
				fileDownloader = null;

				if (timedOut)
				{
					throw new Exception("Connection timed out.");
				}

				if (downloadError != null)
				{
					throw downloadError;
				}

				worker.ReportProgress(99, "Finalizing file");
				if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested

				// Extract downloaded file and delete
				if (extractCompressedFileAndDelete)
				{
					using (var memoryStream = new MemoryStream())
					{
						try
						{
#if UseSharpZipLib
							ICSharpCode.SharpZipLib.Extensions.SharpZipLibTools.ExtractSingleFileEnsured(localTempFileFullPath, memoryStream);
#else
#pragma warning disable 162
							throw new System.NotImplementedException();
#endif
						}
						catch (Exception exception)
						{
							throw new FileCorruptException("Failed to extract file.", exception);
						}

						// Calculate hash
						if (hashCalculator != null)
						{
							var hashResult = false;
							try
							{
								memoryStream.Position = 0;
								hashResult = hashCalculator(memoryStream);
							}
							catch (Exception exception)
							{
								throw new Exception("Failed to calculate hash.", exception);
							}
							if (!hashResult)
							{
								throw new FileCorruptException("Downloaded file hash does not match.");
							}
						}

						if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested

						// Write memory to file
						using (var fileStream = new FileStream(localFileFullPath, FileMode.Create, FileAccess.Write))
						{
							memoryStream.Position = 0;
							memoryStream.WriteTo(fileStream);
						}
					}

					// Delete compressed (downloaded) file
					FileTools.Delete(localTempFileFullPath);
				}
				else
				{
					// Calculate hash
					if (hashCalculator != null)
					{
						var hashResult = false;
						try
						{
							using (var fileStream = File.Open(localTempFileFullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
							{
								hashResult = hashCalculator(fileStream);
							}
						}
						catch (Exception exception)
						{
							throw new Exception("Failed to calculate hash.", exception);
						}
						if (!hashResult)
						{
							throw new FileCorruptException("Downloaded file hash does not match.");
						}
					}

					if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested

					// Rename downloaded file from temp filename to original filename
					// Delete if file already exists.
					FileTools.Delete(localFileFullPath, true);
					// Rename downloaded file
					File.Move(localTempFileFullPath, localFileFullPath);
				}

				worker.ReportProgress(100, "Done.");
			});
		}
	}

}
