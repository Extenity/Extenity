using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.PhysicsToolbox
{

	public static class RigidbodyTools
	{
		public static void ResetVelocity(this Rigidbody rigidbody)
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
		}

		public static Vector3 LocalAngularVelocity(this Rigidbody rigidbody)
		{
			return Quaternion.Inverse(rigidbody.rotation) * rigidbody.angularVelocity;
		}

		public static float HorizontalSpeed(this Rigidbody rigidbody)
		{
			var velocity = rigidbody.velocity;
			return Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
		}
	}

}
