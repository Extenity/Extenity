#if UNITY

using Extenity.DataToolbox;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	[ExecuteInEditMode]
	public class SnapToObjectInEditor : MonoBehaviour
	{
		#region Configuration

		private const float PositionChangeDetectionPrecision = 0.001f;
		private const float RotationChangeDetectionPrecision = 0.001f;

		#endregion

		[Header("Target Object")]
		public Transform TargetObject;

		[Header("Placement")]
		public bool LockPositionX = true;

		public bool LockPositionY = true;
		public bool LockPositionZ = true;
		public bool LockRotationX = true;
		public bool LockRotationY = true;
		public bool LockRotationZ = true;
		public Vector3 PositionOffset;
		public Vector3 RotationOffset;
		public bool UseLocalPositionOfThis = false;
		public bool UseLocalPositionOfTarget = false;
		public bool UseLocalRotationOfThis = false;
		public bool UseLocalRotationOfTarget = false;

		public bool IsAnyPositionLocked => LockPositionX || LockPositionY || LockPositionZ;
		public bool IsAnyRotationLocked => LockRotationX || LockRotationY || LockRotationZ;

		private void Update()
		{
			if (Application.isPlaying)
			{
				Log.With(nameof(SnapToObjectInEditor)).Warning($"Destroying {nameof(SnapToObjectInEditor)} in object '{gameObject.FullName()}' which should already be removed by now.");
				Destroy(this);
			}
			else
			{
				SnapToObject();
			}
		}

		public void SnapToObject()
		{
			if (!TargetObject)
				return;

			var thisObject = transform;

			// Position
			var needsPositionUpdate = false;
			Vector3 thisNewPosition = default;
			if (IsAnyPositionLocked)
			{
				var thisCurrentPosition = UseLocalPositionOfThis ? thisObject.localPosition : thisObject.position;
				thisNewPosition = thisCurrentPosition;
				var targetPosition = UseLocalPositionOfTarget ? TargetObject.localPosition : TargetObject.position;
				if (LockPositionX)
				{
					thisNewPosition.x = targetPosition.x + PositionOffset.x;
				}
				if (LockPositionY)
				{
					thisNewPosition.y = targetPosition.y + PositionOffset.y;
				}
				if (LockPositionZ)
				{
					thisNewPosition.z = targetPosition.z + PositionOffset.z;
				}

				if (!thisNewPosition.IsAlmostEqual(thisCurrentPosition, PositionChangeDetectionPrecision))
				{
					needsPositionUpdate = true;
				}
			}

			// Rotation
			var needsRotationUpdate = false;
			Vector3 thisNewRotation = default;
			if (IsAnyRotationLocked)
			{
				var thisCurrentRotation = UseLocalRotationOfThis ? thisObject.localEulerAngles : thisObject.eulerAngles;
				thisNewRotation = thisCurrentRotation;
				var targetRotation = UseLocalRotationOfTarget ? TargetObject.localEulerAngles : TargetObject.eulerAngles;
				if (LockRotationX)
				{
					thisNewRotation.x = targetRotation.x + RotationOffset.x;
				}
				if (LockRotationY)
				{
					thisNewRotation.y = targetRotation.y + RotationOffset.y;
				}
				if (LockRotationZ)
				{
					thisNewRotation.z = targetRotation.z + RotationOffset.z;
				}

				if (!thisNewRotation.IsAlmostEqual(thisCurrentRotation, RotationChangeDetectionPrecision))
				{
					needsRotationUpdate = true;
				}
			}

			// Update position and rotation if required
			if (needsPositionUpdate || needsRotationUpdate)
			{
#if UNITY_EDITOR
				UnityEditor.Undo.RecordObject(thisObject, $"{nameof(SnapToObjectInEditor)} location update");
#endif
				if (needsPositionUpdate)
				{
					if (UseLocalPositionOfThis)
					{
						thisObject.localPosition = thisNewPosition;
					}
					else
					{
						thisObject.position = thisNewPosition;
					}
				}
				if (needsRotationUpdate)
				{
					if (UseLocalRotationOfThis)
					{
						thisObject.localEulerAngles = thisNewRotation;
					}
					else
					{
						thisObject.eulerAngles = thisNewRotation;
					}
				}
			}
		}
	}

}

#endif
