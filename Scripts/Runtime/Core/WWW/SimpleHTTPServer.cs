// MIT License - Copyright (c) 2016 Can Guney Aksakalli
// https://aksakalli.github.io/2014/02/24/simple-http-server-with-csparp.html
//
// Modified by Can Baycay - 2020.10.26

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Extenity.WWWToolbox
{

	public class SimpleHTTPServer
	{
		#region Configuration

		public string RootDirectory { get; private set; }
		public bool IsServingDirectory { get; private set; }
		public string Host { get; private set; }
		public int Port { get; private set; }

		public string[] IndexFiles = new string[]
		{
			"index.html",
			"index.htm",
			"default.html",
			"default.htm"
		};

		/// <summary>
		/// Mime configuration is shared across multiple server instances. Make sure to modify mime types before
		/// launching a server. Otherwise threading issues might appear.
		/// </summary>
		public static Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
		{
			{ ".asf", "video/x-ms-asf" },
			{ ".asx", "video/x-ms-asf" },
			{ ".avi", "video/x-msvideo" },
			{ ".bin", "application/octet-stream" },
			{ ".cco", "application/x-cocoa" },
			{ ".crt", "application/x-x509-ca-cert" },
			{ ".css", "text/css" },
			{ ".deb", "application/octet-stream" },
			{ ".der", "application/x-x509-ca-cert" },
			{ ".dll", "application/octet-stream" },
			{ ".dmg", "application/octet-stream" },
			{ ".ear", "application/java-archive" },
			{ ".eot", "application/octet-stream" },
			{ ".exe", "application/octet-stream" },
			{ ".flv", "video/x-flv" },
			{ ".gif", "image/gif" },
			{ ".hqx", "application/mac-binhex40" },
			{ ".htc", "text/x-component" },
			{ ".htm", "text/html" },
			{ ".html", "text/html" },
			{ ".ico", "image/x-icon" },
			{ ".img", "application/octet-stream" },
			{ ".iso", "application/octet-stream" },
			{ ".jar", "application/java-archive" },
			{ ".jardiff", "application/x-java-archive-diff" },
			{ ".jng", "image/x-jng" },
			{ ".jnlp", "application/x-java-jnlp-file" },
			{ ".jpeg", "image/jpeg" },
			{ ".jpg", "image/jpeg" },
			{ ".js", "application/x-javascript" },
			{ ".mml", "text/mathml" },
			{ ".mng", "video/x-mng" },
			{ ".mov", "video/quicktime" },
			{ ".mp3", "audio/mpeg" },
			{ ".mpeg", "video/mpeg" },
			{ ".mpg", "video/mpeg" },
			{ ".msi", "application/octet-stream" },
			{ ".msm", "application/octet-stream" },
			{ ".msp", "application/octet-stream" },
			{ ".pdb", "application/x-pilot" },
			{ ".pdf", "application/pdf" },
			{ ".pem", "application/x-x509-ca-cert" },
			{ ".pl", "application/x-perl" },
			{ ".pm", "application/x-perl" },
			{ ".png", "image/png" },
			{ ".prc", "application/x-pilot" },
			{ ".ra", "audio/x-realaudio" },
			{ ".rar", "application/x-rar-compressed" },
			{ ".rpm", "application/x-redhat-package-manager" },
			{ ".rss", "text/xml" },
			{ ".run", "application/x-makeself" },
			{ ".sea", "application/x-sea" },
			{ ".shtml", "text/html" },
			{ ".sit", "application/x-stuffit" },
			{ ".swf", "application/x-shockwave-flash" },
			{ ".tcl", "application/x-tcl" },
			{ ".tk", "application/x-tcl" },
			{ ".txt", "text/plain" },
			{ ".war", "application/java-archive" },
			{ ".wbmp", "image/vnd.wap.wbmp" },
			{ ".wmv", "video/x-ms-wmv" },
			{ ".xml", "text/xml" },
			{ ".xpi", "application/x-xpinstall" },
			{ ".zip", "application/zip" },
			{ ".unityweb", "application/octet-stream" }, // Unity WebGL
			{ ".unity3d", "application/vnd.unity" }, // Unity WebPlayer
			{ ".unitypackage", "application/x-gzip" }, // Unity package
		};

		#endregion

		#region Start/Stop Server

		/// <summary>
		/// Configures and runs the server.
		/// </summary>
		/// <param name="host">Host of the server. "localhost" means only local connections are allowed. See <see cref="https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener"/> for more.</param>
		/// <param name="port">Port of the server. 0 means a random port will be selected, which is accessible via <c>Port</c>.</param>
		/// <param name="rootDirectory">Directory path to serve. Empty string means no directory will be served. Use "." to serve current working directory.</param>
		public SimpleHTTPServer(string host = "localhost", int port = 0, string rootDirectory = "")
		{
			RootDirectory = rootDirectory?.Trim();
			IsServingDirectory = !string.IsNullOrEmpty(RootDirectory);
			if (port <= 0)
			{
				port = FindEmptyPort();
			}
			Host = host;
			Port = port;
			InitializeListening();
		}

		/// <summary>
		/// Stops the server. Create another instance from scratch to start the server again.
		/// </summary>
		public void Stop()
		{
			Log.Verbose($"Closing server at {Port}");
			DeinitializeCustomProcessors();
			DeinitializeListening();
		}

		#endregion

		#region Listening Thread

		private Thread ServerThread;
		private HttpListener Listener;

		private void InitializeListening()
		{
			ServerThread = new Thread(ListenThread);
			ServerThread.Start();
		}

		private void DeinitializeListening()
		{
			if (Listener != null)
			{
				Listener.Stop();
				Listener = null;
			}
			if (ServerThread != null)
			{
				ServerThread.Abort();
				ServerThread = null;
			}
		}

		private async void ListenThread()
		{
			Log.Verbose($"Listening at {Port}");
			Listener = new HttpListener();
			Listener.Prefixes.Add($"http://{Host}:{Port}/");
			Listener.Start();
			while (true)
			{
				try
				{
					var context = await Listener.GetContextAsync();
					ProcessRequest(context);
				}
				catch (ThreadAbortException)
				{
					break; // Exit the thread
				}
				catch (Exception exception)
				{
					Log.Fatal(exception);
				}
			}
		}

		#endregion

		#region Process Request

		private void ProcessRequest(HttpListenerContext context)
		{
			string path = default;
			try
			{
				path = context.Request.Url.AbsolutePath;
				Log.Verbose("Get: " + path);
				path = path.Substring(1);

				// See if any custom processor is eager to process this path.
				lock (CustomProcessors)
				{
					foreach (var processor in CustomProcessors)
					{
						if (processor(context, path))
						{
							// The processor is interested in the path and consumed it.
							return;
						}
					}
				}

				// Warmup for directory and file serving.
				{
					if (!IsServingDirectory)
					{
						// Nothing to do here. The server is not configured for serving files in a directory.
						Log.Verbose("Not found.");
						context.Response.StatusCode = (int)HttpStatusCode.NotFound;
						return;
					}
					// Try looking for an index file to be served if no path is specified.
					if (string.IsNullOrWhiteSpace(path))
					{
						var indexFilesCached = IndexFiles; // Required for thread safety.
						foreach (string indexFile in indexFilesCached)
						{
							if (File.Exists(Path.Combine(RootDirectory, indexFile)))
							{
								path = indexFile;
								break;
							}
						}
					}
					// Get the path of requested file in local machine. The resulting path is relative to
					// working directory. From now on, separators in 'path' are converted to system's
					// default Path.DirectorySeparatorChar.
					path = Path.Combine(RootDirectory, path);
				}

				// Querying a directory instead of a path. List the content.
				if (Directory.Exists(path))
				{
					if (path[path.Length - 1] != Path.DirectorySeparatorChar)
					{
						path += Path.DirectorySeparatorChar;
					}
					var stringBuilder = new StringBuilder();
					var subPaths = Directory.GetFileSystemEntries(path);
					stringBuilder.AppendLine("<html><body>");
					foreach (var subPath in subPaths)
					{
						var relativeSubPath = subPath.Substring(RootDirectory.Length);
						stringBuilder.Append("<a href=\"");
						stringBuilder.Append(relativeSubPath);
						stringBuilder.AppendLine("\"/>");
						stringBuilder.Append(relativeSubPath);
						stringBuilder.AppendLine("</a><br>");
					}
					stringBuilder.Append("</body></html>");

					WriteTextOutputResponse(context, stringBuilder.ToString());
					return;
				}
				else
				{
					// Debug.Log("Open file: " + path);
					using (var fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						Log.Verbose("Serving file at: " + path);

						// Adding permanent http response headers
						context.Response.ContentType = MimeTypes.TryGetValue(Path.GetExtension(path), out var mime) ? mime : "application/octet-stream";
						context.Response.ContentLength64 = fileStream.Length;
						context.Response.AddHeader("Date", DateTime.Now.ToString("r"));
						context.Response.AddHeader("Last-Modified", File.GetLastWriteTime(path).ToString("r"));
						context.Response.StatusCode = (int)HttpStatusCode.OK;

						// Send the data
						var buffer = new byte[1024 * 16];
						int readBytes;
						while ((readBytes = fileStream.Read(buffer, 0, buffer.Length)) > 0)
						{
							context.Response.OutputStream.Write(buffer, 0, readBytes);
						}
						return;
					}
				}
			}
			catch (DirectoryNotFoundException)
			{
				Log.Verbose("Directory not found: " + path);
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			}
			catch (FileNotFoundException)
			{
				Log.Verbose("File not found: " + path);
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			}
			catch (Exception exception)
			{
				Log.Fatal(exception);
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
			}
			finally
			{
				context.Response.OutputStream.Flush();
				context.Response.OutputStream.Close();
			}
		}

		#endregion

		#region Custom Request Processors

		public delegate bool CustomProcessorMethod(HttpListenerContext context, string path);

		private readonly List<CustomProcessorMethod> CustomProcessors = new List<CustomProcessorMethod>();

		private void DeinitializeCustomProcessors()
		{
			// Release delegate links. They are external objects. Keeping a reference to these objects here might result
			// in garbage collector to not release these objects properly.
			lock (CustomProcessors)
			{
				CustomProcessors.Clear();
			}
		}

		public void AddCustomProcessor(CustomProcessorMethod processorMethod)
		{
			lock (CustomProcessors)
			{
				if (!CustomProcessors.Contains(processorMethod))
				{
					CustomProcessors.Add(processorMethod);
				}
			}
		}

		public bool RemoveCustomProcessor(CustomProcessorMethod processorMethod)
		{
			lock (CustomProcessors)
			{
				return CustomProcessors.Remove(processorMethod);
			}
		}

		#endregion

		#region Response

		public void WriteJsonOutputResponse<T>(HttpListenerContext context, T obj, bool indentedFormatting = true)
		{
			var text = JsonConvert.SerializeObject(obj, indentedFormatting ? Formatting.Indented : Formatting.None);
			WriteTextOutputResponse(context, text);
		}

		public void WriteTextOutputResponse(HttpListenerContext context, string text)
		{
			Log.Verbose("Serving response:\n" + text);

			var bytes = Encoding.UTF8.GetBytes(text);

			context.Response.ContentType = "text/html";
			context.Response.ContentLength64 = bytes.Length;
			context.Response.StatusCode = (int)HttpStatusCode.OK;

			context.Response.OutputStream.Write(bytes, 0, bytes.Length);
		}

		public void WriteErrorJsonOutputResponse<T>(HttpListenerContext context, T errorObj, bool indentedFormatting = true, HttpStatusCode httpStatusCode = HttpStatusCode.NotAcceptable)
		{
			var text = JsonConvert.SerializeObject(errorObj, indentedFormatting ? Formatting.Indented : Formatting.None);
			WriteErrorTextOutputResponse(context, text, httpStatusCode);
		}

		public void WriteErrorTextOutputResponse(HttpListenerContext context, string errorMessage, HttpStatusCode httpStatusCode = HttpStatusCode.NotAcceptable)
		{
			Log.Verbose("Serving error response:\n" + errorMessage);

			var bytes = Encoding.UTF8.GetBytes(errorMessage);

			context.Response.ContentType = "text/html";
			context.Response.ContentLength64 = bytes.Length;
			context.Response.StatusCode = (int)httpStatusCode;

			context.Response.OutputStream.Write(bytes, 0, bytes.Length);
		}

		#endregion

		#region Tools

		private int FindEmptyPort()
		{
			var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}

		#endregion

		#region Log

        private Logger Log = new(nameof(SimpleHTTPServer));

		#endregion
	}

}
