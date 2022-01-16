#if UNITY

using System;
using Extenity.MathToolbox;
using Extenity.MeshToolbox;
using Unity.Mathematics;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public static class GizmosTools
	{
		#region Path

		public static void DrawPathLines(
			Func<int, float3> pointGetter, int pointCount, bool loop,
			Color lineColor)
		{
			DrawPath(pointGetter, pointCount, loop, true, lineColor, false, default(Color), float.NaN, float.NaN);
		}

		public static void DrawPath(
			Func<int, float3> pointGetter, int pointCount, bool loop,
			bool drawLines, Color lineColor,
			bool drawPoints, Color pointColor,
			float pointSize, float firstPointSizeFactor = 1f)
		{
			if (pointCount == 0)
				return;

			// Draw lines
			if (drawLines)
			{
				Gizmos.color = lineColor;

				var previousPoint = pointGetter(0);

				for (int i = 1; i < pointCount; i++)
				{
					var currentPoint = pointGetter(i);
					Gizmos.DrawLine(previousPoint, currentPoint);
					previousPoint = currentPoint;
				}
				if (loop)
				{
					Gizmos.DrawLine(previousPoint, pointGetter(0));
				}
			}

			// Draw points
			if (drawPoints)
			{
				Gizmos.color = pointColor;

				var size3 = new Vector3(pointSize * firstPointSizeFactor, pointSize * firstPointSizeFactor, pointSize * firstPointSizeFactor);

				Gizmos.DrawWireCube(pointGetter(0), size3);

				size3.Set(pointSize, pointSize, pointSize);

				for (int i = 1; i < pointCount; i++)
				{
					var currentPoint = pointGetter(i);
					Gizmos.DrawWireCube(currentPoint, size3);
				}
			}
		}

		#endregion

		#region DrawCylinder

		public static void DrawCylinder(Vector3 start, Vector3 end, float radius)
		{
			Gizmos.DrawMesh(MeshTools.UnitCylinderZ,
				start,
				Quaternion.LookRotation(end - start, Vector3.up),
				new Vector3(radius * 2f, radius * 2f, end.DistanceTo(start)));
		}

		public static void DrawCylinder(Vector3 position, Quaternion rotation, float radius, float length)
		{
			Gizmos.DrawMesh(MeshTools.UnitCylinderZ,
				position,
				rotation * Quaternion.Euler(90f, 0f, 0f),
				new Vector3(radius * 2f, radius * 2f, length));
		}

		#endregion
	}

}

#endif
