//----------------------------------------------------------------------------------------------------
// <copyright company="Avira Operations GmbH & Co. KG and its licensors">
// © 2016 Avira Operations GmbH & Co. KG and its licensors.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------
// Heavily modified to use in Extenity
//----------------------------------------------------------------------------------------------------

using System;

namespace Extenity.WWWToolbox.FileDownloader
{

	/// <summary>
	/// DownloadFileCompleted event args
	/// </summary>
	public class DownloadFileCompletedArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DownloadFileCompletedArgs"/> class
		/// </summary>
		/// <param name="result">Result of download</param>
		/// <param name="fileName">Local path to downloaded file</param>
		/// <param name="fileSource">Downloaded file source</param>
		/// <param name="downloadTime">Downloaded time</param>
		/// <param name="bytesTotal">File size</param>
		/// <param name="bytesReceived">Received bytes</param>
		/// <param name="error">Exception object</param>
		public DownloadFileCompletedArgs(FileDownloaderResult result, string fileName, Uri fileSource, TimeSpan downloadTime, long bytesTotal, long bytesReceived, Exception error)
		{
			Result = result;
			FileName = fileName;
			FileSource = fileSource;
			Error = error;
			DownloadTime = downloadTime;
			BytesTotal = bytesTotal;
			BytesReceived = bytesReceived;
		}

		/// <summary>
		/// Gets the download result 
		/// </summary>
		public FileDownloaderResult Result { get; private set; }

		/// <summary>
		/// Gets the name of downloaded file
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// Gets the download source
		/// </summary>
		public Uri FileSource { get; private set; }

		/// <summary>
		/// Gets the error, or null if there is no error
		/// </summary>
		public Exception Error { get; private set; }

		/// <summary>
		/// Gets the total download time
		/// </summary>
		public TimeSpan DownloadTime { get; private set; }

		/// <summary>
		/// Gets the number of received bytes
		/// </summary>
		public long BytesReceived { get; private set; }

		/// <summary>
		/// Gets the number of total bytes which should be received
		/// </summary>
		public long BytesTotal { get; private set; }

		/// <summary>
		/// Gets the download progress in percent, from 0 to 100
		/// </summary>
		public int DownloadProgress
		{
			get
			{
				if (BytesTotal <= 0 || BytesReceived <= 0)
				{
					return 0;
				}
				return Convert.ToInt32((float)BytesReceived / BytesTotal * 100);
			}
		}

		/// <summary>
		/// Gets the download speed in kilobytes per second
		/// </summary>
		public int AverageDownloadSpeed
		{
			get
			{
				if (DownloadTime == TimeSpan.Zero || BytesReceived == 0)
				{
					return 0;
				}

				var kiloBytesReceived = BytesReceived / 1024.0;

				return Convert.ToInt32(kiloBytesReceived / DownloadTime.TotalSeconds);
			}
		}
	}

}