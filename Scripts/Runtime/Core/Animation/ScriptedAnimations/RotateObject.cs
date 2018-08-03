using UnityEngine;

namespace Extenity.AnimationToolbox.ScriptedAnimations
{

	public class RotateObject : MonoBehaviour
	{
		public float RotationSpeed = 1f;
		public Vector3 RotationAxis = Vector3.up;

		public bool InLocalCoordinates = true;
		public bool InFixedUpdate = true;

		private float PreviousFrameRealtime;

		private void OnEnable()
		{
			PreviousFrameRealtime = Time.realtimeSinceStartup;
		}

		private void FixedUpdate()
		{
			if (!InFixedUpdate)
				return;

			Rotate(RotationSpeed * Time.fixedDeltaTime);
		}

		private void Update()
		{
			if (InFixedUpdate)
				return;

			var now = Time.realtimeSinceStartup;
			var deltaRealtime = now - PreviousFrameRealtime;
			PreviousFrameRealtime = now;
			Rotate(RotationSpeed * deltaRealtime);
		}

		private void Rotate(float angle)
		{
			if (InLocalCoordinates)
			{
				transform.Rotate(RotationAxis, angle, Space.Self);
			}
			else
			{
				transform.Rotate(RotationAxis, angle);
			}
		}
	}

}
