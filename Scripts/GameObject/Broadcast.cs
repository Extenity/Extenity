using UnityEngine;

// TODO: Not heavily tested yet
public class Broadcast
{
	public static void ToScene(string methodName)
	{
		var objects = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		if (objects == null)
			return;

		for (int i = 0; i < objects.Length; i++)
		{
			var obj = objects[i];
			if (obj != null && obj.transform.parent == null)
				obj.BroadcastMessage(methodName, SendMessageOptions.DontRequireReceiver);
		}
	}

	public static void ToScene(string methodName, object value)
	{
		var objects = Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
		if (objects == null)
			return;

		for (int i = 0; i < objects.Length; i++)
		{
			var obj = objects[i];
			if (obj != null && obj.transform.parent == null)
				obj.BroadcastMessage(methodName, value, SendMessageOptions.DontRequireReceiver);
		}
	}
}
