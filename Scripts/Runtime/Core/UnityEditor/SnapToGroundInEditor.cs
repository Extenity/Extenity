#if UNITY

using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	[ExecuteInEditMode]
	public class SnapToGroundInEditor : MonoBehaviour
	{
		[Header("Ground")]
		public LayerMask GroundLayerMask;
		public float GroundOffset = 0f;

		[Header("Rotation")]
		public SnapToGroundRotationOption Rotation = SnapToGroundRotationOption.DontRotate;
		public float RotationCastDistanceX = 1f;
		public float RotationCastDistanceY = 1f;

		[Header("Snap Raycast Details")]
		public const float RaycastDistance = 50f;
		public const int RaycastSteps = 20;

		private void Update()
		{
			if (Application.isPlaying)
			{
				Log.With(nameof(SnapToGroundInEditor)).Warning($"Destroying {nameof(SnapToGroundInEditor)} in object '{gameObject.FullName()}' which should already be removed by now.");
				Destroy(this);
			}
			else
			{
				transform.SnapToGround(RaycastDistance, RaycastSteps, GroundLayerMask, GroundOffset, Rotation, RotationCastDistanceX, RotationCastDistanceY);
			}
		}
	}

}

#endif
