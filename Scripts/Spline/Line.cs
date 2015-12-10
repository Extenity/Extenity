using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
	#region Initialization

	public void ClearData()
	{
		ClearPoints();
	}

	#endregion

	#region Configuration

	[Header("Configuration")]
	public bool Loop = false;

	#endregion

	#region Points

	[Header("Data")]
	public List<Vector3> Points;

	public bool IsAnyPointAvailable
	{
		get { return Points != null && Points.Count > 0; }
	}

	private void ClearPoints()
	{
		if (Points == null)
		{
			Points = new List<Vector3>();
		}
		else
		{
			Points.Clear();
		}
	}

	public Vector3 GetPoint(float distanceFromStart, ref Vector3 part)
	{
		return MathTools.GetPointAtDistanceFromStart(Points, Loop, distanceFromStart, ref part);
	}

	public Vector3 GetPoint(float distanceFromStart)
	{
		return MathTools.GetPointAtDistanceFromStart(Points, Loop, distanceFromStart);
	}

	public Vector3 GetPoint(int index)
	{
		return Points[index];
	}

	public int SortPoints(Vector3 initialPointReference)
	{
		return MathTools.SortLineStripUsingClosestSequentialPointsMethod(Points, initialPointReference);
	}

	#endregion

	#region Segments

	public int SegmentCount
	{
		get
		{
			if (Points == null)
				return 0;

			if (Loop)
				return Points.Count;
			else
				return Points.Count - 1;
		}
	}

	#endregion

	#region Length

	public bool IsTotalLengthInvalidated { get; private set; }
	private float _TotalLength = -1f;
	public float TotalLength
	{
		get
		{
			if (IsTotalLengthInvalidated || _TotalLength < 0f)
			{
				_TotalLength = CalculateTotalLength();
			}
			return _TotalLength;
		}
	}

	public bool IsAverageSegmentLengthInvalidated { get; private set; }
	private float _AverageSegmentLength = -1f;
	public float AverageSegmentLength
	{
		get
		{
			if (IsAverageSegmentLengthInvalidated || _AverageSegmentLength < 0f)
			{
				_AverageSegmentLength = CalculateAverageSegmentLength();
			}
			return _AverageSegmentLength;
		}
	}

	private float CalculateTotalLength()
	{
		if (Points == null || Points.Count < 2)
			return 0f;

		var totalLength = 0f;
		var previousPoint = GetPoint(0);
		for (int i = 1; i < Points.Count; i++)
		{
			var currentPoint = GetPoint(i);
			totalLength += Vector3.Distance(previousPoint, currentPoint);
			previousPoint = currentPoint;
		}
		if (Loop)
		{
			totalLength += Vector3.Distance(previousPoint, GetPoint(0));
		}
		return totalLength;
	}

	private float CalculateAverageSegmentLength()
	{
		if (Points == null || Points.Count < 2)
			return 0f;

		return TotalLength / SegmentCount;
	}

	#endregion

	#region Calculations - Closest Point

	public Vector3 ClosestPointOnLine(Vector3 point)
	{
		return Points.ClosestPointOnLineStrip(point, Loop);
	}

	public Vector3 ClosestPointOnLine(Vector3 point, ref Vector3 part)
	{
		return Points.ClosestPointOnLineStrip(point, Loop, ref part);
	}

	public float DistanceFromStartOfClosestPointOnLine(Vector3 point)
	{
		return Points.DistanceFromStartOfClosestPointOnLineStrip(point, Loop);
	}

	#endregion

	#region Calculations - Point Ahead

	public Vector3 GetPointAheadOfClosestPoint(Vector3 point, float resultingPointDistanceToClosestPoint)
	{
		var distanceFromStartOfClosestPointOnLine = DistanceFromStartOfClosestPointOnLine(point);
		var distanceFromStartOfResultingPoint = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
		if (Loop)
		{
			distanceFromStartOfResultingPoint = distanceFromStartOfResultingPoint % TotalLength;
		}
		return GetPoint(distanceFromStartOfResultingPoint);
	}

	#endregion

	#region Invalidate

	public void Invalidate()
	{
		IsTotalLengthInvalidated = true;
		IsAverageSegmentLengthInvalidated = true;
	}

	#endregion

	#region Debug and Gizmos

#if UNITY_EDITOR

	[Serializable]
	public class DebugConfigurationData
	{
		public bool DrawUnselected = true;
		public Color UnselectedGizmoColor = new Color(0.4f, 0.4f, 0.42f);
		public Color GizmoColor = new Color(0.0f, 0.0f, 1.0f);
		public float FirstControlPointSizeFactor = 0.08f;
		public float ControlPointSizeFactor = 0.04f;
	}

	[Header("Debug")]
	public DebugConfigurationData DEBUG;

	private void OnDrawGizmos()
	{
		if (DEBUG.DrawUnselected)
		{
			Gizmos.color = DEBUG.UnselectedGizmoColor;
			DrawPath();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (!IsAnyPointAvailable)
			return;

		Gizmos.color = DEBUG.GizmoColor;
		DrawPath();
	}

	private void DrawPath()
	{
		var previousPoint = GetPoint(0);

		var size = AverageSegmentLength * DEBUG.FirstControlPointSizeFactor;
		var size3 = new Vector3(size, size, size);

		Gizmos.DrawWireCube(previousPoint, new Vector3(size, size, size));

		size = AverageSegmentLength * DEBUG.ControlPointSizeFactor;
		size3.Set(size, size, size);

		for (int i = 1; i < Points.Count; i++)
		{
			var currentPoint = GetPoint(i);
			Gizmos.DrawLine(previousPoint, currentPoint);
			Gizmos.DrawWireCube(currentPoint, size3);
			previousPoint = currentPoint;
		}
		if (Loop)
		{
			Gizmos.DrawLine(previousPoint, GetPoint(0));
		}
	}

#endif

	#endregion

	#region Editor

	protected void OnValidate()
	{
		Invalidate();
	}

	#endregion
}
