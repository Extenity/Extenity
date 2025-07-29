#if UNITY_5_3_OR_NEWER

using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.CameraToolbox
{

	[ExecuteAlways]
	public class CameraHorizontalFOV : MonoBehaviour
	{
		[Required]
		public Camera Camera;

		[OnValueChanged(nameof(Invalidate))]
		public float HorizontalFOV = 45f;

		private float LastAspectRatio = -1f;

		protected void LateUpdate()
		{
			if (!Camera)
				return;

			// Method 1: This is just an estimation and drastically fails in large aspect ratios.
			// Camera.fieldOfView = HorizontalFOV / ((float)Camera.pixelWidth / Camera.pixelHeight);

			// Method 2: This is precise.
			var currentAspectRatio = Camera.aspect;
			var isChanged = LastAspectRatio != currentAspectRatio;
#if UNITY_EDITOR // Don't try to optimize in Editor edit time.
			if (!Application.isPlaying)
			{
				isChanged = true;
			}
#endif
			if (isChanged)
			{
				LastAspectRatio = currentAspectRatio;

				var horizontalFOVRad = HorizontalFOV * Mathf.Deg2Rad;
				var camH = Mathf.Tan(horizontalFOVRad * 0.5f) / currentAspectRatio;
				var verticalFOVRad = Mathf.Atan(camH) * 2f;
				Camera.fieldOfView = verticalFOVRad * Mathf.Rad2Deg;
			}
		}

		public void Invalidate()
		{
			LastAspectRatio = -1f;
		}
	}

}

#endif
