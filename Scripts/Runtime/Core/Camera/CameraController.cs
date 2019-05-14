using System;
using Extenity.FlowToolbox;
using Extenity.UnityEditorToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.CameraToolbox
{

	public abstract class CameraController : MonoBehaviour
	{
		#region Initialization

		protected virtual void OnEnable()
		{
			BreakIdle(); // This is here to initialize idle calculations.
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Camera

		public Camera Camera;

		#endregion

		#region User Controls

		public abstract bool IsAxisActive { get; set; }
		public abstract bool IsMouseActive { get; set; }

		#endregion

		#region Input

		[Tooltip("This helps fixing huge deltas when starting to drag the camera, happening on mobile WebGL.")]
		public bool IgnoreInitialDragOnClick = true;
		protected bool IsDragging;

		#endregion

		#region Idle

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

		#endregion

		#region GUI

		//private EventSystem CachedEventSystem;

		//public bool IsGUIActive
		//{
		//	get
		//	{
		//		if (CachedEventSystem == null)
		//		{
		//			CachedEventSystem = FindObjectOfType<EventSystem>();
		//		}
		//		if (CachedEventSystem == null)
		//		{
		//			Log.Error($"Scene should have an EventSystem for camera controller on game object '{gameObject.name}' to work.", this);
		//			return false;
		//		}
		//		return CachedEventSystem.currentSelectedGameObject != null;
		//	}
		//}

		#endregion
	}

}
