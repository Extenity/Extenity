using System;
using UnityEngine;

public static class AnimationTools
{
	/// <summary>
	/// TODO: Not tested yet
	/// </summary>
	public static AnimationCurve Clone(this AnimationCurve me)
	{
		var newKeys = new Keyframe[me.keys.Length];
		for (int i = 0; i < me.keys.Length; i++)
		{
			newKeys[i] = me.keys[i];
		}

		var newCurve = new AnimationCurve(newKeys);
		newCurve.postWrapMode = me.postWrapMode;
		newCurve.preWrapMode = me.preWrapMode;
		return newCurve;
	}

	/// <summary>
	/// TODO: Not tested yet
	/// </summary>
	public static bool IsSame(this AnimationCurve me, AnimationCurve other)
	{
		if (ReferenceEquals(me, other))
			return true;

		if (me.keys.Length != other.keys.Length ||
			me.preWrapMode != other.preWrapMode ||
			me.postWrapMode != other.postWrapMode)
			return false;


		for (int i = 0; i < me.keys.Length; i++)
		{
			//var myKey = me.keys[i];
			//var otherKey = other.keys[i];

			//if (myKey.inTangent != otherKey.inTangent ||
			//	myKey.outTangent != otherKey.outTangent ||
			//	myKey.tangentMode != otherKey.tangentMode ||
			//	myKey.time != otherKey.time ||
			//	myKey.value != otherKey.value)
			//	return false;

			if (!me.keys[i].Equals(other.keys[i]))
				return false;
		}
		return true;
	}

	public static bool IsAvailable(this Animation me)
	{
		return me != null && me.clip != null;
	}
}
