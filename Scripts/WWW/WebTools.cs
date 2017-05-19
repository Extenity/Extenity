using System.Net;

namespace Extenity.WWWToolbox
{

	public static class WebTools
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
