using UnityEngine;
using Extenity.Logging;
using System.Collections;

namespace Extenity.InputManagement
{

	public class MouseCursor : SingletonUnity<MouseCursor>
	{
		#region Initialization

		protected void Awake()
		{
			InitializeSingleton(this, true);
		}

		#endregion

		#region Configuration

		public bool LockMouseWhenHidden = true;
		public bool UnlockMouseWhenShown = true;

		#endregion

		#region Update

		protected void Update()
		{
			if (IsHidden)
			{
				if (LockMouseWhenHidden)
				{
					Cursor.lockState = CursorLockMode.Locked;
				}
				Cursor.visible = false;
			}
			else
			{
				if (UnlockMouseWhenShown)
				{
					Cursor.lockState = CursorLockMode.None;
				}
				Cursor.visible = true;
			}
		}

		#endregion

		#region Mouse Lock and Hide

		public static bool IsHidden { get; private set; }

		public static void ToggleCursor()
		{
			if (IsHidden)
			{
				ShowCursor();
			}
			else
			{
				HideCursor();
			}
		}

		public static void HideCursor()
		{
			if (IsHidden)
				return;
			IsHidden = true;

			//Instance.CancelInternalInvokes();
			//Instance.Invoke("InternalDelayedHide", 0.1f);
		}

		public static void ShowCursor()
		{
			if (!IsHidden)
				return;
			IsHidden = false;

			//Instance.CancelInternalInvokes();
			//Instance.Invoke("InternalDelayedShow", 0.1f);
		}

		//private void InternalDelayedShow()
		//{
		//	CancelInternalInvokes();

		//	if (UnlockMouseWhenShown)
		//	{
		//		Cursor.lockState = CursorLockMode.None;
		//	}
		//	Cursor.visible = true;
		//}

		//private void InternalDelayedHide()
		//{
		//	CancelInternalInvokes();

		//	if (LockMouseWhenHidden)
		//	{
		//		Cursor.lockState = CursorLockMode.Locked;
		//	}
		//	Cursor.visible = false;
		//}

		//private void CancelInternalInvokes()
		//{
		//	CancelInvoke("InternalDelayedShow");
		//	CancelInvoke("InternalDelayedHide");
		//}

		#endregion
	}

}
