using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Extenity.FileSystemToolbox;
using Extenity.ParallelToolbox;

namespace Extenity.WWWToolbox
{

	public class FTPAsync
	{
		private readonly NetworkCredential Credentials;
		public readonly string Host;
		public readonly string BaseDirectory;
		public readonly string BaseAddress;
		public readonly string TempFileNamePrefix;

		public FTPAsync(string host, string username, string password, string baseDirectory = null, string tempFileNamePrefix = "_inprogress.")
		{
			Host = host;
			Credentials = new NetworkCredential
			{
				UserName = username,
				Password = password
			};
			if (!string.IsNullOrEmpty(baseDirectory))
			{
				BaseDirectory = baseDirectory.FixDirectorySeparatorChars('/').AddDirectorySeparatorToEnd('/');
			}
			if (string.IsNullOrEmpty(tempFileNamePrefix))
			{
				throw new Exception("Temp prefix was not defined.");
			}
			TempFileNamePrefix = tempFileNamePrefix;
			BaseAddress = $"ftp://{Host}/{BaseDirectory}";
		}

		public DeferredExecutionController CreateDownloadJob(string remoteFileRelativePath, string localFileFullPath, bool extractCompressedFileAndDelete = false)
		{
			return DeferredExecution.Setup((sender, args) =>
			{
				var worker = (BackgroundWorker)sender;

				// Initialize paths
				localFileFullPath = localFileFullPath.FixDirectorySeparatorChars(); // To system defaults
				remoteFileRelativePath = remoteFileRelativePath.FixDirectorySeparatorChars('/'); // To FTP
				var localFileName = Path.GetFileName(localFileFullPath);
				var localDirectoryPath = Path.GetDirectoryName(localFileFullPath).AddDirectorySeparatorToEnd();
				var localTempFileFullPath = Path.Combine(localDirectoryPath, TempFileNamePrefix + localFileName);
				var remoteFileFullPath = BaseAddress + remoteFileRelativePath;

				// Request file size first
				long fileSize;
				{
					// Get the object used to communicate with the server.
					var fileSizeRequest = (FtpWebRequest)FtpWebRequest.Create(remoteFileFullPath);
					fileSizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
					fileSizeRequest.Credentials = Credentials;
					fileSizeRequest.Timeout = 120000;
					var fileSizeResponse = (FtpWebResponse)fileSizeRequest.GetResponse();
					//Stream responseStream = fileSizeResponse.GetResponseStream();
					fileSize = fileSizeResponse.ContentLength;
					fileSizeResponse.Close();
				}
				var humanReadableFileSize = fileSize.ToFileSizeString();

				var ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(remoteFileFullPath);
				ftpWebRequest.Credentials = Credentials;
				ftpWebRequest.KeepAlive = true;
				ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
				ftpWebRequest.UseBinary = true;
				ftpWebRequest.Timeout = 120000;
				if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested, just before making the request

				var ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
				var inputStream = ftpWebResponse.GetResponseStream();
				//long fileSize = ftpWebResponse.ContentLength; This is the wrong way to do this.
				//var humanReadableFileSize = fileSize.ToFileSizeString();

				//if (inputStream == null)
				//{
				//    worker.CancelAsync();
				//    Log.Error("FTP Web Response GetResponseStream is NULL!!");
				//    return;
				//}

				// Create directory
				DirectoryTools.CreateFromFilePath(localTempFileFullPath);

				using (var fileStream = File.OpenWrite(localTempFileFullPath))
				{
					var buffer = new byte[32768];
					int read;
					var total = 0L;

					while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested

						fileStream.Write(buffer, 0, read);

						total += read;
						var message = $"{total.ToFileSizeString()} / {humanReadableFileSize}";
						var ratio = (double)total / fileSize;
						var progress = (int)(99f * ratio);
						if (progress < 0) progress = 0;
						if (progress > 99) progress = 99;
						worker.ReportProgress(progress, message);
					}
					fileStream.Flush(); // In any case...
					fileStream.Close();
				}

				// Rename downloaded file from temp filename to original filename
				worker.ReportProgress(99, "Changing temporary file name");
				if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested, just before changing the file name


				if (extractCompressedFileAndDelete)
				{
					// Extract downloaded file and delete
#if UseSharpZipLib
					ICSharpCode.SharpZipLib.Extensions.SharpZipLibTools.ExtractSingleFileEnsured(File.OpenRead(localTempFileFullPath), localFileFullPath);
#else
#pragma warning disable 162
					throw new System.NotImplementedException();
#endif

					FileTools.Delete(localTempFileFullPath);
				}
				else
				{
					// Delete if file already exists.
					FileTools.Delete(localFileFullPath, true);
					// Rename downloaded file
					File.Move(localTempFileFullPath, localFileFullPath);
				}

				worker.ReportProgress(100, "Done.");
			});
		}

		public DeferredExecutionController CreateUploadJob(string remoteFileRelativePath, string localFileFullPath, bool useTemporaryNaming = true)
		{
			return DeferredExecution.Setup((sender, args) =>
			{
				var worker = (BackgroundWorker)sender;

				// Initialize paths
				localFileFullPath = localFileFullPath.FixDirectorySeparatorChars(); // To system defaults
				remoteFileRelativePath = remoteFileRelativePath.FixDirectorySeparatorChars('/'); // To FTP
				string remoteFileName;
				string remoteTempFileRelativePath;
				if (remoteFileRelativePath.Contains("/"))
				{
					remoteFileName = Path.GetFileName(remoteFileRelativePath);
					var filePath = Path.GetDirectoryName(remoteFileRelativePath);
					remoteTempFileRelativePath = filePath + '/' + TempFileNamePrefix + remoteFileName;
				}
				else
				{
					remoteFileName = remoteFileRelativePath;
					remoteTempFileRelativePath = TempFileNamePrefix + remoteFileRelativePath;
				}
				var remoteFileFullPath = BaseAddress + remoteFileRelativePath;
				var remoteTempFileFullPath = BaseAddress + remoteTempFileRelativePath;

				// Get file size
				var fileSize = new FileInfo(localFileFullPath).Length;
				var humanReadableFileSize = fileSize.ToFileSizeString();

				// Initialize web request to upload file (with temporary name)
				var ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(useTemporaryNaming ? remoteTempFileFullPath : remoteFileFullPath);
				ftpWebRequest.Credentials = Credentials;
				ftpWebRequest.KeepAlive = true;
				ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
				ftpWebRequest.UseBinary = true;
				ftpWebRequest.Timeout = 120000;
				if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested, just before making the request

				using (var output = ftpWebRequest.GetRequestStream())
				{

					using (var input = File.OpenRead(localFileFullPath))
					{
						var buffer = new byte[32768];
						int read;
						var total = 0L;

						while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
						{
							if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested

							output.Write(buffer, 0, read);

							total += read;
							var message = $"{total.ToFileSizeString()} / {humanReadableFileSize}";
							var ratio = (double)total / fileSize;
							var progress = (int)(99f * ratio);
							if (progress < 0) progress = 0;
							if (progress > 99) progress = 99;
							worker.ReportProgress(progress, message);
						}
					}
				}

				// Change file name from temp to original
				if (useTemporaryNaming)
				{
					worker.ReportProgress(99, "Changing temporary file name");
					ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(remoteTempFileFullPath);
					ftpWebRequest.Credentials = Credentials;
					ftpWebRequest.KeepAlive = true;
					ftpWebRequest.Method = WebRequestMethods.Ftp.Rename;
					ftpWebRequest.RenameTo = remoteFileName;
					if (worker.CancellationPending) { args.Cancel = true; return; } // Cancel if requested, just before making the request
					ftpWebRequest.GetResponse();
				}

				worker.ReportProgress(100, "Done.");
			});
		}
	}

}
