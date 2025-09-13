using System;
using System.ComponentModel;
using System.Net;

namespace Extenity.WWWToolbox.FileDownloader
{

	[DesignerCategory("Code")]
	internal class DownloadWebClient : WebClient
	{
		private readonly CookieContainer CookieContainer = new CookieContainer();
		private WebResponse WebResponse;
		private long Position;

		private TimeSpan Timeout = TimeSpan.FromMinutes(2);

		public bool HasResponse
		{
			get { return WebResponse != null; }
		}

		public bool IsPartialResponse
		{
			get
			{
				var response = WebResponse as HttpWebResponse;
				return response != null && response.StatusCode == HttpStatusCode.PartialContent;
			}
		}

		public void OpenReadAsync(Uri address, long newPosition)
		{
			Position = newPosition;
			OpenReadAsync(address);
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			var response = base.GetWebResponse(request);
			WebResponse = response;
			return response;
		}

		protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
		{
			var response = base.GetWebResponse(request, result);
			WebResponse = response;
			return response;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address);

			if (request != null)
			{
				request.Timeout = (int)Timeout.TotalMilliseconds;
			}

			var webRequest = request as HttpWebRequest;
			if (webRequest == null)
			{
				return request;
			}

			webRequest.ReadWriteTimeout = (int)Timeout.TotalMilliseconds;
			webRequest.Timeout = (int)Timeout.TotalMilliseconds;
			if (Position != 0)
			{
				webRequest.AddRange((int)Position);
				webRequest.Accept = "*/*"; /**/ // Quick hack to prevent Code Correct to falsely detect comment characters in string.
			}
			webRequest.CookieContainer = CookieContainer;
			return request;
		}
	}

}
