using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	public abstract class KernelHandler<TKernel> : MonoBehaviour
		where TKernel : KernelBase<TKernel>
	{
		#region Initialization

		protected virtual void Awake()
		{
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroy()
		{
			if (Kernel.IsInitialized)
			{
				Kernel.Deinitialize();
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
		public TKernel Kernel
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => KernelBase<TKernel>.Instance;
		}

		#endregion

		#region Stats

		[Title("Stats")]
		[ShowInInspector, HideLabel, NotNull]
		public Versioning.VersioningStats Stats
		{
			get => Kernel.Versioning.Stats;
			set => Kernel.Versioning.Stats = value;
		}

		#endregion
	}

}
