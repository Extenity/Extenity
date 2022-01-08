#if UNITY

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

		public static float ForwardSpeed(this Rigidbody rigidbody)
		{
			return Vector3.Dot(rigidbody.transform.forward, rigidbody.velocity);
		}

		public static float HorizontalForwardSpeed(this Rigidbody rigidbody)
		{
			var forward = rigidbody.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			return Vector3.Dot(forward, rigidbody.velocity);
		}
	}

}

#endif
