using UnityEngine;
using System.Net;
using System.IO;
using Extenity.Logging;
using Logger = Extenity.Logging.Logger;

public static class NetworkTools
{
	public static bool CheckForAddressInconsistency(string address)
	{
		if (string.IsNullOrEmpty(address))
			return false;
		return true;
	}

	public static bool CheckForPortInconsistency(string port)
	{
		if (string.IsNullOrEmpty(port))
			return false;
		if (!port.IsNumeric())
			return false;
		return CheckForPortInconsistency(int.Parse(port));
	}

	public static bool CheckForPortInconsistency(int port)
	{
		return port > 0 && port < 65536;
	}

	public static string QueryExternalIP()
	{
		HttpWebRequest request = HttpWebRequest.Create("http://www.whatismyip.org/") as HttpWebRequest;
		if (request == null)
			return string.Empty;

		request.UserAgent = "User-Agent: Mozilla/4.0 (compatible; MSIE6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
		HttpWebResponse response = request.GetResponse() as HttpWebResponse;
		if (response == null)
			return string.Empty;

		StreamReader reader = new StreamReader(response.GetResponseStream());
		string externalIP = reader.ReadToEnd();
		reader.Close();

		response.Close();
		return externalIP;
	}

	public static string GetIPFromURL(string url)
	{
		url = url.Replace("http://", ""); //remove http://
		url = url.Replace("https://", ""); //remove https://
		if (url.IndexOf("/") > 0)
		{
			url = url.Substring(0, url.IndexOf("/")); //remove everything after the first /
		}

		try
		{
			IPHostEntry hosts = Dns.GetHostEntry(url);
			if (hosts.AddressList.Length > 0)
				return hosts.AddressList[0].ToString();
		}
		catch
		{
			// ignored
		}
		return string.Empty;
	}
}
