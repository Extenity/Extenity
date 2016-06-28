using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using Extenity.DataTypes;

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
		try
		{
			url = url.Replace("http://", ""); //remove http://
			url = url.Replace("https://", ""); //remove https://
			if (url.IndexOf("/") > 0)
			{
				url = url.Substring(0, url.IndexOf("/")); //remove everything after the first /
			}

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

	/// Source: http://stackoverflow.com/questions/6803073/get-local-ip-address
	/// Explanation in StackOverflow: There is a more accurate way when there are multi ip addresses available on local machine. Just try to make a UDP connection to a target, the target no need to be existed at all. And you will receive the preferred outbound IP address of local machine. 
	public static string GetPreferredLocalIP()
	{
		try
		{
			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
			{
				socket.Connect("1.0.0.0", 65530);
				var endPoint = (IPEndPoint)socket.LocalEndPoint;
				return endPoint.Address.ToString();
			}
		}
		catch (Exception)
		{
			return "";
		}
	}

	/// Source: http://www.csharp-examples.net/local-ip/
	/// Examples:
	/// IsLocalIP("localhost");        // true (loopback name)
	/// IsLocalIP("127.0.0.1");        // true (loopback IP)
	/// IsLocalIP("MyNotebook");       // true (my computer name)
	/// IsLocalIP("192.168.0.1");      // true (my IP)
	/// IsLocalIP("NonExistingName");  // false (non existing computer name)
	/// IsLocalIP("99.0.0.1");         // false (non existing IP in my net)
	public static bool IsLocalIP(string host)
	{
		try
		{ // get host IP addresses
			IPAddress[] hostIPs = Dns.GetHostAddresses(host);
			// get local IP addresses
			IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

			// test if any host IP equals to any local IP or to localhost
			foreach (IPAddress hostIP in hostIPs)
			{
				// is localhost
				if (IPAddress.IsLoopback(hostIP))
					return true;
				// is local address
				foreach (IPAddress localIP in localIPs)
				{
					if (hostIP.Equals(localIP))
						return true;
				}
			}
		}
		catch { }
		return false;
	}
}
