using Extenity.GameObjectToolbox;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	[ExecuteInEditMode]
	public class SnapToGroundInEditor : MonoBehaviour
	{
		public LayerMask GroundLayerMask;
		public float Offset = 0f;

		public const float RaycastDistance = 30f;
		public const int RaycastSteps = 20;

		private void Update()
		{
			if (Application.isPlaying)
			{
				Debug.LogWarningFormat("Destroying SnapToGroundInEditor in object '{0}' which should already be removed by now.", gameObject.FullName());
				Destroy(this);
			}
			else
			{
				transform.SnapToGround(RaycastDistance, RaycastSteps, GroundLayerMask, Offset);
			}
		}
	}

}
