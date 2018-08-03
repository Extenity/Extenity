using System;
using UnityEngine;

namespace Extenity.MathToolbox
{

	[Serializable]
	public struct PathPoint
	{
		public Vector3 Position;

		/// <summary>
		/// Total distance between this point and next point on path.
		/// </summary>
		public float SegmentLength;

		/// <summary>
		/// Total distance of previous line segments.
		/// </summary>
		public float TotalLengthUntilThisPoint;

		public PathPoint(Vector3 position) : this()
		{
			Position = position;
			SegmentLength = 0f;
			TotalLengthUntilThisPoint = 0f;
		}

		public void ResetLength()
		{
			TotalLengthUntilThisPoint = 0f;
			SegmentLength = 0f;
		}
	}

}
