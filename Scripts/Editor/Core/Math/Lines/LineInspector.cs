using System;
using System.Collections.Generic;
using System.Text;
using Extenity.ApplicationToolbox;
using UnityEngine;
using UnityEditor;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox;

namespace Extenity.MathToolbox.Editor
{

	[CustomEditor(typeof(Line))]
	public class LineInspector : LineInspectorBase<Line>
	{
		protected override void OnEnableDerived()
		{
			IsAutoRepaintSceneViewEnabled = true;
			IsMovementDetectionEnabled = true;
		}

		protected override void OnDisableDerived()
		{
			// TODO: That did not work as expected. The second we hit the Start Editing button, OnDisableDerived is called for some reason.
			//Me.StopEditing();
		}

		protected override void OnMovementDetected()
		{
			if (MovementDetectionPreviousPosition.IsAnyNaN() ||
				MovementDetectionPreviousRotation.IsAnyNaN() ||
				MovementDetectionPreviousScale.IsAnyNaN())
				return;

			// Move all points
			{
				var previousTransformMatrix = new Matrix4x4();
				previousTransformMatrix.SetTRS(MovementDetectionPreviousPosition, MovementDetectionPreviousRotation, MovementDetectionPreviousScale);
				previousTransformMatrix = previousTransformMatrix.inverse;

				for (int i = 0; i < Points.Count; i++)
				{
					var position = Points[i];
					position = previousTransformMatrix.MultiplyPoint(position);
					position = Me.transform.TransformPoint(position);
					Points[i] = position;
				}
			}

			// Invalidate
			InvalidatePoints();
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(15f);

			GUILayout.BeginVertical("Edit", EditorStyles.helpBox, GUILayout.Height(60f));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();

			// Edit
			{
				if (GUILayoutTools.Button("Start Editing", !Me.IsEditing, BigButtonHeight))
				{
					Me.StartEditing();
				}
				if (GUILayoutTools.Button("Stop Editing", Me.IsEditing, BigButtonHeight))
				{
					Me.StopEditing();
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.Space(15f);

			GUILayout.BeginVertical("Data", EditorStyles.helpBox, GUILayout.Height(100f));
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();

			// Clipboard
			{
				if (GUILayout.Button("Copy To Clipboard", BigButtonHeight))
				{
					CopyToClipboard();
				}
				if (GUILayout.Button("Paste", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Paste data");
					PasteClipboard();
					InvalidatePoints();
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			// Invalidate
			{
				if (GUILayout.Button("Clear Data", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Clear data");
					Me.ClearData();
				}
				if (GUILayout.Button("Invalidate", BigButtonHeight))
				{
					InvalidatePoints();
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			GUILayout.Space(15f);
		}

		#region Data

		private List<Vector3> Points => Me.Points;

		protected override bool IsEditing => Me.IsEditing;
		protected override bool IsPointListAvailable => Points != null;
		protected override bool IsPointListAvailableAndNotEmpty => Points != null && Points.Count > 0;
		protected override int PointCount => Points.Count;

		protected override Vector3 GetPointPosition(int i)
		{
			return Points[i];
		}

		protected override void SetPoint(int i, Vector3 position)
		{
			Points[i] = position;
		}

		protected override void InsertPoint(int i, Vector3 position)
		{
			Points.Insert(i, position);
		}

		protected override void AppendPoint(Vector3 position)
		{
			Points.Add(position);
		}

		protected override void RemovePoint(int i)
		{
			Points.RemoveAt(i);
		}

		protected override void InvalidatePoints()
		{
			Me.Invalidate();
		}

		#endregion

		#region Clipboard

		private void CopyToClipboard()
		{
			if (Points.IsNullOrEmpty())
				return;

			var stringBuilder = new StringBuilder();
			for (int i = 0; i < Points.Count; i++)
			{
				var point = Points[i];
				stringBuilder.AppendLine(point.x + " " + point.y + " " + point.z);
			}

			Clipboard.SetClipboardText(stringBuilder.ToString(), false);
		}

		private void PasteClipboard()
		{
			var text = Clipboard.GetClipboardText();
			if (!string.IsNullOrEmpty(text))
			{
				var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
				for (int iLine = 0; iLine < lines.Length; iLine++)
				{
					var line = lines[iLine];
					var split = line.Split(' ');
					var point = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
					Points.Add(point);
				}
			}
		}

		#endregion
	}

}
