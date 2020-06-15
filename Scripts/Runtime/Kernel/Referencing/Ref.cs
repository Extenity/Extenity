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
	public struct Ref<TKernelObject, TKernel> : IComparable<Ref<TKernelObject, TKernel>>, IEquatable<Ref<TKernelObject, TKernel>>
		where TKernelObject : KernelObject<TKernel>
		where TKernel : KernelBase<TKernel>
	{
		#region Initialization

		public static readonly Ref<TKernelObject, TKernel>[] EmptyArray = new Ref<TKernelObject, TKernel>[0];

		public Ref(UInt32 id)
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
		internal UInt32 Value;

		// Value 0 is defined as Invalid. Note that all validity checks should treat only being 0 is considered being
		// Invalid. Treating greater than 0 as Valid or negative values as Invalid breaks simple comparisons like
		// "Something.ID == ID.Invalid". See 116451215.
		public static readonly Ref<TKernelObject, TKernel> Invalid = default;

		#endregion

		#region Implicit Conversion Between Ref and UInt32

		public static implicit operator Ref<TKernelObject, TKernel>(UInt32 me)
		{
			return new Ref<TKernelObject, TKernel>(me);
		}
		public static implicit operator UInt32(Ref<TKernelObject, TKernel> me)
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

		// public bool Equals(ID other) { return Value == other.Value; }
		public bool Equals(Ref<TKernelObject, TKernel> other) { return Value == other.Value; }
		public bool Equals(UInt32 other) { return Value == other; }

		public override bool Equals(object obj)
		{
			return obj is Ref<TKernelObject, TKernel> castRef && Value == castRef.Value;
		}

		// public static bool operator ==(Ref lhs, Ref rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(Ref lhs, Ref rhs) { return lhs.Value != rhs.Value; }
		// public static bool operator ==(Ref lhs, ID rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(Ref lhs, ID rhs) { return lhs.Value != rhs.Value; }
		// public static bool operator ==(ID lhs, Ref rhs) { return lhs.Value == rhs.Value; }
		// public static bool operator !=(ID lhs, Ref rhs) { return lhs.Value != rhs.Value; }
		public static bool operator ==(Ref<TKernelObject, TKernel> lhs, UInt32 rhs) { return lhs.Value == rhs; }
		public static bool operator !=(Ref<TKernelObject, TKernel> lhs, UInt32 rhs) { return lhs.Value != rhs; }
		// public static bool operator ==(UInt32 lhs, Ref rhs) { return lhs == rhs.Value; }
		// public static bool operator !=(UInt32 lhs, Ref rhs) { return lhs != rhs.Value; }

		// public int CompareTo(ID other) { return Value.CompareTo(other.Value); }
		public int CompareTo(Ref<TKernelObject, TKernel> other) { return Value.CompareTo(other.Value); }
		public int CompareTo(UInt32 other) { return Value.CompareTo(other); }

		#endregion

		#region Hash

		public override int GetHashCode()
		{
			return unchecked((int)Value);
		}

		#endregion

		#region String Conversion

		public override string ToString()
		{
			return ToHexAndTypeString();
		}

		public string ToHexString()
		{
			return Value.ToString("X");
		}

		public string ToHexAndTypeString()
		{
			return Value.ToString("X") + "(" + typeof(TKernelObject).Name + ")";
		}

		public static Ref<TKernelObject, TKernel> FromHexString(string text)
		{
			return new Ref<TKernelObject, TKernel>(Convert.ToUInt32(text, 16));
		}

		#endregion
	}

}
