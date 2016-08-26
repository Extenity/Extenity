using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEngine;
using Extenity.Parallel;

namespace Extenity.WorldWideWeb
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
			BaseAddress = string.Format("ftp://{0}/{1}", Host, BaseDirectory);
		}

		public DeferredExecutionState Download(string remoteFileRelativePath, string localFileFullPath)
		{
			return DeferredExecution.ExecuteInBackground((sender, args) =>
			{
				var worker = (BackgroundWorker)sender;

				// Initialize paths
				localFileFullPath = localFileFullPath.FixDirectorySeparatorChars(); // To system defaults
				remoteFileRelativePath = remoteFileRelativePath.FixDirectorySeparatorChars('/'); // To FTP
				string localFileName = Path.GetFileName(localFileFullPath);
				string localTempFileFullPath = Path.Combine(Path.GetDirectoryName(localFileFullPath), TempFileNamePrefix + localFileName);
				var remoteFileFullPath = BaseAddress + remoteFileRelativePath;

				var ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(remoteFileFullPath);
				ftpWebRequest.Credentials = Credentials;
				ftpWebRequest.KeepAlive = true;
				ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
				ftpWebRequest.UseBinary = true;

				FtpWebResponse ftpWebResponse = (FtpWebResponse)ftpWebRequest.GetResponse();
				Stream inputStream = ftpWebResponse.GetResponseStream();
				long fileSize = ftpWebResponse.ContentLength;
				var humanReadableFileSize = fileSize.ToFileSizeString();

				//if (inputStream == null)
				//{
				//    worker.CancelAsync();
				//    Debug.LogError("FTP Web Response GetResponseStream is NULL!!");
				//    return;
				//}

				using (var fileStream = File.OpenWrite(localTempFileFullPath))
				{
					var buffer = new byte[32768];
					int read;
					var total = 0L;

					while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
					{
						if (worker.CancellationPending)
						{
							args.Cancel = true;
							return;
						}

						fileStream.Write(buffer, 0, read);

						total += read;
						var message = string.Format("{0} / {1}", total.ToFileSizeString(), humanReadableFileSize);
						var progress = Mathf.CeilToInt(100f * total / fileSize);
						worker.ReportProgress(progress, message);
					}
					fileStream.Close();
				}

				// Rename downloaded file from temp filename to original filename
				File.Move(localTempFileFullPath, localFileFullPath);
			});
		}

		public DeferredExecutionState Upload(string remoteFileRelativePath, string localFileFullPath)
		{
			return DeferredExecution.ExecuteInBackground((sender, args) =>
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
				//var remoteFileFullPath = BaseAddress + remoteFileRelativePath;
				var remoteTempFileFullPath = BaseAddress + remoteTempFileRelativePath;

				// Get file size
				var fileSize = new FileInfo(localFileFullPath).Length;
				var humanReadableFileSize = fileSize.ToFileSizeString();

				// Initialize web request to upload file (with temporary name)
				var ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(remoteTempFileFullPath);
				ftpWebRequest.Credentials = Credentials;
				ftpWebRequest.KeepAlive = true;
				ftpWebRequest.Method = WebRequestMethods.Ftp.UploadFile;
				ftpWebRequest.UseBinary = true;

				using (var output = ftpWebRequest.GetRequestStream())
				{

					using (var input = File.OpenRead(localFileFullPath))
					{
						var buffer = new byte[32768];
						int read;
						var total = 0L;

						while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
						{
							if (worker.CancellationPending)
							{
								args.Cancel = true;
								return;
							}

							output.Write(buffer, 0, read);

							total += read;
							var message = string.Format("{0} / {1}", total.ToFileSizeString(), humanReadableFileSize);
							var progress = Mathf.CeilToInt(100f * total / fileSize);
							worker.ReportProgress(progress, message);
						}
					}
				}

				// Change file name from temp to original
				ftpWebRequest = (FtpWebRequest)FtpWebRequest.Create(remoteTempFileFullPath);
				ftpWebRequest.Credentials = Credentials;
				ftpWebRequest.KeepAlive = true;
				ftpWebRequest.Method = WebRequestMethods.Ftp.Rename;
				ftpWebRequest.RenameTo = remoteFileName;
				ftpWebRequest.GetResponse();
			});
		}
	}

}
