using System;
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
				throw new NotImplementedException("DontKeepFocusOnClick needs a Selectable component to work");
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

			throw new NotImplementedException("DontKeepFocusOnClick is not implemented for type " + selectable.GetType().Name);
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

		private void LoseFocus(bool arg1)
		{
			LoseFocus();
		}

		private void LoseFocus()
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

}
