using System;
using System.Collections.Generic;
using System.Text;
using Extenity.ApplicationToolbox;
using UnityEngine;
using UnityEditor;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;

namespace Extenity.MathToolbox.Editor
{

	[CustomEditor(typeof(OrientedLine))]
	public class OrientedLineInspector : LineInspectorBase<OrientedLine>
	{
		#region Inspector GUI

		protected override void DrawPanel_Edit()
		{
			Draw_StartStopEditing();
		}

		protected override void DrawPanel_Operations()
		{
			Draw_Operations_Mirror();
			Draw_Operations_Position();

			// Orientations
			GUILayout.BeginHorizontal();
			var action = "Normalize All Orientations";
			if (GUILayoutTools.Button(action, IsPointListAvailableAndNotEmpty, BigButtonHeight))
			{
				Undo.RecordObject(Me, action);
				Me.NormalizeAllOrientations();
			}
			GUILayout.EndHorizontal();
		}

		protected override void DrawPanel_Data()
		{
			Draw_Data_Clipboard();
			Draw_Data_General();
		}

		#endregion

		#region Generalization

		private List<OrientedPoint> Points => Me.Points;

		protected override bool IsEditing => Me.IsEditing;

		protected override bool IsPointListAvailable => Points != null;
		protected override bool IsPointListAvailableAndNotEmpty => Points != null && Points.Count > 0;
		protected override int PointCount => Points.Count;

		protected override bool KeepDataInLocalCoordinates => Me.KeepDataInLocalCoordinates;

		protected override Vector3 GetPointPosition(int i) { return Points[i].Position; }
		protected override void SetPoint(int i, Vector3 position) { Points[i] = Points[i].WithPosition(position); }
		protected override void InsertPoint(int i, Vector3 position) { var midOrientation = Points[i].MidOrientation(Points[i - 1]); Points.Insert(i, new OrientedPoint(position, midOrientation)); }
		protected override void AppendPoint(Vector3 position) { Points.Add(new OrientedPoint(position, Points[Points.Count - 1].Orientation)); }
		protected override void RemovePoint(int i) { Points.RemoveAt(i); }
		protected override void ClearData() { Me.ClearData(); }
		protected override void InvalidatePoints() { Me.Invalidate(); }

		protected override void MirrorX() { Me.MirrorX(); }
		protected override void MirrorY() { Me.MirrorY(); }
		protected override void MirrorZ() { Me.MirrorZ(); }
		protected override void MoveToLocalZero(bool keepWorldPosition) { Me.MoveToLocalZero(keepWorldPosition); }
		protected override void MoveToStart(bool keepWorldPosition) { Me.MoveToStart(keepWorldPosition); }
		protected override void MoveToEnd(bool keepWorldPosition) { Me.MoveToEnd(keepWorldPosition); }

		protected override void StartEditing() { Me.StartEditing(); }
		protected override void StopEditing() { Me.StopEditing(); }

		#endregion

		#region Space Conversions

		protected override void TransformPointFromLocalToLocal(int pointIndex, Matrix4x4 currentMatrix, Matrix4x4 newMatrix)
		{
			Points[pointIndex] = Points[pointIndex].TransformPointFromLocalToLocal(currentMatrix, newMatrix);
		}

		#endregion

		#region Clipboard

		protected override void CopyToClipboard()
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

		protected override void PasteClipboard()
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
