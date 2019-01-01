using System;
using System.Collections.Generic;
using System.Text;
using Extenity.ApplicationToolbox;
using UnityEngine;
using UnityEditor;
using Extenity.DataToolbox;

namespace Extenity.MathToolbox.Editor
{

	[CustomEditor(typeof(Line))]
	public class LineInspector : LineInspectorBase<Line>
	{
		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(15f);

			// Edit
			{
				GUILayout.BeginVertical("Edit", EditorStyles.helpBox, GUILayout.Height(60f));
				GUILayout.FlexibleSpace();

				Draw_StartStopEditing();

				GUILayout.EndVertical();
			}

			GUILayout.Space(15f);

			// Operations
			{
				GUILayout.BeginVertical("Operations", EditorStyles.helpBox, GUILayout.Height(100f));
				GUILayout.FlexibleSpace();

				Draw_Operations_Mirror();
				Draw_Operations_Position();

				GUILayout.EndVertical();
			}

			GUILayout.Space(15f);

			// Data
			{
				GUILayout.BeginVertical("Data", EditorStyles.helpBox, GUILayout.Height(100f));
				GUILayout.FlexibleSpace();

				Draw_Data_Clipboard();
				Draw_Data_General();

				GUILayout.EndVertical();
			}

			GUILayout.Space(15f);
		}

		#region Data

		private List<Vector3> Points => Me.Points;

		protected override bool IsEditing => Me.IsEditing;

		protected override bool IsPointListAvailable => Points != null;
		protected override bool IsPointListAvailableAndNotEmpty => Points != null && Points.Count > 0;
		protected override int PointCount => Points.Count;

		protected override bool KeepDataInLocalCoordinates => Me.KeepDataInLocalCoordinates;

		protected override Vector3 GetPointPosition(int i) { return Points[i]; }
		protected override void SetPoint(int i, Vector3 position) { Points[i] = position; }
		protected override void InsertPoint(int i, Vector3 position) { Points.Insert(i, position); }
		protected override void AppendPoint(Vector3 position) { Points.Add(position); }
		protected override void RemovePoint(int i) { Points.RemoveAt(i); }
		protected override void ClearData() { Me.ClearData(); }
		protected override void InvalidatePoints() { Me.Invalidate(); }

		protected override void MirrorX() { Me.MirrorX(); }
		protected override void MirrorY() { Me.MirrorY(); }
		protected override void MirrorZ() { Me.MirrorZ(); }
		protected override void MoveToZero(bool keepWorldPosition) { Me.MoveToZero(keepWorldPosition); }

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
				stringBuilder.AppendLine(point.x + " " + point.y + " " + point.z);
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
					var point = new Vector3(float.Parse(split[0]), float.Parse(split[1]), float.Parse(split[2]));
					Points.Add(point);
				}
			}
		}

		#endregion
	}

}
