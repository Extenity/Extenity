#if ExtenityKernel

using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	// TODO STATIC CODE ANALYSIS: Ensure Ref struct size is 4 bytes.
	// TODO OPTIMIZATION: Make sure aggressive inlining works in all methods.

	[InlineProperty, HideLabel]
	[Serializable, JsonObject(MemberSerialization.Fields)]
	public struct Ref<TKernelObject, TKernel> : IComparable<Ref<TKernelObject, TKernel>>, IEquatable<Ref<TKernelObject, TKernel>>
		where TKernelObject : KernelObject<TKernel>
		where TKernel : KernelBase<TKernel>
	{
		#region Initialization

		public static readonly Ref<TKernelObject, TKernel>[] EmptyArray = Array.Empty<Ref<TKernelObject, TKernel>>();

		public Ref(UInt32 id)
		{
			_ReferencedID = id;
		}

		#endregion

		#region Data

		[SerializeField, JsonProperty(PropertyName = "ID")]
		[HideInInspector]
		private UInt32 _ReferencedID;

		public UInt32 ReferencedID => _ReferencedID;

		public bool IsSet => _ReferencedID != 0; // See 116451215.
		public bool IsNotSet => _ReferencedID == 0; // See 116451215.

		// Value 0 is defined as Invalid. Note that all validity checks should treat only being 0 is considered being
		// Invalid. Treating greater than 0 as Valid or negative values as Invalid breaks simple comparisons like
		// "Something.ID == ID.Invalid". See 116451215.
		public static readonly Ref<TKernelObject, TKernel> Invalid = default;

		#endregion

		#region Conversion between Ref and UInt32

		// public static implicit operator Ref<TKernelObject, TKernel>(UInt32 me) Decided not to do that. Treating an integer value as Ref is unsafe and might cause unintended conversions. Refs should be created in controlled environments.
		// {
		// 	return new Ref<TKernelObject, TKernel>(me);
		// }

		// See 113543345.
		public static implicit operator Ref<TKernelObject, TKernel>([NotNull] TKernelObject me)
		{
			if (me == null)
				throw new ArgumentNullException(nameof(me));
			return new Ref<TKernelObject, TKernel>(me.ID);
		}

		public static implicit operator UInt32(Ref<TKernelObject, TKernel> me)
		{
			return me._ReferencedID;
		}

		#endregion

		#region Conversion between Ref and Referenced Object

		public static implicit operator TKernelObject(Ref<TKernelObject, TKernel> me)
		{
			return KernelBase<TKernel>.Instance.Get(me);
		}

		[ShowInInspector, HideLabel, Indent]
		[FoldoutGroup("TheGroup", GroupName = "@($property.Parent.Name).ToString() + \"           \" + ReferencedID")]
		public TKernelObject Data => KernelBase<TKernel>.Instance.Get(this);
		public bool IsSetAndDataExists => KernelBase<TKernel>.Instance.Exists(this);

		#endregion

		#region Equality and Comparison

		public bool Equals(Ref<TKernelObject, TKernel> other) { return _ReferencedID == other._ReferencedID; }
		public bool Equals(UInt32 other) { return _ReferencedID == other; }

		public override bool Equals(object obj)
		{
			return obj is Ref<TKernelObject, TKernel> castRef && _ReferencedID == castRef._ReferencedID;
		}

		public static bool operator ==(Ref<TKernelObject, TKernel> lhs, UInt32 rhs) { return lhs._ReferencedID == rhs; }
		public static bool operator !=(Ref<TKernelObject, TKernel> lhs, UInt32 rhs) { return lhs._ReferencedID != rhs; }

		public int CompareTo(Ref<TKernelObject, TKernel> other) { return _ReferencedID.CompareTo(other._ReferencedID); }
		public int CompareTo(UInt32 other) { return _ReferencedID.CompareTo(other); }

		#endregion

		#region Hash

		public override int GetHashCode()
		{
			return unchecked((int)_ReferencedID);
		}

		#endregion

		#region String Conversion

		public override string ToString()
		{
			return ToHexAndTypeString();
		}

		public string ToHexString()
		{
			return _ReferencedID.ToString("X");
		}

		public string ToHexAndTypeString()
		{
			return _ReferencedID.ToString("X") + "(" + typeof(TKernelObject).Name + ")";
		}

		public static Ref<TKernelObject, TKernel> FromHexString(string text)
		{
			return new Ref<TKernelObject, TKernel>(Convert.ToUInt32(text, 16));
		}

		#endregion
	}

}

#endif
