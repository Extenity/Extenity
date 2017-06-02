using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using Extenity.DataToolbox;

namespace Extenity.NetworkToolbox
{

	public static class NetworkTools
	{
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

				var hosts = Dns.GetHostEntry(url);
				if (hosts.AddressList.Length > 0)
					return hosts.AddressList[0].ToString();
			}
			catch
			{
				// ignored
			}
			return string.Empty;
		}

		#region External IP / Local IP

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

		#endregion

		#region Broadcast Addresses

		public static List<IPAddress> GetAllBroadcastAddressesOfLocalComputer()
		{
			var list = new List<IPAddress>();

			foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (networkInterface.OperationalStatus != OperationalStatus.Up)
					continue;

				//Debug.Write("----------------------------");
				//Debug.Write(networkInterface.Name);
				//Debug.Write("MAC: " + networkInterface.GetPhysicalAddress());
				//Debug.Write("Gateways:");

				//foreach (GatewayIPAddressInformation gipi in networkInterface.GetIPProperties().GatewayAddresses)
				//{
				//    Debug.Write(string.Format("\t{0}", gipi.Address));
				//}

				//Debug.Write("IP Addresses:");

				foreach (UnicastIPAddressInformation uipi in networkInterface.GetIPProperties().UnicastAddresses)
				{
					//Debug.Write(string.Format("\t{0} / {1}", uipi.Address, uipi.IPv4Mask));
					if (!uipi.Address.IsNullOrZero() && !uipi.IPv4Mask.IsNullOrZero())
					{
						var broadcastIPAddress = uipi.Address.GetBroadcastAddress(uipi.IPv4Mask);
						if (!broadcastIPAddress.IsNullOrZero())
						{
							list.Add(broadcastIPAddress);
							//Debug.Write(string.Format("\tbroadcast: {0}", broadcastIPAddress));
						}
					}
				}
			}

			return list;
		}

		#endregion

		#region IPAddress Extensions

		public static bool IsNullOrZero(this IPAddress address)
		{
			if (address == null)
				return true;

			byte[] addressBytes = address.GetAddressBytes();

			foreach (var addressByte in addressBytes)
			{
				if (addressByte != 0)
					return false;
			}
			return true;
		}

		public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
		{
			byte[] addressBytes = address.GetAddressBytes();
			byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

			if (addressBytes.Length != subnetMaskBytes.Length)
				return null;

			byte[] broadcastAddress = new byte[addressBytes.Length];
			for (int i = 0; i < broadcastAddress.Length; i++)
			{
				broadcastAddress[i] = (byte)(addressBytes[i] | (subnetMaskBytes[i] ^ 255));
			}
			return new IPAddress(broadcastAddress);
		}

		public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
		{
			byte[] ipAdressBytes = address.GetAddressBytes();
			byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

			if (ipAdressBytes.Length != subnetMaskBytes.Length)
				throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

			byte[] broadcastAddress = new byte[ipAdressBytes.Length];
			for (int i = 0; i < broadcastAddress.Length; i++)
			{
				broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
			}
			return new IPAddress(broadcastAddress);
		}

		public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
		{
			IPAddress network1 = address.GetNetworkAddress(subnetMask);
			IPAddress network2 = address2.GetNetworkAddress(subnetMask);

			return network1.Equals(network2);
		}

		#endregion

		#region Consistency

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

		#endregion
	}

}
