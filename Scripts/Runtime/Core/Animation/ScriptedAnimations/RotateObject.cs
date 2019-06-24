using UnityEngine;

namespace Extenity.AnimationToolbox.ScriptedAnimations
{

	public class RotateObject : MonoBehaviour
	{
		public float RotationSpeed = 1f;
		public Vector3 RotationAxis = Vector3.up;

		public bool InLocalCoordinates = true;
		public bool InFixedUpdate = true;

		private void OnEnable()
		{
			if (InFixedUpdate)
			{
				if (InLocalCoordinates)
				{
					Loop.FixedUpdateCallbacks.AddListener(CalculateLocalCoordinates);
				}
				else
				{
					Loop.FixedUpdateCallbacks.AddListener(CalculateWorldCoordinates);
				}
			}
			else
			{
				if (InLocalCoordinates)
				{
					Loop.UpdateCallbacks.AddListener(CalculateLocalCoordinates);
				}
				else
				{
					Loop.UpdateCallbacks.AddListener(CalculateWorldCoordinates);
				}
			}
		}

		private void OnDisable()
		{
			if (InFixedUpdate)
			{
				if (InLocalCoordinates)
				{
					Loop.FixedUpdateCallbacks.RemoveListener(CalculateLocalCoordinates);
				}
				else
				{
					Loop.FixedUpdateCallbacks.RemoveListener(CalculateWorldCoordinates);
				}
			}
			else
			{
				if (InLocalCoordinates)
				{
					Loop.UpdateCallbacks.RemoveListener(CalculateLocalCoordinates);
				}
				else
				{
					Loop.UpdateCallbacks.RemoveListener(CalculateWorldCoordinates);
				}
			}
		}

		private void CalculateWorldCoordinates()
		{
			transform.Rotate(RotationAxis, RotationSpeed * Loop.DeltaTime, Space.World);
		}

		private void CalculateLocalCoordinates()
		{
			transform.Rotate(RotationAxis, RotationSpeed * Loop.DeltaTime, Space.Self);
		}
	}

}
