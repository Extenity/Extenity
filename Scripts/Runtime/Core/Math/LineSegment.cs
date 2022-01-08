#if UNITY // TODO-UniversalExtenity: Convert these to Mathematics after importing it into Universal project.

using UnityEngine;

namespace Extenity.MathToolbox
{

	public struct LineSegment
	{
		public Vector3 p1;
		public Vector3 p2;

		public LineSegment(Vector3 p1, Vector3 p2)
		{
			this.p1 = p1;
			this.p2 = p2;
		}

		public Vector3 Direction
		{
			get { return (p2 - p1).normalized; }
		}
	}

}

#endif
