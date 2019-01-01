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

	[CustomEditor(typeof(OrientedLine))]
	public class OrientedLineInspector : LineInspectorBase<OrientedLine>
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
			if (Me.KeepDataInLocalCoordinates)
				return;

			if (MovementDetectionPreviousPosition.IsAnyNaN() ||
				MovementDetectionPreviousRotation.IsAnyNaN() ||
				MovementDetectionPreviousScale.IsAnyNaN())
				return;

			// Move all points
			{
				var previousTransformMatrix = new Matrix4x4();
				previousTransformMatrix.SetTRS(MovementDetectionPreviousPosition, MovementDetectionPreviousRotation, MovementDetectionPreviousScale);

				for (int i = 0; i < Points.Count; i++)
				{
					Points[i] = Points[i].TransformPointFromLocalToLocal(previousTransformMatrix.inverse, Me.transform);
				}
			}

			// Invalidate
			InvalidatePoints();
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(15f);

			// Edit
			{
				GUILayout.BeginVertical("Edit", EditorStyles.helpBox, GUILayout.Height(60f));
				GUILayout.FlexibleSpace();

				GUILayout.BeginHorizontal();
				if (GUILayoutTools.Button("Start Editing", !Me.IsEditing, BigButtonHeight))
				{
					Me.StartEditing();
				}
				if (GUILayoutTools.Button("Stop Editing", Me.IsEditing, BigButtonHeight))
				{
					Me.StopEditing();
				}
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			GUILayout.Space(15f);

			// Operations
			{
				GUILayout.BeginVertical("Operations", EditorStyles.helpBox, GUILayout.Height(140f));
				GUILayout.FlexibleSpace();

				// Mirror
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Mirror X", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Line mirror X");
					Me.MirrorX();
				}
				if (GUILayout.Button("Mirror Y", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Line mirror Y");
					Me.MirrorY();
				}
				if (GUILayout.Button("Mirror Z", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Line mirror Z");
					Me.MirrorZ();
				}
				GUILayout.EndHorizontal();

				// Position
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Move To Zero", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Move to zero");
					Me.MoveToZero(true);
				}
				GUILayout.EndHorizontal();

				// Orientations
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Normalize All Orientations", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Normalize all orientations");
					Me.NormalizeAllOrientations();
				}
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			GUILayout.Space(15f);

			// Data
			{
				GUILayout.BeginVertical("Data", EditorStyles.helpBox, GUILayout.Height(100f));
				GUILayout.FlexibleSpace();

				// Clipboard
				GUILayout.BeginHorizontal();
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
				GUILayout.EndHorizontal();

				// Clear / Invalidate
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Clear Data", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Clear data");
					Me.ClearData();
				}
				if (GUILayout.Button("Invalidate", BigButtonHeight))
				{
					InvalidatePoints();
				}
				GUILayout.EndHorizontal();

				GUILayout.EndVertical();
			}

			GUILayout.Space(15f);
		}

		#region Data

		private List<OrientedPoint> Points => Me.Points;

		protected override bool IsEditing => Me.IsEditing;

		protected override bool IsPointListAvailable => Points != null;
		protected override bool IsPointListAvailableAndNotEmpty => Points != null && Points.Count > 0;
		protected override int PointCount => Points.Count;

		protected override bool KeepDataInLocalCoordinates => Me.KeepDataInLocalCoordinates;

		protected override Vector3 GetPointPosition(int i)
		{
			return Points[i].Position;
		}

		protected override void SetPoint(int i, Vector3 position)
		{
			Points[i] = Points[i].WithPosition(position);
		}

		protected override void InsertPoint(int i, Vector3 position)
		{
			var midOrientation = Points[i].MidOrientation(Points[i - 1]);
			Points.Insert(i, new OrientedPoint(position, midOrientation));
		}

		protected override void AppendPoint(Vector3 position)
		{
			Points.Add(new OrientedPoint(position, Points[Points.Count - 1].Orientation));
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
				stringBuilder.AppendLine(point.Position.x + " " + point.Position.y + " " + point.Position.z + " " + point.Orientation.x + " " + point.Orientation.y + " " + point.Orientation.z);
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
					var position = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
					var orientation = new Vector3(float.Parse(split[3]), float.Parse(split[4]), float.Parse(split[5]));
					Points.Add(new OrientedPoint(position, orientation));
				}
			}
		}

		#endregion
	}

}
