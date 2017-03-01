using System.Net;

namespace Extenity.WorldWideWeb
{

	public static class WebExtensions
	{
		public static long GetContentLength(this WebHeaderCollection responseHeaders)
		{
			long contentLength = -1;
			if (responseHeaders != null)
			{
				var contentLengthString = responseHeaders["Content-Length"];
				if (!string.IsNullOrEmpty(contentLengthString))
				{
					long.TryParse(contentLengthString, out contentLength);
				}
			}
			return contentLength;
		}
	}

}
