#if ExtenityKernel

using System;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace Extenity.KernelToolbox
{

	// TODO: Find a way to prevent KernelObject to be instantiated via 'new'.

	public abstract class KernelObject
	{
		#region Deinitialization

		protected internal virtual void OnDestroy() { }

		#endregion

		#region ID

		// Value 0 is defined as Invalid. Note that all validity checks should treat only being 0 is considered being
		// Invalid. Treating greater than 0 as Valid or negative values as Invalid breaks simple comparisons like
		// "Something.ID == ID.Invalid". See 116451215.
		public static readonly UInt32 InvalidID = default;

		[JsonProperty(PropertyName = "ID")]
		protected UInt32 _ID;
		[JsonIgnore]
		public UInt32 ID
		{
			get => _ID;
		}

		internal void ResetIDOnDestroy()
		{
			_ID = InvalidID;
		}

		#endregion

		#region Validation

		public bool IsAlive => _ID != 0; // See 116451215.
		public bool IsDestroyed => _ID == 0; // See 116451215.

		#endregion

		#region ToString

		public override string ToString()
		{
			return ToTypeAndIDString();
		}

		public string ToTypeAndIDString()
		{
			return GetType().Name + "|" + ID;
		}

		#endregion
	}

	public abstract class KernelObject<TKernel> : KernelObject
		where TKernel : KernelBase<TKernel>
	{
		#region ID

		internal void SetID(UInt32 id)
		{
			if (IsAlive)
			{
				throw new Exception($"Tried to set the ID to '{id}' of an already initialized object '{ToTypeAndIDString()}'.");
			}

			if (Kernel != null)
			{
				var existingInstance = Kernel.Get(id, GetType());
				if (existingInstance != null)
				{
					throw new Exception($"Tried to set the ID to '{id}' of the object '{ToTypeAndIDString()}' while there was an already registered object '{existingInstance.ToTypeAndIDStringSafe()}'.");
				}
			}

			_ID = id;
			// Kernel.Register(this); This was an idea that any ID assignment is instantly registered in Kernel. It seriously fails with serialization stuff. So rolled back to registering at instantiation.
		}

		// This is being done as Ref implicit conversion. See 113543345.
		// public Ref<KernelObject<TKernel>, TKernel> Ref => new Ref<KernelObject<TKernel>, TKernel>(_ID);

		#endregion

		#region Kernel Link

		[JsonIgnore]
		public TKernel Kernel
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => KernelBase<TKernel>.Instance;
		}

		#endregion

		#region Ref

		// TODO: This is not a good way to approach that problem. Returning an object of type KernelObject<TKernel> is not really useful. The returned type should be Derived KernelObject, but currently there is no way to know that. Figure out a good way, or do not provide this functionality. See 112712367.
		// [JsonIgnore]
		// public Ref<KernelObject<TKernel>, TKernel> Ref => new Ref<KernelObject<TKernel>, TKernel>(ID);

		#endregion

		#region Invalidate

		public void Invalidate()
		{
			Kernel.Invalidate(ID);
		}

		#endregion
	}

	public static class KernelObjectTools
	{
		public static string ToTypeAndIDStringSafe(this KernelObject instance)
		{
			if (instance == null)
				return "[NA]"; // TODO: Remove hardcode
			return instance.ToTypeAndIDString();
		}
	}

}

#endif
