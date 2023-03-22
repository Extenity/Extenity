using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if PLAYMAKER
using HutongGames.PlayMaker.Ecosystem.Utils;
using HutongGames.PlayMaker.Actions;
#endif

namespace Extenity.UIToolbox
{

	public class DetectMouseHoverGroup : MonoBehaviour
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

		#endregion

		#region Update

		public bool IsInside { get; private set; }

		protected void Update()
		{
			var wasInside = IsInside;

			IsInside = IsMouseInsideAnyDetector;
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

		#region Detectors

		public List<DetectMouseHover> Detectors;

		public bool IsMouseInsideAnyDetector
		{
			get
			{
				if (Detectors == null)
					return false;
				for (int i = 0; i < Detectors.Count; i++)
				{
					if (Detectors[i].IsInside)
						return true;
				}
				return false;
			}
		}

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
