using System;
using UnityEngine;
using System.Collections;

[Serializable]
public struct Target
{
	public Transform Transform;
	public Vector3 Position;

	public void RefreshPosition()
	{
		if (Transform != null)
		{
			Position = Transform.position;
		}
	}
}
