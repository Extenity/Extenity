using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.TouchInput
{

	public class StickSchemeElement : SchemeElementBase
	{
		[BoxGroup("Placement")]
		public RectTransform DefaultLocation;

		[BoxGroup("Placement")]
		public bool EnableFreeRoam = false;

		[BoxGroup("Device Leaning")]
		public bool UseDeviceLeaning = false;

		[BoxGroup("Device Leaning")]
		public float DeviceLeaningSensitivity = 1.25f;

		#region Activation

		protected override void OnActivationChange(bool active)
		{
			if (DefaultLocation)
				DefaultLocation.gameObject.SetActive(active);
		}

		#endregion
	}

}
