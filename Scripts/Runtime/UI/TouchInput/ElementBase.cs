using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.TouchInput
{

	[HideMonoScript]
	public abstract class ElementBase<TSchemeElement> : MonoBehaviour
		where TSchemeElement : SchemeElementBase
	{
		#region Initialization

		protected virtual void Start()
		{
			InitializeSchemeElements();
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroy()
		{
			DeinitializeSchemeElements();
		}

		#endregion

		#region Links

		[BoxGroup("Links"), PropertyOrder(8)]
		public RectTransform Base = default;

		#endregion

		#region Scheme Elements

		protected abstract void ApplySchemeElement(TSchemeElement schemeElement);

		public TSchemeElement ActiveSchemeElement { get; private set; }

		[BoxGroup("Links"), PropertyOrder(20)]
		[ListDrawerSettings(Expanded = true)]
		public TSchemeElement[] SchemeElements;

		private void InitializeSchemeElements()
		{
			TouchInputManager.OnInputSchemeChanged.AddListener(SetupForInputScheme);
			if (TouchInputManager.IsInstanceAvailable)
			{
				SetupForInputScheme(TouchInputManager.Instance.CurrentScheme);
			}
		}

		private void DeinitializeSchemeElements()
		{
			if (TouchInputManager.IsInstanceAvailable)
			{
				TouchInputManager.OnInputSchemeChanged.RemoveListener(SetupForInputScheme);
			}
		}

		private void SetupForInputScheme(string scheme)
		{
			if (SchemeElements == null || SchemeElements.Length == 0)
				return;

			// 'Null' scheme will be applied if none found with the specified name.
			ActiveSchemeElement = null;

			foreach (var schemeElement in SchemeElements)
			{
				if (schemeElement.SchemeName.Equals(scheme, StringComparison.OrdinalIgnoreCase))
				{
					ActiveSchemeElement = schemeElement;
					// schemeElement.ChangeActivation(true); Nope! This will be done after every other one is disabled. See below.
				}
				else
				{
					schemeElement.ChangeActivation(false);
				}
			}

			if (ActiveSchemeElement != null)
			{
				ActiveSchemeElement.ChangeActivation(true);
			}

			// Log.Info(ActiveSchemeElement != null
			// 	         ? $"Changing scheme of element '{gameObject.FullName()}' to '{ActiveSchemeElement.gameObject.FullName()}'"
			// 	         : $"Resetting scheme of element '{gameObject.FullName()}'.");
			ApplySchemeElement(ActiveSchemeElement);
		}

		#endregion

		#region Show / Hide UI

		[BoxGroup("Links"), PropertyOrder(15)]
		public UIFader Fader;

		#endregion
	}

}
