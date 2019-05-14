using System;
using Extenity.FlowToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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

		#region Camera

		[SerializeField]
		private Camera _Camera;
		public Camera Camera
		{
			get
			{
				if (!_Camera)
				{
					var camerasInChildren = transform.GetComponentsInChildren<Camera>(false);
					if (camerasInChildren.Length == 1)
					{
						_Camera = camerasInChildren[0];
					}
					else
					{
						Log.Error($"There should be a single camera in '{typeof(CameraController).Name}' object or its children.", this);
					}
				}
				return _Camera;
			}
		}

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

		// Exact copy of UITools.IsGUIActiveInCurrentEventSystem because we can't reach Extenity UI DLL from here. See 11637281.
		protected static bool IsGUIActiveInCurrentEventSystem
		{
			get
			{
				var eventSystem = EventSystem.current;
				if (eventSystem)
				{
					if (eventSystem.currentSelectedGameObject != null)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion
	}

}
