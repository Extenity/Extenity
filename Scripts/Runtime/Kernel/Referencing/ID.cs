using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	// TODO STATIC CODE ANALYSIS: Ensure Ref struct size is 4 bytes.
	// TODO OPTIMIZATION: Make sure aggressive inlining works in all methods.

	[InlineProperty]
	[Serializable, JsonObject(MemberSerialization.Fields)]
	public struct ID : IComparable<ID>, IEquatable<ID>
	{
		#region Initialization

		public ID(int id)
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

		// Value 0 is defined as Invalid. Note that all validity checks should treat only being 0 is considered being
		// Invalid. Treating greater than 0 as Valid or negative values as Invalid breaks simple comparisons like
		// "Something.ID == ID.Invalid". See 116451215.
		public static readonly ID Invalid = default;

		#endregion

		#region Implicit Conversion To Ref and int

		public static implicit operator Ref(ID me)
		{
			return new Ref(me.Value);
		}

		public static implicit operator int(ID me)
		{
			return me.Value;
		}

		#endregion

		#region Validation

		// See 116451215.
		public bool IsValid => Value != 0;
		public bool IsInvalid => Value == 0;

		#endregion

		#region Equality and Comparison

		public bool Equals(ID other) { return Value == other.Value; }
		// public bool Equals(Ref other) { return Value == other.Value; }
		public bool Equals(int other) { return Value == other; }

		public override bool Equals(object obj)
		{
			return (obj is Ref castRef && Value == castRef.Value) ||
			       (obj is ID castID && Value == castID.Value);
		}

		// public static bool operator ==(ID lhs, ID rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(ID lhs, ID rhs) { return lhs.Value != rhs.Value; }
		// public static bool operator ==(ID lhs, Ref rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(ID lhs, Ref rhs) { return lhs.Value != rhs.Value; }
		// public static bool operator ==(Ref lhs, ID rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(Ref lhs, ID rhs) { return lhs.Value != rhs.Value; }
		public static bool operator ==(ID lhs, int rhs) { return lhs.Value == rhs; }
		public static bool operator !=(ID lhs, int rhs) { return lhs.Value != rhs; }
		// public static bool operator ==(int lhs, ID rhs) { return lhs == rhs.Value; }
		// public static bool operator !=(int lhs, ID rhs) { return lhs != rhs.Value; }

		public int CompareTo(ID other) { return Value.CompareTo(other.Value); }
		// public int CompareTo(Ref other) { return Value.CompareTo(other.Value); }
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

		public static ID FromHexString(string text)
		{
			return new ID(Convert.ToInt32(text, 16));
		}

		#endregion
	}

	#region Extensions

	public static class IDTools
	{
		public static Ref[] ToReferenceArray(this ID[] ids)
		{
			var references = new Ref[ids.Length];
			for (int i = 0; i < ids.Length; i++)
			{
				references[i] = ids[i];
			}
			return references;
		}

		public static void ToReferenceArray(this List<ID> ids, List<Ref> targetList)
		{
			for (int i = 0; i < ids.Count; i++)
			{
				targetList.Add(ids[i]);
			}
		}
	}

	#endregion

}
