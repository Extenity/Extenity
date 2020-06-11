using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	// TODO STATIC CODE ANALYSIS: Ensure Ref struct size is 4 bytes.
	// TODO OPTIMIZATION: Make sure aggressive inlining works in all methods.

	[InlineProperty]
	[Serializable, JsonObject(MemberSerialization.Fields)]
	public struct Ref : IComparable<Ref>, IEquatable<Ref>
	{
		#region Initialization

		public static readonly Ref[] EmptyArray = new Ref[0];

		public Ref(int id)
		{
			Value = id;
		}

		#endregion

		#region Data

		/// <summary>
		/// CAUTION! Use it as readonly.
		/// </summary>
		[HideLabel]
		[SerializeField]
		internal int Value;

		public static readonly Ref Invalid = default;

		#endregion

		#region Implicit Conversion To ID and int

		public static implicit operator ID(Ref me)
		{
			return new ID(me.Value);
		}

		public static implicit operator int(Ref me)
		{
			return me.Value;
		}

		#endregion

		#region Validation

		public bool IsValid => Value > 0;

		#endregion

		#region Equality and Comparison

		// public bool Equals(ID other) { return Value == other.Value; }
		public bool Equals(Ref other) { return Value == other.Value; }
		public bool Equals(int other) { return Value == other; }

		public override bool Equals(object obj)
		{
			return (obj is Ref castRef && Value == castRef.Value) ||
			       (obj is ID castID && Value == castID.Value);
		}

		// public static bool operator ==(Ref lhs, Ref rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(Ref lhs, Ref rhs) { return lhs.Value != rhs.Value; }
		// public static bool operator ==(Ref lhs, ID rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(Ref lhs, ID rhs) { return lhs.Value != rhs.Value; }
		// public static bool operator ==(ID lhs, Ref rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(ID lhs, Ref rhs) { return lhs.Value != rhs.Value; }
		public static bool operator ==(Ref lhs, int rhs) { return lhs.Value == rhs; }
		public static bool operator !=(Ref lhs, int rhs) { return lhs.Value != rhs; }
		// public static bool operator ==(int lhs, Ref rhs) { return lhs == rhs.Value; }
		// public static bool operator !=(int lhs, Ref rhs) { return lhs != rhs.Value; }

		// public int CompareTo(ID other) { return Value.CompareTo(other.Value); }
		public int CompareTo(Ref other) { return Value.CompareTo(other.Value); }
		public int CompareTo(int other) { return Value.CompareTo(other); }

		#endregion

		#region Hash

		public override int GetHashCode()
		{
			return Value;
		}

		#endregion

		#region String Conversion

		public override string ToString()
		{
			return ToHexString();
		}

		public string ToHexString()
		{
			return Value.ToString("X");
		}

		public static Ref FromHexString(string text)
		{
			return new Ref(Convert.ToInt32(text, 16));
		}

		#endregion
	}

}
