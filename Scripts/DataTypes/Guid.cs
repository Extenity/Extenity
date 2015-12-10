using System;
using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;

namespace Extenity.DataTypes
{

	[Serializable]
	public struct Guid : IFormattable, IComparable, IComparable<Guid>, IEquatable<Guid>
	{
		public byte[] Data;

		#region Initialization

		public Guid(Guid g)
		{
			Data = g.Data.Clone() as byte[];
		}

		public Guid(byte[] b)
		{
			Data = b.Clone() as byte[];
		}

		public Guid(string g)
		{
			Data = new System.Guid(g).ToByteArray();
		}

		public Guid(System.Guid g)
		{
			Data = g.ToByteArray();
		}

		#endregion

		#region Empty

		public static readonly Guid Empty = new Guid();

		public bool IsEmpty
		{
			get
			{
				if (Data == null || Data.Length == 0)
					return true;

				if (Data.Length != 16)
					return true;

				for (int i = 0; i < 16; i++)
				{
					if (Data[i] != 0)
						return false;
				}

				return true;
			}
		}

		#endregion

		#region Create Random

		public static Guid NewGuid()
		{
			return new Guid(System.Guid.NewGuid());
		}

		#endregion

		#region Convert To System.Guid

		public System.Guid ToSystemGuid()
		{
			return new System.Guid(Data);
		}

		#endregion

		#region Equality

		public static bool operator ==(Guid a, Guid b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Guid a, Guid b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Guid))
				return false;

			var castObj = (Guid)obj;
			return Equals(castObj);
		}

		public bool Equals(Guid other)
		{
			if (IsEmpty)
			{
				if (other.IsEmpty)
					return true;
				return false;
			}
			else if (other.IsEmpty)
			{
				return false;
			}

			CheckDataLength();
			other.CheckDataLength();

			for (int i = 0; i < 16; i++)
			{
				if (Data[i] != other.Data[i])
					return false;
			}

			return true;
		}

		#endregion

		#region Comparison

		public int CompareTo(object obj)
		{
			if (obj == null || !(obj is Guid))
				return 1;

			var castObj = (Guid)obj;
			return CompareTo(castObj);
		}

		public int CompareTo(Guid other)
		{
			if (IsEmpty)
			{
				if (other.IsEmpty)
					return 0;
			}
			else if (other.IsEmpty)
			{
				return 1;
			}

			CheckDataLength();
			other.CheckDataLength();

			for (int i = 0; i < 16; i++)
			{
				var result = Data[i].CompareTo(other.Data[i]);
				if (result != 0)
					return result;
			}

			return 0;
		}

		#endregion

		#region GetHashCode

		public override int GetHashCode()
		{
			if (Data == null || Data.Length != 16)
				return 0;
			return ToSystemGuid().GetHashCode();
		}

		#endregion

		#region Consistency

		private void CheckDataLength()
		{
			if (Data.Length != 16)
				throw new Exception("Guid data length '" + Data.Length + "' is invalid.");
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			if (IsEmpty)
				return System.Guid.Empty.ToString();
			return ToSystemGuid().ToString();
		}

		public String ToString(String format)
		{
			return ToString(format, null);
		}

		public String ToString(String format, IFormatProvider formatProvider)
		{
			if (IsEmpty)
				return System.Guid.Empty.ToString(format, formatProvider);
			return ToSystemGuid().ToString(format, formatProvider);
		}

		#endregion
	}

	public static class ExtenityGuidExtentions
	{
		#region Conversion

		public static Guid ToExtenityGuid(this System.Guid value)
		{
			return new Guid(value);
		}

		#endregion
	}

}
