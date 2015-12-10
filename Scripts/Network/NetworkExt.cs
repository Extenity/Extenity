#if !UNITY_WEBPLAYER

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

public static class NetworkExtensions
{
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
}

public static class IPAddressExtensions
{
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
}

#endif
