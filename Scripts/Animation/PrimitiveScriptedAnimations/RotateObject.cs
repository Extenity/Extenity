using UnityEngine;
using System.Collections;

public class RotateObject : MonoBehaviour
{
	public float rotationSpeed = 1f;
	public Vector3 rotationAxis = Vector3.up;

	public bool inLocalCoordinates = true;
	public bool inFixedUpdate = true;

	private float previousFrameRealtime;

	private void OnEnable()
	{
		previousFrameRealtime = Time.realtimeSinceStartup;
	}

	private void FixedUpdate()
	{
		if (!inFixedUpdate)
			return;

		Rotate(rotationSpeed * Time.fixedDeltaTime);
	}

	private void Update()
	{
		if (inFixedUpdate)
			return;

		var now = Time.realtimeSinceStartup;
		var deltaRealtime = now - previousFrameRealtime;
		previousFrameRealtime = now;
		Rotate(rotationSpeed * deltaRealtime);
	}

	private void Rotate(float angle)
	{
		if (inLocalCoordinates)
		{
			transform.Rotate(rotationAxis, angle, Space.Self);
		}
		else
		{
			transform.Rotate(rotationAxis, angle);
		}
	}
}
