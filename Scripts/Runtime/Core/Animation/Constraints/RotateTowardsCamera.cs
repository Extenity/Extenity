using UnityEngine;

namespace Extenity.AnimationToolbox
{

	public class RotateTowardsCamera : MonoBehaviour
	{
		public Transform RotatedObject;
		public Transform CameraTransform;

		public bool EnableSmoothing = true;
		public float SmoothingSpeed = 1f;

		protected void LateUpdate()
		{
			if (CameraTransform == null)
			{
				var mainCamera = Camera.main;
				if (!mainCamera)
					return;
				CameraTransform = mainCamera.transform;
			}

			if (EnableSmoothing)
			{
				var targetRotation = Quaternion.LookRotation(RotatedObject.position - CameraTransform.position, Vector3.up);
				RotatedObject.rotation = Quaternion.Slerp(RotatedObject.rotation, targetRotation, SmoothingSpeed * Time.deltaTime);
			}
			else
			{
				RotatedObject.LookAt(CameraTransform);
			}
		}

#if UNITY_EDITOR

		protected void OnValidate()
		{
			if (!RotatedObject)
			{
				RotatedObject = transform;
			}
		}

#endif
	}

}
