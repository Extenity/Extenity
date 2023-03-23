#if UNITY

using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.UnityEditorToolbox;
using Unity.Mathematics;
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
		public List<float3> RawPoints;

		public bool IsAnyRawPointAvailable => RawPoints != null && RawPoints.Count > 0;

		private void ClearRawPoints()
		{
			RawPoints?.Clear();
		}

		public float3 GetRawPointPosition(int index)
		{
			return RawPoints[index];
		}

		public float3 GetRawPointLocalPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? RawPoints[index]
				: transform.InverseTransformPoint(RawPoints[index]);
		}

		public float3 GetRawPointWorldPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? transform.TransformPoint(RawPoints[index])
				: RawPoints[index];
		}

		public int SortRawLineStripUsingClosestSequentialPointsMethod(float3 initialPointReference)
		{
			return RawPoints.SortLineStripUsingClosestSequentialPointsMethod(initialPointReference);
		}

		#endregion

		#region Points - Processed

		[NonSerialized]
		public List<float3> ProcessedPoints;

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

		public float3 GetProcessedPointPosition(int index)
		{
			return ProcessedPoints[index];
		}

		public float3 GetProcessedPointLocalPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? ProcessedPoints[index]
				: transform.InverseTransformPoint(ProcessedPoints[index]);
		}

		public float3 GetProcessedPointWorldPosition(int index)
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
					ProcessedPoints = new List<float3>();
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

		private static void ProcessPointsUsingSmoothing(List<float3> smoothedPoints, List<float3> points)
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

		public float3 GetRawPointAtDistanceFromStart(float distanceFromStart, Space space)
		{
			var position = RawPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart);
			return TransformFromDataSpace(position, space);
		}

		public float3 GetRawPointAtDistanceFromStart(float distanceFromStart, ref float3 part, Space space)
		{
			var position = RawPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float3 ClosestPointOnRawLine(float3 point, Space space)
		{
			var position = RawPoints.ClosestPointOnLineStrip(point, Loop);
			return TransformFromDataSpace(position, space);
		}

		public float3 ClosestPointOnRawLine(float3 point, ref float3 part, Space space)
		{
			var position = RawPoints.ClosestPointOnLineStrip(point, Loop, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float DistanceFromStartOfClosestPointOnRawLine(float3 point)
		{
			return RawPoints.DistanceFromStartOfClosestPointOnLineStrip(point, Loop);
		}

		public float3 GetPointAheadOfClosestPointOnRawLine(float3 point, float resultingPointDistanceToClosestPoint, Space space)
		{
			var position = RawPoints.GetPointAheadOfClosestPoint(point, resultingPointDistanceToClosestPoint, Loop, TotalRawLength);
			return TransformFromDataSpace(position, space);
		}

		#endregion

		#region Calculations - Processed

		public float3 GetProcessedPointAtDistanceFromStart(float distanceFromStart, Space space)
		{
			var position = ProcessedPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart);
			return TransformFromDataSpace(position, space);
		}

		public float3 GetProcessedPointAtDistanceFromStart(float distanceFromStart, ref float3 part, Space space)
		{
			var position = ProcessedPoints.GetPointAtDistanceFromStart(Loop, distanceFromStart, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float3 ClosestPointOnProcessedLine(float3 point, Space space)
		{
			var position = ProcessedPoints.ClosestPointOnLineStrip(point, Loop);
			return TransformFromDataSpace(position, space);
		}

		public float3 ClosestPointOnProcessedLine(float3 point, ref float3 part, Space space)
		{
			var position = ProcessedPoints.ClosestPointOnLineStrip(point, Loop, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float DistanceFromStartOfClosestPointOnProcessedLine(float3 point)
		{
			return ProcessedPoints.DistanceFromStartOfClosestPointOnLineStrip(point, Loop);
		}

		public float3 GetPointAheadOfClosestPointOnProcessedLine(float3 point, float resultingPointDistanceToClosestPoint, Space space)
		{
			var position = ProcessedPoints.GetPointAheadOfClosestPoint(point, resultingPointDistanceToClosestPoint, Loop, TotalProcessedLength);
			return TransformFromDataSpace(position, space);
		}

		#endregion

		#region Operations

		public void MirrorX()
		{
			if (RawPoints == null)
				return;
			for (var i = 0; i < RawPoints.Count; i++)
			{
				var point = RawPoints[i];
				point.x = -point.x;
				RawPoints[i] = point;
			}
			InvalidateRawLine();
		}

		public void MirrorY()
		{
			if (RawPoints == null)
				return;
			for (var i = 0; i < RawPoints.Count; i++)
			{
				var point = RawPoints[i];
				point.y = -point.y;
				RawPoints[i] = point;
			}
			InvalidateRawLine();
		}

		public void MirrorZ()
		{
			if (RawPoints == null)
				return;
			for (var i = 0; i < RawPoints.Count; i++)
			{
				var point = RawPoints[i];
				point.z = -point.z;
				RawPoints[i] = point;
			}
			InvalidateRawLine();
		}

		public void MoveToLocalZero(bool keepWorldPosition)
		{
			if (!IsAnyRawPointAvailable)
				return;
			if (keepWorldPosition)
			{
				var matrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
				for (var i = 0; i < RawPoints.Count; i++)
				{
					RawPoints[i] = matrix.MultiplyPoint(RawPoints[i]);
				}
			}
			transform.ResetTransformToLocalZero();
			InvalidateRawLine();
		}

		public void MoveToStart(bool keepWorldPosition)
		{
			if (!IsAnyRawPointAvailable)
				return;
			var pointPosition = GetRawPointLocalPosition(0);
			var matrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
			var newPosition = matrix.MultiplyPoint(pointPosition);
			if (keepWorldPosition)
			{
				matrix = Matrix4x4.TRS(-newPosition, Quaternion.identity, Vector3Tools.One) * matrix;
				for (var i = 0; i < RawPoints.Count; i++)
				{
					RawPoints[i] = matrix.MultiplyPoint(RawPoints[i]);
				}
			}
			transform.SetLocalLocation(newPosition, Quaternion.identity, Vector3Tools.One);
			InvalidateRawLine();
		}

		public void MoveToEnd(bool keepWorldPosition)
		{
			if (!IsAnyRawPointAvailable)
				return;
			var pointPosition = GetRawPointLocalPosition(RawPoints.Count - 1);
			var matrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
			var newPosition = matrix.MultiplyPoint(pointPosition);
			if (keepWorldPosition)
			{
				matrix = Matrix4x4.TRS(-newPosition, Quaternion.identity, Vector3Tools.One) * matrix;
				for (var i = 0; i < RawPoints.Count; i++)
				{
					RawPoints[i] = matrix.MultiplyPoint(RawPoints[i]);
				}
			}
			transform.SetLocalLocation(newPosition, Quaternion.identity, Vector3Tools.One);
			InvalidateRawLine();
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

		public float3 TransformFromDataSpace(float3 pointInDataSpace, Space targetSpace)
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
			Log.InfoWithContext(this, $"Starting to edit '{this.FullGameObjectName()}'");

			IsEditing = true;
			var helper = gameObject.GetSingleOrAddComponent<DontShowEditorHandler>();
			helper.hideFlags = HideFlags.DontSave;

			UnityEditor.Selection.selectionChanged -= CheckIfNeedToStopEditing;
			UnityEditor.Selection.selectionChanged += CheckIfNeedToStopEditing;
		}

		public void StopEditing()
		{
			UnityEditor.Selection.selectionChanged -= CheckIfNeedToStopEditing;

			if (IsEditing)
			{
				Log.InfoWithContext(this, $"Finished editing '{this.FullGameObjectName()}'");
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

		#region Log

		private static readonly Logger Log = new(nameof(Spline));

		#endregion
	}

}

#endif
