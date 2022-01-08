#if UNITY

using System;
using Extenity.MathToolbox;
using Extenity.PhysicsToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.AnimationToolbox
{

	public enum ObjectDraggingType
	{
		HoverInFrontOfCamera,
		SnapWithRaycast,
	}

	public class DraggableObject : MonoBehaviour
	{
		#region Initialization

		protected void OnEnable()
		{
			SmoothFollowTargetPosition = Object.position;
		}

		#endregion

		#region Update

		protected void LateUpdate()
		{
			if (!Object)
				return;
			CalculateDraggingPosition();
			UpdateSmoothFollow();
		}

		#endregion

		#region Dragging

		[Title("Dragging")]
		public Transform Object;
		public ObjectDraggingType DraggingType = ObjectDraggingType.HoverInFrontOfCamera;

		[ShowIf("@DraggingType == ObjectDraggingType." + nameof(ObjectDraggingType.HoverInFrontOfCamera))]
		[Range(0f, 10f)]
		public float DistanceFromCamera = 3f;

		[ShowIf("@DraggingType == ObjectDraggingType." + nameof(ObjectDraggingType.SnapWithRaycast))]
		[Range(0f, 10f)]
		public float DistanceFromHitPoint = 3f;
		[ShowIf("@DraggingType == ObjectDraggingType." + nameof(ObjectDraggingType.SnapWithRaycast))]
		public float RaycastMaxDistance = 0f;
		[ShowIf("@DraggingType == ObjectDraggingType." + nameof(ObjectDraggingType.SnapWithRaycast))]
		public LayerMask RaycastLayerMask = ~0;

		private void CalculateDraggingPosition()
		{
			var theCamera = Camera.main;
			if (theCamera == null)
				return; // Skip positioning if the camera is not available right now.
			var mousePosition = Input.mousePosition;

			switch (DraggingType)
			{
				case ObjectDraggingType.HoverInFrontOfCamera:
				{
					var screenPosition = mousePosition.WithZ(DistanceFromCamera);
					var position = theCamera.ScreenToWorldPoint(screenPosition);
					SetSmoothFollowTargetPosition(position);
					break;
				}

				case ObjectDraggingType.SnapWithRaycast:
				{
					var ray = theCamera.ScreenPointToRay(mousePosition);
					var maxDistance = RaycastMaxDistance <= 0.0001f ? float.MaxValue : RaycastMaxDistance;
					if (PhysicsTools.Raycast(ray, out var hitInfo, maxDistance, RaycastLayerMask, Object))
					{
						var distance = hitInfo.distance - DistanceFromHitPoint;
						var hitPoint = ray.GetPoint(distance);
						SetSmoothFollowTargetPosition(hitPoint);
					}
					break;
				}

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Smooth Follow

		[Title("Smooth Movement")]
		public float MovementSmoothingFactor = 20f;

		[Tooltip("If the movement distance exceeds this threshold distance, the object will be directly teleported to the target without smoothing.")]
		public float TeleportThresholdDistance = 10;

		private Vector3 SmoothFollowTargetPosition;

		private void SetSmoothFollowTargetPosition(Vector3 targetPosition)
		{
			SmoothFollowTargetPosition = targetPosition;
		}

		private void UpdateSmoothFollow()
		{
			var position = Object.position;
			var difference = SmoothFollowTargetPosition - position;

			if (difference.sqrMagnitude > TeleportThresholdDistance * TeleportThresholdDistance)
			{
				Object.position = SmoothFollowTargetPosition;
			}
			else
			{
				Object.position = position + difference * Mathf.Clamp01(MovementSmoothingFactor * Time.deltaTime);
			}
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		protected void OnValidate()
		{
			if (!Object)
			{
				Object = transform;
			}
		}

#endif

		#endregion
	}

}

#endif
