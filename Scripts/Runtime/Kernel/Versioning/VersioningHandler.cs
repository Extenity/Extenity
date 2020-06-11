using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.KernelToolbox
{

	[HideMonoScript]
	public class VersioningHandler : MonoBehaviour
	{
		// Note that this Update should be called earlier than any View behaviours in execution order.
		private void Update()
		{
			KernelBase._TempInstance.Versioning.EmitEventsInQueue();
		}

		#region Stats

		[Title("Stats")]
		[ShowInInspector, HideLabel, NotNull]
		public Versioning.VersioningStats Stats
		{
			get => KernelBase._TempInstance.Versioning.Stats;
			set => KernelBase._TempInstance.Versioning.Stats = value;
		}

		#endregion
	}

}
