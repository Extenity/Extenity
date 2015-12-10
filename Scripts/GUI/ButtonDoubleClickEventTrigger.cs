using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonDoubleClickEventTrigger : MonoBehaviour
{
	#region Configuration

	public static readonly bool RunInRealTime = true;
	public static readonly float TimeIntervalToDetectSecondClick = 0.5f;

	#endregion

	#region Initialization

	private void Start()
	{
		Button.onClick.AddListener(InternalOnButtonClick);
	}

	#endregion

	#region Calculations

	private float LastClickTime = -1000f;

	private void InternalOnButtonClick()
	{
		var now = RunInRealTime
			? Time.realtimeSinceStartup
			: Time.time;

		if (LastClickTime + TimeIntervalToDetectSecondClick > now)
		{
			onDoubleClick.Invoke();
			LastClickTime = -1000f;
		}
		else
		{
			LastClickTime = now;
		}
	}

	#endregion

	#region Button

	private Button _Button;
	public Button Button
	{
		get
		{
			if (_Button == null)
			{
				_Button = GetComponent<Button>();
			}
			return _Button;
		}
	}

	#endregion

	#region DoubleClick Event

	[FormerlySerializedAs("onDoubleClick")]
	[SerializeField]
	private ButtonDoubleClickedEvent m_OnDoubleClick = new ButtonDoubleClickedEvent();

	public ButtonDoubleClickedEvent onDoubleClick
	{
		get { return this.m_OnDoubleClick; }
		set { this.m_OnDoubleClick = value; }
	}

	[Serializable]
	public class ButtonDoubleClickedEvent : UnityEvent
	{
	}

	#endregion
}
