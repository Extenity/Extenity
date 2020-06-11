using System;
using System.Runtime.CompilerServices;

namespace Extenity.KernelToolbox
{

	public class KernelBase
	{
		#region TEMP Singleton

		// TODO IMMEDIATE: This is a temporary solution to cover up until a reference resolver is introduced in Kernel deserialization. DataLink and SyncList keeps a reference to Kernel, but others may too. Figure out a generalized way to assign these references on deserialization.
		public static KernelBase _TempInstance;

		public KernelBase()
		{
			_TempInstance = this;
		}

		#endregion

		#region Versioning

		[NonSerialized]
		public readonly Versioning Versioning = new Versioning();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate(ID id)
		{
			Versioning.Invalidate(id);
		}

		#endregion
	}

}
