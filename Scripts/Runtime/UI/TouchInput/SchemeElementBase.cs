using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.TouchInput
{

	[HideMonoScript]
	public abstract class SchemeElementBase : MonoBehaviour
	{
		[BoxGroup("Layout")]
		[InfoBox("The name of the Scheme that this Element will be activated for.")]
		public string SchemeName = "Default";

		[BoxGroup("Layout")]
		[InfoBox("The area that the Element detects clicks. This area is generally defined larger than the render area to allow easier clicks. Also this area is where the Element can roam freely if Free Ream is enabled.")]
		public RectTransform ClickableArea = default;

		#region Activation

		protected abstract void OnActivationChange(bool active);

		internal void ChangeActivation(bool active)
		{
			if (ClickableArea)
				ClickableArea.gameObject.SetActive(active);

			OnActivationChange(active);
		}

		#endregion
	}

}
