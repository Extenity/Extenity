using System;
using UnityEngine;
using System.Collections.Generic;

namespace Extenity.MathToolbox
{

	public class Spline : MonoBehaviour
	{
		#region Initialization

		public void ClearData()
		{
			//Logger.Log("######### Clear");
			ClearRawPoints();
			ClearProcessedPoints();
		}

		#endregion

		#region Configuration

		[Header("Configuration")]
		//public bool SmoothingEnabled = false;
		public bool KeepDataInLocalCoordinates = true;

		#endregion

		#region Points

		[Header("Data")]
		public List<Vector3> RawPoints;

		public bool IsAnyRawPointAvailable
		{
			get { return RawPoints != null && RawPoints.Count > 0; }
		}

		private void ClearRawPoints()
		{
			if (RawPoints == null)
			{
				RawPoints = new List<Vector3>();
			}
			else
			{
				RawPoints.Clear();
			}
		}

		public Vector3 GetRawPoint(float distanceFromStart, ref Vector3 part)
		{
			return RawPoints.GetPointAtDistanceFromStart(distanceFromStart, ref part);
		}

		public int SortRawPoints(Vector3 initialPointReference)
		{
			return RawPoints.SortLineStripUsingClosestSequentialPointsMethod(initialPointReference);
		}

		#endregion

		#region Processed Points

		[NonSerialized]
		public List<Vector3> ProcessedPoints;

		public bool IsAnyProcessedPointAvailable
		{
			get { return ProcessedPoints != null && ProcessedPoints.Count > 0; }
		}

		private void ClearProcessedPoints()
		{
			if (ProcessedPoints != null)
			{
				if (ProcessedPoints == RawPoints)
				{
					ProcessedPoints = null;
				}
				else
				{
					ProcessedPoints.Clear();
				}
			}
		}

		public Vector3 GetProcessedPoint(float distanceFromStart, ref Vector3 part)
		{
			return ProcessedPoints.GetPointAtDistanceFromStart(distanceFromStart, ref part);
		}

		public void ProcessPoints()
		{
			//Logger.Log("######### ProcessPoints");

			// Clear previous points
			if (ProcessedPoints == null)
			{
				ProcessedPoints = new List<Vector3>();
			}
			else
			{
				ProcessedPoints.Clear();
			}

			// Process
			//if (SmoothingEnabled)
			{
				ProcessPointsUsingSmoothing(ProcessedPoints, RawPoints);
			}
			//else
			//{
			//	ProcessedPoints = RawPoints;
			//}

			InvalidateProcessedLineLengths();
		}

		private static void ProcessPointsUsingSmoothing(List<Vector3> smoothedPoints, List<Vector3> points)
		{
			if (points == null || points.Count == 0)
				return;

			for (int i = 0; i < points.Count - 1; i++)
			{
				var previousPoint = points[Mathf.Max(0, i - 1)];
				var startPoint = points[i];
				var endPoint = points[i + 1];
				var nextPoint = points[Mathf.Min(points.Count - 1, i + 2)];

				for (float t = 0; t < 0.95f; t += 0.1f)
				{
					var point = MathTools.GetCatmullRomPoint(previousPoint, startPoint, endPoint, nextPoint, t);
					smoothedPoints.Add(point);
				}
			}

			smoothedPoints.Add(points[points.Count - 1]);
		}

		#endregion

		#region Length

		public bool IsTotalRawLengthInvalidated { get; private set; }
		private float _TotalRawLength = -1f;
		public float TotalRawLength
		{
			get
			{
				if (IsTotalRawLengthInvalidated || _TotalRawLength < 0f)
				{
					_TotalRawLength = RawPoints == null
						? 0f
						: RawPoints.CalculateLineStripLength();
				}
				return _TotalRawLength;
			}
		}

		public bool IsAverageRawSegmentLengthInvalidated { get; private set; }
		private float _AverageRawSegmentLength = -1f;
		public float AverageRawSegmentLength
		{
			get
			{
				if (IsAverageRawSegmentLengthInvalidated || _AverageRawSegmentLength < 0f)
				{
					_AverageRawSegmentLength = RawPoints == null
						? 0f
						: RawPoints.CalculateAverageLengthOfLineStripParts();
				}
				return _AverageRawSegmentLength;
			}
		}

		public bool IsTotalProcessedLengthInvalidated { get; private set; }
		private float _TotalProcessedLength = -1f;
		public float TotalProcessedLength
		{
			get
			{
				if (IsTotalProcessedLengthInvalidated || _TotalProcessedLength < 0f)
				{
					if (ProcessedPoints == RawPoints)
					{
						_TotalProcessedLength = TotalRawLength;
					}
					else
					{
						_TotalProcessedLength = ProcessedPoints == null
							? 0f
							: ProcessedPoints.CalculateLineStripLength();
					}
				}
				return _TotalProcessedLength;
			}
		}

		public bool IsAverageProcessedSegmentLengthInvalidated { get; private set; }
		private float _AverageProcessedSegmentLength = -1f;
		public float AverageProcessedSegmentLength
		{
			get
			{
				if (IsAverageProcessedSegmentLengthInvalidated || _AverageProcessedSegmentLength < 0f)
				{
					if (ProcessedPoints == RawPoints)
					{
						_AverageProcessedSegmentLength = AverageRawSegmentLength;
					}
					else
					{
						_AverageProcessedSegmentLength = ProcessedPoints == null
							? 0f
							: ProcessedPoints.CalculateAverageLengthOfLineStripParts();
					}
				}
				return _AverageProcessedSegmentLength;
			}
		}

		#endregion

		#region Invalidate

		public void InvalidateDelayed(float delay = 0.1f)
		{
			CancelInvoke("Invalidate");
			Invoke("Invalidate", delay);
		}

		public void Invalidate()
		{
			//Logger.Log("######### Invalidate");

			CancelInvoke("Invalidate");

			InvalidateRawLineLengths();
			//InvalidateProcessedLineLengths(); // ProcessPoints() already does this.

			ProcessPoints();
		}

		public void InvalidateRawLineLengths()
		{
			IsTotalRawLengthInvalidated = true;
			IsAverageRawSegmentLengthInvalidated = true;
		}

		public void InvalidateProcessedLineLengths()
		{
			IsTotalProcessedLengthInvalidated = true;
			IsAverageProcessedSegmentLengthInvalidated = true;
		}

		#endregion

		#region Gizmos

#if UNITY_EDITOR

		[Header("Debug")]
		public Color DEBUG_GizmoColor = new Color(0.6f, 0.6f, 0.7f);
		public float DEBUG_ControlPointSizeFactor = 0.04f;
		private float DEBUG_ControlPointSize;
		private Vector3 DEBUG_ControlPointSize3;

		private void OnDrawGizmosSelected()
		{
			if (!IsAnyRawPointAvailable)
				return;

			Gizmos.color = DEBUG_GizmoColor;
			DEBUG_ControlPointSize = AverageRawSegmentLength * DEBUG_ControlPointSizeFactor;
			DEBUG_ControlPointSize3.Set(DEBUG_ControlPointSize, DEBUG_ControlPointSize, DEBUG_ControlPointSize);

			var previousPoint = KeepDataInLocalCoordinates
				? transform.TransformPoint(RawPoints[0])
				: RawPoints[0];
			Gizmos.DrawWireCube(previousPoint, DEBUG_ControlPointSize3);

			for (int i = 1; i < RawPoints.Count; i++)
			{
				var currentPoint = KeepDataInLocalCoordinates
					? transform.TransformPoint(RawPoints[i])
					: RawPoints[i];

				Gizmos.DrawLine(previousPoint, currentPoint);
				Gizmos.DrawWireCube(currentPoint, DEBUG_ControlPointSize3);
				previousPoint = currentPoint;
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

}
