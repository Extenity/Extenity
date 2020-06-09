using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.Kernel
{

	[HideMonoScript]
	public class VersioningHandler : MonoBehaviour
	{
		// Note that this Update should be called earlier than any View behaviours in execution order.
		private void Update()
		{
			Versioning._TempInstance.EmitEventsInQueue();
		}

		#region Stats

		[Title("Stats")]
		[ShowInInspector, HideLabel, NotNull]
		public Versioning.VersioningStats Stats
		{
			get => Versioning._TempInstance.Stats;
			set => Versioning._TempInstance.Stats = value;
		}

		#endregion
	}

}
