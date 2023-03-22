using System;
using UnityEngine;
using UnityEngine.Events;
#if PLAYMAKER
using HutongGames.PlayMaker.Ecosystem.Utils;
using HutongGames.PlayMaker.Actions;
#endif

namespace Extenity.UIToolbox
{

	public class DetectMouseHover : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
			if (RectTransform == null)
				RectTransform = GetComponent<RectTransform>();
		}

		#endregion

		#region Update

		public bool IsInside { get; private set; }

		protected void Update()
		{
			if (RectTransform == null)
				return;

			var wasInside = IsInside;
			IsInside = ApplicationHasFocus && RectTransformUtility.RectangleContainsScreenPoint(RectTransform, Input.mousePosition);
			if (IsInside)
			{
				if (!wasInside)
				{
					OnHoverEnter.Invoke();

#if PLAYMAKER
					GetLastPointerDataInfo.lastPointeEventData = null;
					onEnterEvent.SendEvent(PlayMakerUGuiSceneProxy.fsm, eventTarget);
#endif
				}
				else
				{
					OnHoverStay.Invoke();
				}
			}
			else
			{
				if (wasInside)
				{
					OnHoverExit.Invoke();

#if PLAYMAKER
					GetLastPointerDataInfo.lastPointeEventData = null;
					onExitEvent.SendEvent(PlayMakerUGuiSceneProxy.fsm, eventTarget);
#endif
				}
			}
		}

		#endregion

		#region Application Focus

		protected bool ApplicationHasFocus { get; private set; }

		protected void OnApplicationFocus(bool hasFocus)
		{
			ApplicationHasFocus = hasFocus;
		}

		#endregion

		#region RectTransform

		[Tooltip("RectTransform to be used in mouse hover detection. Current game object's RectTransform will be automatically assigned if not specified manually.")]
		public RectTransform RectTransform;

		#endregion

		#region Events

		[NonSerialized]
		public UnityEvent OnHoverEnter;
		[NonSerialized]
		public UnityEvent OnHoverStay;
		[NonSerialized]
		public UnityEvent OnHoverExit;

		#endregion

		#region PlayMaker

#if PLAYMAKER

		[Header("PlayMaker")]
		public PlayMakerEventTarget eventTarget;

		[EventTargetVariable("eventTarget")]
		//[ShowOptions]
		public PlayMakerEvent onEnterEvent = new PlayMakerEvent("HOVER / ON MOUSE ENTER");

		[EventTargetVariable("eventTarget")]
		//[ShowOptions]
		public PlayMakerEvent onExitEvent = new PlayMakerEvent("HOVER / ON MOUSE EXIT");

#endif

		#endregion
	}

}
