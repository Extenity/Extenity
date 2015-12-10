using UnityEngine;
using System.Collections;

public static class CameraTools
{
	public static Vector3? WorldToScreenPointWithReverseCheck(this Camera camera, Vector3 position)
	{
		if (Vector3.Dot(camera.transform.forward, position - camera.transform.position) > 0)
		{
			return camera.WorldToScreenPoint(position);
		}
		return null;
	}
}
