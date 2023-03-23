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

	public class Line : MonoBehaviour
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
		public bool KeepDataInLocalCoordinates = true;

		#endregion

		#region Points

		[Header("Data")]
		public List<float3> Points;

		public bool IsAnyPointAvailable => Points != null && Points.Count > 0;

		private void ClearPoints()
		{
			Points?.Clear();
		}

		public float3 GetPointPosition(int index)
		{
			return Points[index];
		}

		public float3 GetPointLocalPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? Points[index]
				: transform.InverseTransformPoint(Points[index]);
		}

		public float3 GetPointWorldPosition(int index)
		{
			return KeepDataInLocalCoordinates
				? transform.TransformPoint(Points[index])
				: Points[index];
		}

		public int SortLineStripUsingClosestSequentialPointsMethod(float3 initialPointReference)
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

		public float3 GetPointAtDistanceFromStart(float distanceFromStart, Space space)
		{
			var position = Points.GetPointAtDistanceFromStart(Loop, distanceFromStart);
			return TransformFromDataSpace(position, space);
		}

		public float3 GetPointAtDistanceFromStart(float distanceFromStart, ref float3 part, Space space)
		{
			var position = Points.GetPointAtDistanceFromStart(Loop, distanceFromStart, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float3 ClosestPointOnLine(float3 point, Space space)
		{
			var position = Points.ClosestPointOnLineStrip(point, Loop);
			return TransformFromDataSpace(position, space);
		}

		public float3 ClosestPointOnLine(float3 point, ref float3 part, Space space)
		{
			var position = Points.ClosestPointOnLineStrip(point, Loop, ref part);
			return TransformFromDataSpace(position, space);
		}

		public float DistanceFromStartOfClosestPointOnLine(float3 point)
		{
			return Points.DistanceFromStartOfClosestPointOnLineStrip(point, Loop);
		}

		public float3 GetPointAheadOfClosestPoint(float3 point, float resultingPointDistanceToClosestPoint, Space space)
		{
			var position = Points.GetPointAheadOfClosestPoint(point, resultingPointDistanceToClosestPoint, Loop, TotalLength);
			return TransformFromDataSpace(position, space);
		}

		#endregion

		#region Operations

		public void MirrorX()
		{
			if (Points == null)
				return;
			for (var i = 0; i < Points.Count; i++)
			{
				var point = Points[i];
				point.x = -point.x;
				Points[i] = point;
			}
			Invalidate();
		}

		public void MirrorY()
		{
			if (Points == null)
				return;
			for (var i = 0; i < Points.Count; i++)
			{
				var point = Points[i];
				point.y = -point.y;
				Points[i] = point;
			}
			Invalidate();
		}

		public void MirrorZ()
		{
			if (Points == null)
				return;
			for (var i = 0; i < Points.Count; i++)
			{
				var point = Points[i];
				point.z = -point.z;
				Points[i] = point;
			}
			Invalidate();
		}

		public void MoveToLocalZero(bool keepWorldPosition)
		{
			if (!IsAnyPointAvailable)
				return;
			if (keepWorldPosition)
			{
				var matrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
				for (var i = 0; i < Points.Count; i++)
				{
					Points[i] = matrix.MultiplyPoint(Points[i]);
				}
			}
			transform.ResetTransformToLocalZero();
			Invalidate();
		}

		public void MoveToStart(bool keepWorldPosition)
		{
			if (!IsAnyPointAvailable)
				return;
			var pointPosition = GetPointLocalPosition(0);
			var matrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
			var newPosition = matrix.MultiplyPoint(pointPosition);
			if (keepWorldPosition)
			{
				matrix = Matrix4x4.TRS(-newPosition, Quaternion.identity, Vector3Tools.One) * matrix;
				for (var i = 0; i < Points.Count; i++)
				{
					Points[i] = matrix.MultiplyPoint(Points[i]);
				}
			}
			transform.SetLocalLocation(newPosition, Quaternion.identity, Vector3Tools.One);
			Invalidate();
		}

		public void MoveToEnd(bool keepWorldPosition)
		{
			if (!IsAnyPointAvailable)
				return;
			var pointPosition = GetPointLocalPosition(Points.Count - 1);
			var matrix = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
			var newPosition = matrix.MultiplyPoint(pointPosition);
			if (keepWorldPosition)
			{
				matrix = Matrix4x4.TRS(-newPosition, Quaternion.identity, Vector3Tools.One) * matrix;
				for (var i = 0; i < Points.Count; i++)
				{
					Points[i] = matrix.MultiplyPoint(Points[i]);
				}
			}
			transform.SetLocalLocation(newPosition, Quaternion.identity, Vector3Tools.One);
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
				GizmosTools.DrawPathLines(GetPointWorldPosition, Points?.Count ?? 0, Loop, DEBUG.UnselectedColor);
			}
		}

		private void OnDrawGizmosSelected()
		{
			GizmosTools.DrawPath(GetPointWorldPosition, Points?.Count ?? 0, Loop, true, DEBUG.PointColor, true, DEBUG.LineColor, AverageSegmentLength * DEBUG.PointSize, DEBUG.FirstPointSizeFactor);
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
			Invalidate();
		}

#endif

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Line));

		#endregion
	}

}

#endif
