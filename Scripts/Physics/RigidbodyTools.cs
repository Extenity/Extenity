using UnityEngine;
using Extenity.Logging;
using System.Collections;
using System.Collections.Generic;

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
}
