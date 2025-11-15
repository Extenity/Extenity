#if UNITY_5_3_OR_NEWER

using Extenity.FlowToolbox;
using UnityEngine;

namespace Extenity.AnimationToolbox
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
					Loop.RegisterFixedUpdate(CalculateLocalCoordinates);
				}
				else
				{
					Loop.RegisterFixedUpdate(CalculateWorldCoordinates);
				}
			}
			else
			{
				if (InLocalCoordinates)
				{
					Loop.RegisterUpdate(CalculateLocalCoordinates);
				}
				else
				{
					Loop.RegisterUpdate(CalculateWorldCoordinates);
				}
			}
		}

		private void OnDisable()
		{
			if (InFixedUpdate)
			{
				if (InLocalCoordinates)
				{
					Loop.DeregisterFixedUpdate(CalculateLocalCoordinates);
				}
				else
				{
					Loop.DeregisterFixedUpdate(CalculateWorldCoordinates);
				}
			}
			else
			{
				if (InLocalCoordinates)
				{
					Loop.DeregisterUpdate(CalculateLocalCoordinates);
				}
				else
				{
					Loop.DeregisterUpdate(CalculateWorldCoordinates);
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

#endif
