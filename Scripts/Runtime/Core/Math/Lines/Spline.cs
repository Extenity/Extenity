using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.GameObjectToolbox;
using Extenity.UnityEditorToolbox;
using UnityEngine.Events;

namespace Extenity.MathToolbox
{

	public class Spline : MonoBehaviour
	{
		#region Initialization

		public void ClearData()
		{
			ClearRawPoints();
			ClearProcessedPoints();
			InvalidateRawLine();
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
		public bool SmoothingEnabled = true;
		public bool KeepDataInLocalCoordinates = true;

		#endregion

		#region Points - Raw

		[Header("Data")]
		public List<Vector3> RawPoints;

		public bool IsAnyRawPointAvailable => RawPoints != null && RawPoints.Count > 0;

		private void ClearRawPoints()
		{
			RawPoints?.Clear();
		}

		public Vector3 GetRawPointPosition(int index)
		{
			return RawPoints[index];
		}

		public Vector3 GetRawPointLocalPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? RawPoints[index]
				: transform.InverseTransformPoint(RawPoints[index]);
		}

		public Vector3 GetRawPointWorldPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? transform.TransformPoint(RawPoints[index])
				: RawPoints[index];
		}

		public int SortRawLineStripUsingClosestSequentialPointsMethod(Vector3 initialPointReference)
		{
			return RawPoints.SortLineStripUsingClosestSequentialPointsMethod(initialPointReference);
		}

		#endregion

		#region Points - Processed

		[NonSerialized]
		public List<Vector3> ProcessedPoints;

		public bool IsAnyProcessedPointAvailable => ProcessedPoints != null && ProcessedPoints.Count > 0;

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

		public Vector3 GetProcessedPointPosition(int index)
		{
			return ProcessedPoints[index];
		}

		public Vector3 GetProcessedPointLocalPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? ProcessedPoints[index]
				: transform.InverseTransformPoint(ProcessedPoints[index]);
		}

		public Vector3 GetProcessedPointWorldPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? transform.TransformPoint(ProcessedPoints[index])
				: ProcessedPoints[index];
		}

		#endregion

		#region Process

		public void ProcessPoints()
		{
			// Process
			if (SmoothingEnabled)
			{
				// Clear previous points
				if (ProcessedPoints == null)
				{
					ProcessedPoints = new List<Vector3>();
				}
				else
				{
					ProcessedPoints.Clear();
				}

				ProcessPointsUsingSmoothing(ProcessedPoints, RawPoints);
			}
			else
			{
				ProcessedPoints = RawPoints;
			}

			InvalidateProcessedLine();
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

		#region Segments

		public int RawSegmentCount => RawPoints.GetLineSegmentCount(Loop);
		public int ProcessedSegmentCount => ProcessedPoints.GetLineSegmentCount(Loop);

		#endregion

		#region Length - Raw

		public bool IsTotalRawLengthInvalidated { get; private set; }
		private float _TotalRawLength = -1f;
		public float TotalRawLength
		{
			get
			{
				if (IsTotalRawLengthInvalidated || _TotalRawLength < 0f)
				{
					_TotalRawLength = RawPoints?.CalculateLineStripLength(Loop) ?? 0f;
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
					_AverageRawSegmentLength = CalculateAverageRawSegmentLength();
				}
				return _AverageRawSegmentLength;
			}
		}

		private float CalculateAverageRawSegmentLength()
		{
			if (RawPoints == null || RawPoints.Count < 2)
				return 0f;

			return TotalRawLength / RawSegmentCount;
		}

		#endregion

		#region Length - Processed

		public bool IsTotalProcessedLengthInvalidated { get; private set; }
		private float _TotalProcessedLength = -1f;
		public float TotalProcessedLength
		{
			get
			{
				if (IsTotalProcessedLengthInvalidated || _TotalProcessedLength < 0f)
				{
					_TotalProcessedLength = ProcessedPoints == RawPoints ? TotalRawLength : ProcessedPoints?.CalculateLineStripLength(Loop) ?? 0f;
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
					_AverageProcessedSegmentLength = ProcessedPoints == RawPoints ? AverageRawSegmentLength : CalculateAverageProcessedSegmentLength();
				}
				return _AverageProcessedSegmentLength;
			}
		}

		private float CalculateAverageProcessedSegmentLength()
		{
			if (ProcessedPoints == null || ProcessedPoints.Count < 2)
				return 0f;

			return TotalProcessedLength / ProcessedSegmentCount;
		}

		#endregion

		#region Calculations - Raw

		public Vector3 GetRawPointAtDistanceFromStart(float distanceFromStart, Space space)
		{
			var position = RawPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart);
			return TransformFromDataSpace(position, space);
		}

		public Vector3 GetRawPointAtDistanceFromStart(float distanceFromStart, ref Vector3 part, Space space)
		{
			var position = RawPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart, ref part);
			return TransformFromDataSpace(position, space);
		}

		public Vector3 ClosestPointOnRawLine(Vector3 point, Space space)
		{
			var position = RawPoints.ClosestPointOnLineStrip(point, Loop);
			return TransformFromDataSpace(position, space);
		}

		public Vector3 ClosestPointOnRawLine(Vector3 point, ref Vector3 part, Space space)
		{
			var position = RawPoints.ClosestPointOnLineStrip(point, Loop, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float DistanceFromStartOfClosestPointOnRawLine(Vector3 point)
		{
			return RawPoints.DistanceFromStartOfClosestPointOnLineStrip(point, Loop);
		}

		public Vector3 GetPointAheadOfClosestPointOnRawLine(Vector3 point, float resultingPointDistanceToClosestPoint, Space space)
		{
			throw new NotImplementedException(); // Look into Line to implement this.
		}

		#endregion

		#region Calculations - Processed

		public Vector3 GetProcessedPointAtDistanceFromStart(float distanceFromStart, Space space)
		{
			var position = ProcessedPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart);
			return TransformFromDataSpace(position, space);
		}

		public Vector3 GetProcessedPointAtDistanceFromStart(float distanceFromStart, ref Vector3 part, Space space)
		{
			var position = ProcessedPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart, ref part);
			return TransformFromDataSpace(position, space);
		}

		public Vector3 ClosestPointOnProcessedLine(Vector3 point, Space space)
		{
			var position = ProcessedPoints.ClosestPointOnLineStrip(point, Loop);
			return TransformFromDataSpace(position, space);
		}

		public Vector3 ClosestPointOnProcessedLine(Vector3 point, ref Vector3 part, Space space)
		{
			var position = ProcessedPoints.ClosestPointOnLineStrip(point, Loop, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float DistanceFromStartOfClosestPointOnProcessedLine(Vector3 point)
		{
			return ProcessedPoints.DistanceFromStartOfClosestPointOnLineStrip(point, Loop);
		}

		public Vector3 GetPointAheadOfClosestPointOnProcessedLine(Vector3 point, float resultingPointDistanceToClosestPoint, Space space)
		{
			throw new NotImplementedException(); // Look into Line to implement this.
		}

		#endregion

		#region Invalidate - Raw

		public readonly UnityEvent OnRawLineInvalidated = new UnityEvent();

		public void InvalidateRawLine()
		{
			IsTotalRawLengthInvalidated = true;
			IsAverageRawSegmentLengthInvalidated = true;

			OnRawLineInvalidated.Invoke();

			// This also invalidates the processed line.
			InvalidateProcessedLine();
		}

		#endregion

		#region Invalidate - Processed

		public readonly UnityEvent OnProcessedLineInvalidated = new UnityEvent();

		public void InvalidateProcessedLine()
		{
			IsTotalProcessedLengthInvalidated = true;
			IsAverageProcessedSegmentLengthInvalidated = true;

			OnProcessedLineInvalidated.Invoke();
		}

		#endregion

		#region Space Transformation

		public Vector3 TransformFromDataSpace(Vector3 pointInDataSpace, Space targetSpace)
		{
			switch (targetSpace)
			{
				case Space.Unspecified: return pointInDataSpace;
				case Space.World:
					return KeepDataInLocalCoordinates
						? transform.TransformPoint(pointInDataSpace)
						: pointInDataSpace;
				case Space.Local:
					return KeepDataInLocalCoordinates
						? pointInDataSpace
						: transform.InverseTransformPoint(pointInDataSpace);
				default:
					throw new ArgumentOutOfRangeException(nameof(targetSpace), targetSpace, null);
			}
		}

		#endregion

		#region Debug and Gizmos

#if UNITY_EDITOR

		[Serializable]
		public class DebugConfigurationData
		{
			public bool DrawUnselectedRaw = false;
			public bool DrawUnselectedProcessed = true;
			public Color UnselectedColor = new Color(0.4f, 0.4f, 0.42f);
			public Color RawPointColor = new Color(0.0f, 0.0f, 1.0f);
			public Color RawLineColor = new Color(0.0f, 0.0f, 1.0f);
			public Color ProcessedPointColor = new Color(0.4f, 0.4f, 0.6f);
			public Color ProcessedLineColor = new Color(0.4f, 0.4f, 0.6f);
			public float RawPointSize = 0.04f;
			public float ProcessedPointSize = 0.04f;
			public float FirstRawPointSizeFactor = 1.1f;
		}

		[Header("Debug")]
		public DebugConfigurationData DEBUG;

		private void OnDrawGizmos()
		{
			if (DEBUG.DrawUnselectedRaw)
			{
				GizmosTools.DrawPathLines(GetRawPointWorldPosition, RawPoints?.Count ?? 0, Loop, DEBUG.UnselectedColor);
			}
			if (DEBUG.DrawUnselectedProcessed)
			{
				GizmosTools.DrawPathLines(GetProcessedPointWorldPosition, ProcessedPoints?.Count ?? 0, Loop, DEBUG.UnselectedColor);
			}
		}

		private void OnDrawGizmosSelected()
		{
			GizmosTools.DrawPath(GetRawPointWorldPosition, RawPoints?.Count ?? 0, Loop, true, DEBUG.RawPointColor, true, DEBUG.RawLineColor, AverageRawSegmentLength * DEBUG.RawPointSize, DEBUG.FirstRawPointSizeFactor);
			GizmosTools.DrawPath(GetProcessedPointWorldPosition, ProcessedPoints?.Count ?? 0, Loop, true, DEBUG.ProcessedPointColor, true, DEBUG.ProcessedLineColor, AverageProcessedSegmentLength * DEBUG.ProcessedPointSize, 1f);
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

			UnityEditor.Selection.selectionChanged -= CheckIfNeedToStopEditing;
			UnityEditor.Selection.selectionChanged += CheckIfNeedToStopEditing;
		}

		public void StopEditing()
		{
			UnityEditor.Selection.selectionChanged -= CheckIfNeedToStopEditing;

			if (IsEditing)
			{
				Log.Info($"Finished editing '{gameObject.FullName()}'");
				IsEditing = false;
			}

			var dontShowEditorHandle = gameObject.GetComponent<DontShowEditorHandler>();
			if (dontShowEditorHandle)
			{
				UnityEditor.EditorApplication.delayCall += () =>
				 {
					 if (dontShowEditorHandle)
					 {
						 DestroyImmediate(dontShowEditorHandle);
					 }
				 };
			}
		}

		private void CheckIfNeedToStopEditing()
		{
			if (!this)
			{
				UnityEditor.Selection.selectionChanged -= CheckIfNeedToStopEditing;
				return;
			}

			if (UnityEditor.Selection.activeGameObject != gameObject)
			{
				StopEditing();
			}
		}

#endif

		#endregion

		#region Editor

#if UNITY_EDITOR

		protected void OnValidate()
		{
			// TODO: Not cool to always invalidate everything. But it's a quick and robust solution for now.
			InvalidateRawLine();
		}

#endif

		#endregion
	}

}
