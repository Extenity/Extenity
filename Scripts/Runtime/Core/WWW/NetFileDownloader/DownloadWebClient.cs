using System;
using System.ComponentModel;
using System.Net;

namespace Extenity.WWWToolbox.FileDownloader
{

	[DesignerCategory("Code")]
	internal class DownloadWebClient : WebClient
	{
		private readonly CookieContainer cookieContainer = new CookieContainer();
		private WebResponse webResponse;
		private long position;

		private TimeSpan timeout = TimeSpan.FromMinutes(2);

		public bool HasResponse
		{
			get { return webResponse != null; }
		}

		public bool IsPartialResponse
		{
			get
			{
				var response = webResponse as HttpWebResponse;
				return response != null && response.StatusCode == HttpStatusCode.PartialContent;
			}
		}

		public void OpenReadAsync(Uri address, long newPosition)
		{
			position = newPosition;
			OpenReadAsync(address);
		}

		protected override WebResponse GetWebResponse(WebRequest request)
		{
			var response = base.GetWebResponse(request);
			webResponse = response;
			return response;
		}

		protected override WebResponse GetWebResponse(WebRequest request, IAsyncResult result)
		{
			var response = base.GetWebResponse(request, result);
			webResponse = response;
			return response;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			var request = base.GetWebRequest(address);

			if (request != null)
			{
				request.Timeout = (int)timeout.TotalMilliseconds;
			}

			var webRequest = request as HttpWebRequest;
			if (webRequest == null)
			{
				return request;
			}

			webRequest.ReadWriteTimeout = (int)timeout.TotalMilliseconds;
			webRequest.Timeout = (int)timeout.TotalMilliseconds;
			if (position != 0)
			{
				webRequest.AddRange((int)position);
				webRequest.Accept = "*/*"; /**/ // Quick hack to prevent Code Correct to falsely detect comment characters in string.
			}
			webRequest.CookieContainer = cookieContainer;
			return request;
		}
	}

}
