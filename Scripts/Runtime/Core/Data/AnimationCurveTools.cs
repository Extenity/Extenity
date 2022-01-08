#if UNITY

using UnityEngine;

namespace Extenity.DataToolbox
{

	public static class AnimationCurveTools
	{
		public static bool Clamp01Vertical(this AnimationCurve curve)
		{
			return curve.ClampVertical(0f, 1f);
		}

		public static bool ClampVertical(this AnimationCurve curve, float min, float max)
		{
			if (curve.keys == null || curve.keys.Length == 0)
				return false;

			var changed = false;

			var keys = curve.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				var keyframe = keys[i];
				if (keyframe.value < min)
				{
					keyframe.value = min;
					changed = true;
					keys[i] = keyframe;
				}
				else if (keyframe.value > max)
				{
					keyframe.value = max;
					changed = true;
					keys[i] = keyframe;
				}
			}
			if (changed)
			{
				curve.keys = keys;
			}
			return changed;
		}

		public static bool AbsoluteScaleHorizontal(this AnimationCurve curve, float endValue)
		{
			if (curve.keys == null || curve.keys.Length < 2)
				return false;

			var keys = curve.keys;
			var startTime = keys[0].time;
			var endTime = keys[keys.Length - 1].time;

			if (startTime == 0f && endTime == endValue)
				return false;

			var scale = endValue / (endTime - startTime);
			var oneOverScale = 1f / scale;

			for (int i = 0; i < keys.Length; i++)
			{
				var keyframe = keys[i];
				keyframe.time = (keyframe.time - startTime) * scale;
				keys[i] = keyframe;
				keys[i].inTangent *= oneOverScale;
				keys[i].outTangent *= oneOverScale;
			}
			curve.keys = keys;
			return true;
		}

		public static void ConvertAllPointsToLinear(this AnimationCurve curve)
		{
			var keys = curve.keys;
			for (int i = 0; i < keys.Length; i++)
			{
				var key = keys[i];

				if (i == 0)
				{
					key.inTangent = 0;
				}
				else
				{
					// Consider using this.
					//AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);

					var diffX = key.time - keys[i - 1].time;
					var diffY = key.value - keys[i - 1].value;
					key.inTangent = diffY / diffX;
				}

				if (i == keys.Length - 1)
				{
					key.outTangent = 0;
				}
				else
				{
					var diffX = keys[i + 1].time - key.time;
					var diffY = keys[i + 1].value - key.value;
					key.outTangent = diffY / diffX;
				}

				curve.MoveKey(i, key);
			}
		}
	}

}

#endif
