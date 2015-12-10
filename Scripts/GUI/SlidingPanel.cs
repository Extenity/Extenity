using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlidingPanel : MonoBehaviour
{
	#region Initialize

	private void Start()
	{
		InitializeEventEmittingControls();
	}

	#endregion

	#region Update

	private void Update()
	{
		UpdateShowHide();
	}

	#endregion

	#region Links

	public RectTransform SlidingPanelTransform;

	#endregion

	#region Events

	public UnityEvent OnShow = new UnityEvent();
	public UnityEvent OnHide = new UnityEvent();

	#endregion

	#region Show/Hide

	private enum PanelAnimationState
	{
		Unspecified,
		Showing,
		Shown,
		Hiding,
		Hidden,
	}

	public bool IsVisible { get; private set; }
	public bool IsPlayingAnimation { get; private set; }
	public float ShiftAnimationDuration = 0.3f;
	public float HideDelay = 0.5f;
	public RectTransform[] SteadyCalculationTransforms;
	public Vector2 AdditionalSteady;
	public Vector2 SteadyDirection = Vector2.one;
	public RectTransform[] ShiftCalculationTransforms;
	public Vector2 AdditionalShift;
	public Vector2 ShiftDirection = Vector2.one;
	public bool InvertPositions = false;
	private PanelAnimationState State = PanelAnimationState.Unspecified;
	private float HideTriggerTime = 0f;

	private Vector2 Steady
	{
		get
		{
			var total = AdditionalSteady;
			if (SteadyCalculationTransforms != null)
			{
				for (int i = 0; i < SteadyCalculationTransforms.Length; i++)
				{
					total += Vector2.Scale(SteadyDirection, SteadyCalculationTransforms[i].sizeDelta);
				}
			}
			return total;
		}
	}

	private Vector2 Shift
	{
		get
		{
			var total = AdditionalShift;
			if (ShiftCalculationTransforms != null)
			{
				for (int i = 0; i < ShiftCalculationTransforms.Length; i++)
				{
					total += Vector2.Scale(ShiftDirection, ShiftCalculationTransforms[i].sizeDelta);
				}
			}
			return total;
		}
	}

	private Vector3 _Position1
	{
		get { return Shift; }
	}
	private Vector3 _Position2
	{
		get { return Steady; }
	}

	public Vector3 Position1
	{
		get
		{
			if (InvertPositions)
				return _Position2;
			else
				return _Position1;
		}
	}
	public Vector3 Position2
	{
		get
		{
			if (InvertPositions)
				return _Position1;
			else
				return _Position2;
		}
	}

	private void UpdateShowHide()
	{
		if (!IsPlayingAnimation)
		{
			if (IsVisible)
			{
				if (State != PanelAnimationState.Shown)
				{
					DoShow();
				}
			}
			else
			{
				if (State != PanelAnimationState.Hidden && Time.realtimeSinceStartup > HideTriggerTime + HideDelay)
				{
					DoHide();
				}
			}
		}
	}

	public void Show()
	{
		IsVisible = true;
		HideTriggerTime = 0f;
	}

	public void Hide()
	{
		IsVisible = false;

		if (HideTriggerTime <= 0f)
		{
			HideTriggerTime = Time.realtimeSinceStartup;
		}
	}

	private void DoShow()
	{
		OnShow.Invoke();

		SlidingPanelTransform.DOLocalMove(Position1, ShiftAnimationDuration).OnComplete(OnShowAnimationCompleted);
		IsPlayingAnimation = true;
	}

	private void DoHide()
	{
		OnHide.Invoke();

		SlidingPanelTransform.DOLocalMove(Position2, ShiftAnimationDuration).OnComplete(OnHideAnimationCompleted);
		IsPlayingAnimation = true;
		HideTriggerTime = 0f;
	}

	private void OnHideAnimationCompleted()
	{
		State = PanelAnimationState.Hidden;
		IsPlayingAnimation = false;
	}

	private void OnShowAnimationCompleted()
	{
		State = PanelAnimationState.Shown;
		IsPlayingAnimation = false;
	}

	#endregion

	#region Show/Hide Event Emitting Controls

	public List<RectTransform> EventEmittingControls;

	private void InitializeEventEmittingControls()
	{
		if (EventEmittingControls == null)
			return;

		for (int i = 0; i < EventEmittingControls.Count; i++)
		{
			var control = EventEmittingControls[i];
			var eventTrigger = control.gameObject.GetFirstOrAddComponent<EventTrigger>();

			CreateEvent(eventData => Show(), EventTriggerType.PointerEnter, eventTrigger);
			CreateEvent(eventData => Show(), EventTriggerType.PointerClick, eventTrigger);
			CreateEvent(eventData => Hide(), EventTriggerType.PointerExit, eventTrigger);
		}
	}

	private static void CreateEvent(UnityAction<BaseEventData> action, EventTriggerType eventType, EventTrigger eventTrigger)
	{
		var trigger = new EventTrigger.TriggerEvent();
		trigger.AddListener(action);

		var entry = new EventTrigger.Entry
		{
			eventID = eventType,
			callback = trigger,
		};

		if (eventTrigger.triggers == null)
		{
			eventTrigger.triggers = new List<EventTrigger.Entry>();
		}
		eventTrigger.triggers.Add(entry);
	}

	#endregion
}
