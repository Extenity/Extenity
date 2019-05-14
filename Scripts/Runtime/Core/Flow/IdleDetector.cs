using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.FlowToolbox
{

	public class IdleDetector : MonoBehaviour
	{
		[Header("Idle")]
		public float RequiredTimeToGoIntoIdle = 5f;
		[ShowInInspector, ReadOnly]
		public bool IsIdle { get; private set; }

		[Tooltip("Switch this on to enable idle detection feature. Switching it off allows minimizing the overhead of idle detection.")]
		public bool IdleDetectionEnabled = false;

		private bool IsIdleDetectionInvoked;

		public class IdleEvent : UnityEvent<bool> { }
		[NonSerialized]
		public readonly IdleEvent OnIdleChanged = new IdleEvent();

		public void BreakIdle()
		{
			InternalChangeIdle(false);

			if (IdleDetectionEnabled && RequiredTimeToGoIntoIdle > 0.001f)
			{
				this.FastInvoke(GoIntoIdle, RequiredTimeToGoIntoIdle, true, true);
				IsIdleDetectionInvoked = true;
			}
			else if (IsIdleDetectionInvoked)
			{
				this.CancelFastInvoke(GoIntoIdle);
				IsIdleDetectionInvoked = false;
			}
		}

		private void GoIntoIdle()
		{
			this.CancelFastInvoke(GoIntoIdle);
			IsIdleDetectionInvoked = false;
			InternalChangeIdle(true);
		}

		private void InternalChangeIdle(bool value)
		{
			if (IsIdle != value)
			{
				IsIdle = value;
				OnIdleChanged.Invoke(IsIdle);
				//Log.Info($"Camera idle state changed to '{IsIdle}'.");
			}
		}
	}

}
