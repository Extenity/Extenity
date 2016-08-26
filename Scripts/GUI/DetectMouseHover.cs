using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UserInterface
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
			IsInside = RectTransformUtility.RectangleContainsScreenPoint(RectTransform, Input.mousePosition);
			if (IsInside)
			{
				if (!wasInside)
				{
					Log("---- Mouse enter");
					OnMouseEnter.Invoke();
				}
				else
				{
					Log("---- Mouse stay");
					OnMouseStay.Invoke();
				}
			}
			else
			{
				if (wasInside)
				{
					Log("---- Mouse exit");
					OnMouseExit.Invoke();
				}
			}
		}

		#endregion

		#region RectTransform

		[Tooltip("RectTransform to be used in mouse hover detection. Current game object's RectTransform will be automatically assigned if not specified manually.")]
		public RectTransform RectTransform;

		#endregion

		#region Events

		public UnityEvent OnMouseEnter;
		public UnityEvent OnMouseStay;
		public UnityEvent OnMouseExit;

		#endregion

		#region Debug

		public bool WriteDebugLogs = false;

		private void Log(string message)
		{
			if (WriteDebugLogs)
			{
				Debug.Log(message);
			}
		}

		#endregion
	}

}
