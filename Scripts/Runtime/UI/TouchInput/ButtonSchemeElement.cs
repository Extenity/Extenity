using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.TouchInput
{

	public class ButtonSchemeElement : SchemeElementBase
	{
		[BoxGroup("Placement")]
		public RectTransform DefaultLocation;

		[BoxGroup("Placement")]
		public bool EnableFreeRoam = false;

		#region Activation

		protected override void OnActivationChange(bool active)
		{
			if (DefaultLocation)
				DefaultLocation.gameObject.SetActive(active);
		}

		#endregion
	}

}
