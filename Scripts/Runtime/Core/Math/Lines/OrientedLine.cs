using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.UnityEditorToolbox;
using UnityEngine.Events;

namespace Extenity.MathToolbox
{

	public class OrientedLine : MonoBehaviour
	{
		#region Initialization

		public void ClearData()
		{
			ClearPoints();
			Invalidate();
		}

		#endregion

		#region Deinitialization

#if UNITY_EDITOR

		private void OnDisable()
		{
			StopEditing();
		}

#endif

		#endregion

		#region Configuration

		[Header("Configuration")]
		public bool Loop = false;
		// TODO: Implement KeepDataInLocalCoordinates. See 1798515712.
		//public bool KeepDataInLocalCoordinates = false;

		//var point = KeepDataInLocalCoordinates
		//	? transform.TransformPoint(points[0])
		//	: points[0];

		#endregion

		#region Points

		[Header("Data")]
		public List<OrientedPoint> Points;

		public bool IsAnyPointAvailable => Points != null && Points.Count > 0;

		private void ClearPoints()
		{
			Points?.Clear();
		}

		public Vector3 GetPoint(float distanceFromStart, ref Vector3 part)
		{
			return Points.GetPointAtDistanceFromStart(Loop, distanceFromStart, ref part);
		}

		public Vector3 GetPoint(float distanceFromStart)
		{
			return Points.GetPointAtDistanceFromStart(Loop, distanceFromStart);
		}

		public Vector3 GetPoint(int index)
		{
			return Points[index].Position;
		}

		public OrientedPoint GetPointAndOrientation(int index)
		{
			return Points[index];
		}

		public int SortLineStripUsingClosestSequentialPointsMethod(Vector3 initialPointReference)
		{
			return Points.SortLineStripUsingClosestSequentialPointsMethod(initialPointReference);
		}

		#endregion

		#region Segments

		public int SegmentCount => Points.GetLineSegmentCount(Loop);

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
					_TotalLength = Points?.CalculateLineStripLength(Loop) ?? 0f;
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

		private float CalculateAverageSegmentLength()
		{
			if (Points == null || Points.Count < 2)
				return 0f;

			return TotalLength / SegmentCount;
		}

		#endregion

		#region Calculations

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

		public Vector3 GetPointAheadOfClosestPoint(Vector3 point, float resultingPointDistanceToClosestPoint)
		{
			throw new NotImplementedException(); // Move this into MathTools.

			var distanceFromStartOfClosestPointOnLine = DistanceFromStartOfClosestPointOnLine(point);
			var distanceFromStartOfResultingPoint = distanceFromStartOfClosestPointOnLine + resultingPointDistanceToClosestPoint;
			if (Loop)
			{
				distanceFromStartOfResultingPoint = distanceFromStartOfResultingPoint % TotalLength;
			}
			return GetPoint(distanceFromStartOfResultingPoint);
		}

		#endregion

		#region Operations

		public void NormalizeAllOrientations()
		{
			if (Points == null)
				return;

			for (var i = 0; i < Points.Count; i++)
			{
				Points[i] = Points[i].WithOrientation(Points[i].Orientation.normalized);
			}

			Invalidate();
		}

		#endregion

		#region Invalidate

		public readonly UnityEvent OnInvalidated = new UnityEvent();

		public void Invalidate()
		{
			IsTotalLengthInvalidated = true;
			IsAverageSegmentLengthInvalidated = true;

			OnInvalidated.Invoke();
		}

		#endregion

		#region Debug and Gizmos

#if UNITY_EDITOR

		[Serializable]
		public class DebugConfigurationData
		{
			public bool DrawUnselected = true;
			public Color UnselectedColor = new Color(0.4f, 0.4f, 0.42f);
			public Color PointColor = new Color(0.0f, 0.0f, 1.0f);
			public Color LineColor = new Color(0.0f, 0.0f, 1.0f);
			public float PointSize = 0.04f;
			public float FirstPointSizeFactor = 1.1f;
		}

		[Header("Debug")]
		public DebugConfigurationData DEBUG;

		private void OnDrawGizmos()
		{
			if (DEBUG.DrawUnselected)
			{
				GizmosTools.DrawPathLines(GetPoint, Points?.Count ?? 0, Loop, DEBUG.UnselectedColor);
			}
		}

		private void OnDrawGizmosSelected()
		{
			GizmosTools.DrawPath(GetPoint, Points?.Count ?? 0, Loop, true, DEBUG.PointColor, true, DEBUG.LineColor, AverageSegmentLength * DEBUG.PointSize, DEBUG.FirstPointSizeFactor);

			if (Points?.Count > 0)
			{
				Gizmos.color = DEBUG.LineColor;
				foreach (var point in Points)
				{
					Gizmos.DrawLine(point.Position, point.Position + point.Orientation * AverageSegmentLength * DEBUG.PointSize);
				}
			}
		}

#endif

		#endregion

		#region Edit Mode

#if UNITY_EDITOR

		public bool IsEditing { get; private set; }

		public void StartEditing()
		{
			Log.Info($"Starting to edit '{gameObject.FullName()}'");

			IsEditing = true;
			gameObject.GetSingleOrAddComponent<DontShowEditorHandler>();
		}

		public void StopEditing()
		{
			if (IsEditing)
			{
				Log.Info($"Finished editing '{gameObject.FullName()}'");
				IsEditing = false;
			}

			var dontShowEditorHandle = gameObject.GetComponent<DontShowEditorHandler>();
			if (dontShowEditorHandle)
			{
				UnityEditor.EditorApplication.delayCall+= () =>
				{
					if (dontShowEditorHandle)
					{
						DestroyImmediate(dontShowEditorHandle);
					}
				};
			}
		}

#endif

		#endregion

		#region Editor

#if UNITY_EDITOR

		protected void OnValidate()
		{
			// TODO: Not cool to always invalidate everything. But it's a quick and robust solution for now.
			Invalidate();
		}

#endif

		#endregion
	}

}
