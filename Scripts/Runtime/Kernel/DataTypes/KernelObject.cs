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

		[JsonProperty(PropertyName = "ID")]
		protected internal ID _ID;
		[JsonIgnore]
		public ID ID
		{
			get => _ID;
		}

		internal void ResetIDOnDestroy()
		{
			_ID = ID.Invalid;
		}

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

		public void SetID(ID id)
		{
			if (_ID.IsValid)
			{
				throw new Exception($"Tried to set the ID to '{id}' of an already initialized object '{ToTypeAndIDString()}'.");
			}

			var existingInstance = Kernel.Get(id);
			if (existingInstance != null)
			{
				throw new Exception($"Tried to set the ID to '{id}' of the object '{ToTypeAndIDString()}' while there was an already registered object '{existingInstance.ToTypeAndIDStringSafe()}'.");
			}

			_ID = id;
			Kernel.Register(this);
		}

		#endregion

		#region Kernel Link

		[JsonIgnore]
		public TKernel Kernel
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => KernelBase<TKernel>.Instance;
		}

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
