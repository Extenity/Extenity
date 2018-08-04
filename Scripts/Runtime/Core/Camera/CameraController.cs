using System;
using Extenity.FlowToolbox;
using Extenity.UnityEditorToolbox;
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

		#region Idle

		[Header("Idle")]
		public float RequiredTimeToGoIntoIdle = 5f;
		[ReadOnlyInInspector]
		public bool IsIdle;

		public class IdleEvent : UnityEvent<bool> { }
		[NonSerialized]
		public IdleEvent OnIdleChanged = new IdleEvent();

		public void BreakIdle()
		{
			InternalChangeIdle(false);

			this.CancelFastInvoke(GoIntoIdle);
			this.FastInvoke(GoIntoIdle, RequiredTimeToGoIntoIdle, true);
		}

		private void GoIntoIdle()
		{
			this.CancelFastInvoke(GoIntoIdle);
			InternalChangeIdle(true);
		}

		private void InternalChangeIdle(bool value)
		{
			if (IsIdle != value)
			{
				IsIdle = value;
				OnIdleChanged.Invoke(IsIdle);
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
		//			Debug.LogErrorFormat(this, "Scene should have an EventSystem for camera controller on game object '{0}' to work.", gameObject.name);
		//			return false;
		//		}
		//		return CachedEventSystem.currentSelectedGameObject != null;
		//	}
		//}

		#endregion
	}

}
