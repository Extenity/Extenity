#if UNITY_5_3_OR_NEWER

using System;
using UnityEngine;

namespace Extenity.GameObjectToolbox
{

	public class FixedTransform : MonoBehaviour
	{
		public enum FixTypes
		{
			FixToMainCamera,
			//FixToTransform,
		}

		public FixTypes FixType;

		public bool FixGlobalPosition = true;
		public bool FixLocalPosition = false;
		public bool FixGlobalRotation = true;
		public bool FixLocalRotation = false;
		public bool FixLocalScale = false;

		private void Update()
		{
			Transform targetTransform = null;

			switch (FixType)
			{
				case FixTypes.FixToMainCamera:
					{
						var mainCamera = Camera.main;
						if (mainCamera == null)
							break;
						targetTransform = mainCamera.transform;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (targetTransform == null)
				return;

			if (FixLocalPosition)
			{
				transform.localPosition = targetTransform.localPosition;
			}
			if (FixGlobalPosition)
			{
				transform.position = targetTransform.position;
			}
			if (FixLocalRotation)
			{
				transform.localRotation = targetTransform.localRotation;
			}
			if (FixGlobalRotation)
			{
				transform.rotation = targetTransform.rotation;
			}
			if (FixLocalScale)
			{
				transform.localScale = targetTransform.localScale;
			}
		}
	}

}

#endif
