using System.Text;
using ExitGames.Client.Photon;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using Photon.Realtime;
using UnityEngine;

namespace BeyondNetworking
{

	public static class PhotonTools
	{
		#region ToString Variations

		public static string ToHumanReadableString(this TypedLobby lobby)
		{
			if (lobby == null)
				return "N/A";
			return $"Lobby '{lobby.Name}' of type '{lobby.Type}'";
		}

		public static string ToHumanReadableString(this TypedLobbyInfo lobby)
		{
			if (lobby == null)
				return "N/A";
			return $"Lobby '{lobby.Name}' of type '{lobby.Type}' with '{lobby.PlayerCount}' players and '{lobby.RoomCount}' rooms";
		}

		public static string ToHumanReadableString(this RoomInfo room)
		{
			if (room == null)
				return "N/A";
			return $"Room '{room.Name}' ({room.PlayerCount}/{room.MaxPlayers}) {(room.IsOpen ? "Open" : "Closed")}, {(room.IsVisible ? "Visible" : "Hidden")} {(room.CustomProperties.IsNotNullAndEmpty() ? "Properties:\n" + room.CustomProperties.ToJoinedString() : "")}";
		}

		public static string NickNameOrRoomActorNumber(this Player player)
		{
			if (player == null)
				return "[Null]";
			return string.IsNullOrEmpty(player.NickName)
				? "ACT:" + player.ActorNumber.ToString()
				: player.NickName;
		}

		#endregion

		#region Serialization

		private static readonly byte[] _Buffer = new byte[sizeof(float) * 4]; // That's the size of a Quaternion, which is the largest data structure.

		public static int Serialize(this StreamBuffer outStream, string value)
		{
			// TODO: OPTIMIZATION: Reduce memory allocations if possible.
			lock (_Buffer)
			{
				var writtenSize = 0;
				var bytes = Encoding.UTF8.GetBytes(value);
				// Write size of the string
				Protocol.Serialize(bytes.Length, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				// Write the string bytes
				outStream.Write(bytes, 0, bytes.Length);
				writtenSize += bytes.Length;
				return writtenSize;
			}
		}

		public static int Serialize(this StreamBuffer outStream, int value)
		{
			lock (_Buffer)
			{
				var writtenSize = 0;
				Protocol.Serialize(value, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				return writtenSize;
			}
		}

		public static int Serialize(this StreamBuffer outStream, short value)
		{
			lock (_Buffer)
			{
				var writtenSize = 0;
				Protocol.Serialize(value, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				return writtenSize;
			}
		}

		public static int Serialize(this StreamBuffer outStream, float value)
		{
			lock (_Buffer)
			{
				var writtenSize = 0;
				Protocol.Serialize(value, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				return writtenSize;
			}
		}

		public static int SerializeAsShort(this StreamBuffer outStream, int value)
		{
			lock (_Buffer)
			{
				var writtenSize = 0;
				var valueShort = (short)value;
				Protocol.Serialize(valueShort, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				return writtenSize;
			}
		}

		public static int Serialize(this StreamBuffer outStream, bool value)
		{
			lock (_Buffer)
			{
				var writtenSize = 0;
				var valueAsShort = value ? (short)1 : (short)0; // TODO: Photon does not support bool serialization. Find a way to do it properly. See 7817575.
				Protocol.Serialize(valueAsShort, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				return writtenSize;
			}
		}

		public static int Serialize(this StreamBuffer outStream, Vector3 value)
		{
			lock (_Buffer)
			{
				var writtenSize = 0;
				Protocol.Serialize(value.x, _Buffer, ref writtenSize);
				Protocol.Serialize(value.y, _Buffer, ref writtenSize);
				Protocol.Serialize(value.z, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				return writtenSize;
			}
		}

		public static int Serialize(this StreamBuffer outStream, Quaternion value)
		{
			lock (_Buffer)
			{
				var writtenSize = 0;
				Protocol.Serialize(value.x, _Buffer, ref writtenSize);
				Protocol.Serialize(value.y, _Buffer, ref writtenSize);
				Protocol.Serialize(value.z, _Buffer, ref writtenSize);
				Protocol.Serialize(value.w, _Buffer, ref writtenSize);
				outStream.Write(_Buffer, 0, writtenSize);
				return writtenSize;
			}
		}


		public static bool Deserialize(this StreamBuffer inStream, out string value)
		{
			// TODO: OPTIMIZATION: Reduce memory allocations if possible.
			lock (_Buffer)
			{
				var index = 0;
				// Read size of the string
				var readSize = inStream.Read(_Buffer, 0, sizeof(int));
				if (readSize != sizeof(int))
				{
					value = "";
					return false;
				}
				Protocol.Deserialize(out int stringBytesLength, _Buffer, ref index);
				// Read the string bytes
				if (stringBytesLength > 0)
				{
					var stringBytes = new byte[stringBytesLength];
					readSize = inStream.Read(stringBytes, 0, stringBytesLength);
					if (readSize != stringBytesLength)
					{
						value = "";
						return false;
					}
					value = Encoding.UTF8.GetString(stringBytes);
					return true;
				}
				else if (stringBytesLength < 0)
				{
					// Negative length is unexpected, which means the buffer has some faulty data.
					value = "";
					return false;
				}
				else
				{
					value = "";
					return true;
				}
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out int value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(int));
				if (readSize != sizeof(int))
				{
					value = int.MinValue;
					return false;
				}
				Protocol.Deserialize(out value, _Buffer, ref index);
				return true;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out short value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(short));
				if (readSize != sizeof(short))
				{
					value = short.MinValue;
					return false;
				}
				Protocol.Deserialize(out value, _Buffer, ref index);
				return true;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out float value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(float));
				if (readSize != sizeof(float))
				{
					value = float.NaN;
					return false;
				}
				Protocol.Deserialize(out value, _Buffer, ref index);
				return true;
			}
		}

		public static bool DeserializeAsShort(this StreamBuffer inStream, out int value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(short));
				if (readSize != sizeof(short))
				{
					value = int.MinValue;
					return false;
				}
				Protocol.Deserialize(out short valueShort, _Buffer, ref index);
				value = valueShort;
				return true;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out bool value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(short)); // TODO: Photon does not support bool serialization. Find a way to do it properly. See 7817575.
				if (readSize != sizeof(short)) // TODO: Photon does not support bool serialization. Find a way to do it properly. See 7817575.
				{
					value = default(bool);
					return false;
				}
				// TODO: Photon does not support bool serialization. Find a way to do it properly. See 7817575.
				Protocol.Deserialize(out short valueAsShort, _Buffer, ref index);
				value = valueAsShort != 0;
				return true;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out Vector3 value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(float) * 3);
				if (readSize != sizeof(float) * 3)
				{
					value = Vector3Tools.NaN;
					return false;
				}
				Protocol.Deserialize(out float valueX, _Buffer, ref index);
				Protocol.Deserialize(out float valueY, _Buffer, ref index);
				Protocol.Deserialize(out float valueZ, _Buffer, ref index);
				value = new Vector3(valueX, valueY, valueZ);
				return true;
			}
		}

		public static bool Deserialize(this StreamBuffer inStream, out Quaternion value)
		{
			lock (_Buffer)
			{
				var index = 0;
				var readSize = inStream.Read(_Buffer, 0, sizeof(float) * 4);
				if (readSize != sizeof(float) * 4)
				{
					value = QuaternionTools.NaN;
					return false;
				}
				Protocol.Deserialize(out float valueX, _Buffer, ref index);
				Protocol.Deserialize(out float valueY, _Buffer, ref index);
				Protocol.Deserialize(out float valueZ, _Buffer, ref index);
				Protocol.Deserialize(out float valueW, _Buffer, ref index);
				value = new Quaternion(valueX, valueY, valueZ, valueW);
				return true;
			}
		}

		#endregion
	}

}
