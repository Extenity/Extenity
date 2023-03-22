using System;
using System.IO;
using System.Net;
using Extenity.FileSystemToolbox;

namespace Extenity.WWWToolbox
{

	/// <summary>
	/// Reference: http://www.codeproject.com/Tips/443588/Simple-Csharp-FTP-Class
	/// </summary>
	public class FTP
	{
		private string host = null;
		private string user = null;
		private string pass = null;
		private FtpWebRequest ftpRequest = null;
		private FtpWebResponse ftpResponse = null;
		private Stream ftpStream = null;
		private int bufferSize = 2048;

		public string Host { get { return host; } }

		/// <summary>
		/// Construct Object
		/// </summary>
		public FTP(string hostIP, string userName, string password)
		{
			host = hostIP;
			user = userName;
			pass = password;
		}

		/// <summary>
		/// Download file and save it to the specified path.
		/// </summary>
		public void Download(string remoteFile, string localFile)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host.AddDirectorySeparatorToEnd('/') + remoteFile);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Get the FTP Server's Response Stream */
				ftpStream = ftpResponse.GetResponseStream();
				/* Open a File Stream to Write the Downloaded File */
				FileStream localFileStream = new FileStream(localFile, FileMode.Create);
				/* Buffer for the Downloaded Data */
				byte[] byteBuffer = new byte[bufferSize];
				int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
				/* Download the File by Writing the Buffered Data Until the Transfer is Complete */
				try
				{
					while (bytesRead > 0)
					{
						localFileStream.Write(byteBuffer, 0, bytesRead);
						bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
					}
				}
				catch (Exception exception) { Log.Error(exception); }
				/* Resource Cleanup */
				localFileStream.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception exception) { Log.Error(exception); }
			return;
		}

		/// <summary>
		/// Download file into specified stream.
		/// </summary>
		public void Download(string remoteFile, Stream downloadStream)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host.AddDirectorySeparatorToEnd('/') + remoteFile);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Get the FTP Server's Response Stream */
				ftpStream = ftpResponse.GetResponseStream();
				/* Buffer for the Downloaded Data */
				byte[] byteBuffer = new byte[bufferSize];
				int bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
				/* Download the File by Writing the Buffered Data Until the Transfer is Complete */
				try
				{
					while (bytesRead > 0)
					{
						downloadStream.Write(byteBuffer, 0, bytesRead);
						bytesRead = ftpStream.Read(byteBuffer, 0, bufferSize);
					}
				}
				catch (Exception exception) { Log.Error(exception); }
				/* Resource Cleanup */
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception exception) { Log.Error(exception); }
			return;
		}

		/// <summary>
		/// Upload File
		/// </summary>
		public bool Upload(string remoteFile, string localFile)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host.AddDirectorySeparatorToEnd('/') + remoteFile);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpRequest.GetRequestStream();
				/* Open a File Stream to Read the File for Upload */
				FileStream localFileStream = new FileStream(localFile, FileMode.Open);
				/* Buffer for the Downloaded Data */
				byte[] byteBuffer = new byte[bufferSize];
				int bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
				/* Upload the File by Sending the Buffered Data Until the Transfer is Complete */
				try
				{
					while (bytesSent != 0)
					{
						ftpStream.Write(byteBuffer, 0, bytesSent);
						bytesSent = localFileStream.Read(byteBuffer, 0, bufferSize);
					}
				}
				catch (Exception exception)
				{
					Log.Error(exception);
					return false;
				}
				finally
				{
					/* Resource Cleanup */
					localFileStream.Close();
					ftpStream.Close();
					ftpRequest = null;
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Delete File
		/// </summary>
		public void Delete(string deleteFile)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)WebRequest.Create(host.AddDirectorySeparatorToEnd('/') + deleteFile);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Resource Cleanup */
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception exception) { Log.Error(exception); }
			return;
		}

		/// <summary>
		/// Rename File
		/// </summary>
		public bool Rename(string currentFileNameAndPath, string newFileName, bool ignoreErrors = false)
		{
			try
			{
				/* Create an FTP Request */
				var path = string.IsNullOrEmpty(currentFileNameAndPath)
					? host.RemoveEndingDirectorySeparatorChar()
					: host.AddDirectorySeparatorToEnd('/') + currentFileNameAndPath;
				ftpRequest = (FtpWebRequest)WebRequest.Create(path);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.Rename;
				/* Rename the File */
				ftpRequest.RenameTo = newFileName;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Resource Cleanup */
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception exception)
			{
				if (!ignoreErrors)
				{
					Log.Error(exception);
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Create a New Directory on the FTP Server
		/// </summary>
		public bool CreateDirectory(string newDirectory)
		{
			try
			{
				/* Create an FTP Request */
				var path = string.IsNullOrEmpty(newDirectory)
					? host.RemoveEndingDirectorySeparatorChar()
					: host.AddDirectorySeparatorToEnd('/') + newDirectory;
				ftpRequest = (FtpWebRequest)WebRequest.Create(path);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Resource Cleanup */
				ftpResponse.Close();
				ftpRequest = null;
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Get the Date/Time a File was Created
		/// </summary>
		public string GetFileCreatedDateTime(string fileName)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host.AddDirectorySeparatorToEnd('/') + fileName);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string fileInfo = null;
				/* Read the Full Response Stream */
				try { fileInfo = ftpReader.ReadToEnd(); }
				catch (Exception exception) { Log.Error(exception); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return File Created Date Time */
				return fileInfo;
			}
			catch (Exception exception) { Log.Error(exception); }
			/* Return an Empty string Array if an Exception Occurs */
			return "";
		}

		/// <summary>
		/// Get the Size of a File
		/// </summary>
		public string GetFileSize(string fileName)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host.AddDirectorySeparatorToEnd('/') + fileName);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string fileInfo = null;
				/* Read the Full Response Stream */
				try { while (ftpReader.Peek() != -1) { fileInfo = ftpReader.ReadToEnd(); } }
				catch (Exception exception) { Log.Error(exception); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return File Size */
				return fileInfo;
			}
			catch (Exception exception) { Log.Error(exception); }
			/* Return an Empty string Array if an Exception Occurs */
			return "";
		}

		/// <summary>
		/// List Directory Contents File/Folder Name Only
		/// </summary>
		public string[] DirectoryListSimple(string directory)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host.AddDirectorySeparatorToEnd('/') + directory);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string directoryRaw = null;
				/* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
				try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
				catch (Exception exception) { Log.Error(exception); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
				try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
				catch (Exception exception) { Log.Error(exception); }
			}
			catch (Exception exception) { Log.Error(exception); }
			/* Return an Empty string Array if an Exception Occurs */
			return new string[] { "" };
		}

		/// <summary>
		/// List Directory Contents in Detail (Name, Size, Created, etc.)
		/// </summary>
		public string[] DirectoryListDetailed(string directory)
		{
			try
			{
				/* Create an FTP Request */
				ftpRequest = (FtpWebRequest)FtpWebRequest.Create(host.AddDirectorySeparatorToEnd('/') + directory);
				/* Log in to the FTP Server with the User Name and Password Provided */
				ftpRequest.Credentials = new NetworkCredential(user, pass);
				/* When in doubt, use these options */
				ftpRequest.UseBinary = true;
				ftpRequest.UsePassive = true;
				ftpRequest.KeepAlive = true;
				/* Specify the Type of FTP Request */
				ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
				/* Establish Return Communication with the FTP Server */
				ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
				/* Establish Return Communication with the FTP Server */
				ftpStream = ftpResponse.GetResponseStream();
				/* Get the FTP Server's Response Stream */
				StreamReader ftpReader = new StreamReader(ftpStream);
				/* Store the Raw Response */
				string directoryRaw = null;
				/* Read Each Line of the Response and Append a Pipe to Each Line for Easy Parsing */
				try { while (ftpReader.Peek() != -1) { directoryRaw += ftpReader.ReadLine() + "|"; } }
				catch (Exception exception) { Log.Error(exception); }
				/* Resource Cleanup */
				ftpReader.Close();
				ftpStream.Close();
				ftpResponse.Close();
				ftpRequest = null;
				/* Return the Directory Listing as a string Array by Parsing 'directoryRaw' with the Delimiter you Append (I use | in This Example) */
				try { string[] directoryList = directoryRaw.Split("|".ToCharArray()); return directoryList; }
				catch (Exception exception) { Log.Error(exception); }
			}
			catch (Exception exception) { Log.Error(exception); }
			/* Return an Empty string Array if an Exception Occurs */
			return new string[] { "" };
		}

		#region Log

		private static readonly Logger Log = new(nameof(FTP));

		#endregion
	}

}
