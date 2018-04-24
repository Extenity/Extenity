using UnityEngine;

namespace Extenity.MathToolbox
{

	public static class QuaternionTools
	{
		public static readonly Quaternion NaN = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);

		#region Basic Checks

		public static bool IsAnyNaN(this Quaternion value)
		{
			return float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z) || float.IsNaN(value.w);
		}

		#endregion

		#region Rotation

		public static Vector3 EulerAnglesInNeg180Pos180(this Quaternion quaternion)
		{
			Vector3 angles = quaternion.eulerAngles;
			if (angles.x > 180f) angles.x -= 360f;
			if (angles.y > 180f) angles.y -= 360f;
			if (angles.z > 180f) angles.z -= 360f;
			return angles;
		}

		/// <summary>
		/// Rotates a rotation from towards to. Same as Quaternion.RotateTowards except this one notifies about rotation completion via isCompleted.
		/// </summary>
		public static Quaternion RotateTowards(this Quaternion from, Quaternion to, float maxDegreesDelta, out bool isCompleted)
		{
			var totalAngles = Quaternion.Angle(from, to);
			if (totalAngles.IsZero())
			{
				isCompleted = true;
				return to;
			}
			var t = maxDegreesDelta / totalAngles;
			if (t > 1f)
			{
				isCompleted = true;
				return to;
			}
			isCompleted = false;
			return Quaternion.SlerpUnclamped(from, to, t);
		}

		#endregion
	}

}
