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

		public ID ID;

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
