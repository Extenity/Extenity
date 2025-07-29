#if UNITY_5_3_OR_NEWER

using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.PhysicsToolbox
{

	public static class RigidbodyTools
	{
		public static void ResetVelocity(this Rigidbody rigidbody)
		{
			rigidbody.linearVelocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
		}

		public static Vector3 LocalAngularVelocity(this Rigidbody rigidbody)
		{
			return Quaternion.Inverse(rigidbody.rotation) * rigidbody.angularVelocity;
		}

		public static float HorizontalSpeed(this Rigidbody rigidbody)
		{
			var velocity = rigidbody.linearVelocity;
			return sqrt(velocity.x * velocity.x + velocity.z * velocity.z);
		}

		public static float ForwardSpeed(this Rigidbody rigidbody)
		{
			return Vector3.Dot(rigidbody.transform.forward, rigidbody.linearVelocity);
		}

		public static float HorizontalForwardSpeed(this Rigidbody rigidbody)
		{
			var forward = rigidbody.transform.forward;
			forward.y = 0f;
			forward.Normalize();
			return Vector3.Dot(forward, rigidbody.linearVelocity);
		}
	}

}

#endif
