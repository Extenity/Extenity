/* The old implementation of Mouse Cursor that was necessary in ancient Unity versions. Nowadays, it should be examined.
using UnityEngine;
using Extenity.DesignPatternsToolbox;

namespace Extenity.InputToolbox
{

	public class MouseCursor : SingletonUnity<MouseCursor>
	{
		#region Initialization

		protected void Awake()
		{
			InitializeSingleton(true);
		}

		#endregion

		#region Configuration

		public bool LockMouseWhenHidden = true;
		public bool UnlockMouseWhenShown = true;

		#endregion

		#region Update

		protected void Update()
		{
			if (Changed)
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
				Changed = false;
			}
		}

		#endregion

		#region Mouse Lock and Hide

		public bool IsHidden { get; private set; }
		private bool Changed;

		public void ToggleCursor()
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

		public void HideCursor()
		{
			if (IsHidden)
				return;
			IsHidden = true;
			Changed = true;

			//Instance.CancelInternalInvokes();
			//Instance.Invoke("InternalDelayedHide", 0.1f);
		}

		public void ShowCursor()
		{
			if (!IsHidden)
				return;
			IsHidden = false;
			Changed = true;

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
*/
