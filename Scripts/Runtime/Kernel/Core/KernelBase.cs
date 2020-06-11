using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	public class KernelBase
	{
		#region TEMP Singleton

		// TODO: This is a temporary solution until a reference resolver is introduced in Kernel deserialization. DataLink and SyncList keeps a reference to Kernel, but others may too. Figure out a generalized way to assign these references on deserialization.
		public static KernelBase _TempInstance;

		public KernelBase()
		{
			_TempInstance = this;
		}

		#endregion

		#region Versioning

		[NonSerialized] // Versioning is only used for callbacks and does not keep any data.
		public readonly Versioning Versioning = new Versioning();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Invalidate(ID id)
		{
			Versioning.Invalidate(id);
		}

		#endregion

		#region ID Generator

		[SerializeField]
		public IDGenerator IDGenerator = new IDGenerator();

		#endregion
	}

}
