using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.Kernel
{

	// TODO STATIC CODE ANALYSIS: Ensure Ref struct size is 4 bytes.
	// TODO OPTIMIZATION: Make sure aggressive inlining works in all methods.

	[InlineProperty]
	[Serializable, JsonObject(MemberSerialization.Fields)]
	public struct Ref : IComparable<Ref>, IEquatable<Ref>
	{
		#region Initialization

		public static readonly Ref[] EmptyArray = new Ref[0];

		/// <summary>
		/// Creates a Ref with specified ID and Ownership information. Note that the highest bit of 'id' integer value
		/// is used for Ownership flag. So ownership part of 'id' will be ignored and will be overridden by 'isOwner'.
		/// </summary>
		public Ref(int id, bool isOwner)
		{
			// Make sure the ownership information in 'id' does not get into the resulting Value, whether it is
			// set or not. The 'isOwner' information should override what was given in id.
			id = ClearOwnershipBit(id);

			if (isOwner)
			{
				id |= 1 << 31;
			}

			Value = id;
		}

		/// <summary>
		/// For internal use only.
		/// </summary>
		private Ref(int value)
		{
			Value = value;
		}

		#endregion

		#region Data

		/// <summary>
		/// CAUTION! Use it as readonly.
		/// </summary>
		[HideLabel]
		[SerializeField]
		private int Value;

		public static readonly Ref Invalid = default;

		#endregion

		#region ID / Implicit Conversion To ID

		public int ID
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ClearOwnershipBit(Value);
		}

		public static implicit operator int(Ref me)
		{
			return me.ID;
		}

		#endregion

		#region Copy Reference

		public Ref Reference => new Ref(ID, false);

		#endregion

		#region Ownership

		public bool IsOwner => IsOwnerBitSet(Value);

		#endregion

		#region Validation

		public bool IsValid => ID > 0;

		#endregion

		#region Equality and Comparison

		/// <summary>
		/// Checks if two Refs are equal without the Ownership information.
		/// </summary>
		public bool Equals(Ref other)
		{
			return ID == other.ID;
		}

		/// <summary>
		/// Checks if two Refs are equal without the Ownership information.
		/// </summary>
		public override bool Equals(object obj)
		{
			return obj is Ref other && Equals(other);
		}

		/// <summary>
		/// Checks if two Refs are equal without the Ownership information.
		/// </summary>
		public static bool operator ==(Ref lhs, Ref rhs)
		{
			return lhs.ID == rhs.ID;
		}

		/// <summary>
		/// Checks if two Refs are not equal without the Ownership information.
		/// </summary>
		public static bool operator !=(Ref lhs, Ref rhs)
		{
			return lhs.ID != rhs.ID;
		}

		/// <summary>
		/// Checks if two Refs are equal without the Ownership information.
		/// </summary>
		public static bool operator ==(Ref lhs, int rhs)
		{
			return lhs.ID == rhs;
		}

		/// <summary>
		/// Checks if two Refs are equal without the Ownership information.
		/// </summary>
		public static bool operator !=(Ref lhs, int rhs)
		{
			return lhs.ID != rhs;
		}

		/// <summary>
		/// Checks if two Refs are equal without the Ownership information.
		/// </summary>
		public static bool operator ==(int lhs, Ref rhs)
		{
			return lhs == rhs.ID;
		}

		/// <summary>
		/// Checks if two Refs are not equal without the Ownership information.
		/// </summary>
		public static bool operator !=(int lhs, Ref rhs)
		{
			return lhs != rhs.ID;
		}

		/// <summary>
		/// Compares two Refs without the Ownership information.
		/// </summary>
		public int CompareTo(Ref other)
		{
			return ID.CompareTo(other.ID);
		}

		#endregion

		#region Hash

		public override int GetHashCode()
		{
			return Value;
		}

		#endregion

		#region Tools

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int ClearOwnershipBit(int value)
		{
			return value & 0x7FFFFFFF;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static bool IsOwnerBitSet(int value)
		{
			return (value & (1 << 31)) != 0;
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

	#region Extensions

	public static class RefTools
	{
		public static Ref[] ToReferenceArray(this Ref[] refs)
		{
			var references = new Ref[refs.Length];
			for (int i = 0; i < refs.Length; i++)
			{
				references[i] = refs[i].Reference;
			}
			return references;
		}

		public static void ToReferenceArray(this List<Ref> refs, List<Ref> targetList)
		{
			for (int i = 0; i < refs.Count; i++)
			{
				targetList.Add(refs[i].Reference);
			}
		}
	}

	#endregion

}
