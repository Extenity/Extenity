using Extenity.FlowToolbox;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class DontKeepFocusOnClick : MonoBehaviour
	{
		protected void Start()
		{
			var selectable = GetComponent<Selectable>();
			if (selectable == null)
			{
				Log.Error($"{nameof(DontKeepFocusOnClick)} needs a {nameof(Selectable)} component to work");
				return;
			}

			// -------------------------------------------------------------------------
			// Register to click event
			// -------------------------------------------------------------------------
			var button = selectable as Button;
			if (button != null)
			{
				button.onClick.AddListener(LoseFocus);
				return;
			}
			var toggle = selectable as Toggle;
			if (toggle != null)
			{
				toggle.onValueChanged.AddListener(LoseFocus);
				return;
			}
			// -------------------------------------------------------------------------
			// -------------------------------------------------------------------------

			Log.Fatal($"{nameof(DontKeepFocusOnClick)} is not implemented for type {selectable.GetType().Name}");
		}

		protected void OnDestroy()
		{
			var selectable = GetComponent<Selectable>();
			if (selectable != null)
			{
				// -------------------------------------------------------------------------
				// Deregister from click event
				// -------------------------------------------------------------------------
				var button = selectable as Button;
				if (button != null)
				{
					button.onClick.RemoveListener(LoseFocus);
					return;
				}
				var toggle = selectable as Toggle;
				if (toggle != null)
				{
					toggle.onValueChanged.RemoveListener(LoseFocus);
					return;
				}
				// -------------------------------------------------------------------------
				// -------------------------------------------------------------------------
			}
		}

		private void LoseFocus(bool dummy)
		{
			this.FastInvoke(LoseFocusDelayed, 0, true);
		}

		private void LoseFocus()
		{
			this.FastInvoke(LoseFocusDelayed, 0, true);
		}

		private void LoseFocusDelayed()
		{
			// Delayed call is needed because immediately setting selected object to null
			// prevents calling other methods registered to button click event.
			// But now we need to check if the currently selected object is still this object.
			if (EventSystem.current.currentSelectedGameObject == gameObject)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}

		#region Log

		private static readonly Logger Log = new(nameof(DontKeepFocusOnClick));

		#endregion
	}

}
