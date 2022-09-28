#if ExtenityKernel

using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	public abstract class KernelHandler<TKernel> : MonoBehaviour
		where TKernel : KernelBase<TKernel>
	{
		#region Deinitialization

		protected virtual void OnDestroy()
		{
			if (Kernel.IsActive)
			{
				Kernel.Deactivate();
			}
		}

		#endregion

		#region Update

		// Note that this Update should be called earlier than any View behaviours in execution order.
		protected virtual void Update()
		{
			Kernel.Versioning.EmitEventsInQueue();
		}

		#endregion

		#region Kernel Link

		[JsonIgnore]
		[FoldoutGroup("Kernel")]
		[ShowInInspector, HideLabel]
		public TKernel Kernel
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => KernelBase<TKernel>.Instance;
		}

		#endregion

		#region Stats

		[JsonIgnore]
		[FoldoutGroup("Stats")]
		[ShowInInspector, HideLabel]
		public Versioning.VersioningStats Stats
		{
			get => Kernel?.Versioning?.Stats;
			set => Kernel.Versioning.Stats = value;
		}

		#endregion
	}

}

#endif
