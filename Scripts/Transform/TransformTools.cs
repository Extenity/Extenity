using System;
using UnityEngine;
using System.Collections;

public static class TransformTools
{
	#region Center To Children

	public static void CenterToChildren(this Transform me)
	{
		if (me.childCount == 0)
			throw new Exception("Transform has no children");

		var center = me.CenterOfChildren();
		var shift = me.position - center;

		me.position = center;

		foreach (Transform child in me)
		{
			child.localPosition += shift;
		}
	}

	public static Vector3 CenterOfChildren(this Transform me)
	{
		if (me.childCount == 0)
			throw new Exception("Transform has no children");

		var value = Vector3.zero;

		foreach (Transform child in me)
		{
			value += child.position;
		}

		return value / me.childCount;
	}

	#endregion
}
