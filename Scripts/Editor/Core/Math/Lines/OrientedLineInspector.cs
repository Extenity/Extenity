using System;
using System.Text;
using Extenity.ApplicationToolbox;
using UnityEngine;
using UnityEditor;
using Extenity.CameraToolbox;
using Extenity.DataToolbox;
using Extenity.UnityEditorToolbox.Editor;

namespace Extenity.MathToolbox.Editor
{

	[CustomEditor(typeof(OrientedLine))]
	public class OrientedLineInspector : ExtenityEditorBase<OrientedLine>
	{
		protected override void OnEnableDerived()
		{
			IsAutoRepaintSceneViewEnabled = true;
			IsMovementDetectionEnabled = true;
		}

		protected override void OnDisableDerived()
		{
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

				for (int i = 0; i < Me.Points.Count; i++)
				{
					var point = Me.Points[i].Position;
					point = previousTransformMatrix.MultiplyPoint(point);
					point = Me.transform.TransformPoint(point);
					Me.Points[i] = Me.Points[i].WithPosition(point);
				}
			}

			// Invalidate
			Me.Invalidate();
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(15f);
			GUILayout.BeginHorizontal();

			// Invalidate
			{
				if (GUILayout.Button("Normalize All Orientations", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Normalize all orientations");
					Me.NormalizeAllOrientations();
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			// Invalidate
			{
				if (GUILayout.Button("Copy To Clipboard", BigButtonHeight))
				{
					CopyToClipboard();
				}
				if (GUILayout.Button("Paste", BigButtonHeight))
				{
					Undo.RecordObject(Me, "Paste data");
					PasteClipboard();
					Me.Invalidate();
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
					Me.Invalidate();
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(15f);
		}

		private static readonly Color InsertButtonBackgroundColor = new Color(0.1f, 1f, 0.1f, 1f);
		private static readonly Color RemoveButtonBackgroundColor = new Color(1f, 0.6f, 0.6f, 1f);
		private static readonly int SmallButtonSize = 20;
		private static readonly int SmallButtonHalfSize = SmallButtonSize / 2;
		private static readonly int MediumButtonSize = 26;
		private static readonly int MediumButtonHalfSize = MediumButtonSize / 2;

		private static int DraggingPointIndex = -1;

		private void OnSceneGUI()
		{
			var eventType = Event.current.type;
			var eventRawType = Event.current.rawType;
			var rect = new Rect();
			var camera = SceneView.lastActiveSceneView.camera;
			var screenWidth = camera.pixelWidth;
			var screenHeight = camera.pixelHeight;
			var mousePosition = MouseSceneViewPosition;
			float mouseVisibilityDistance = Mathf.Min(screenWidth, screenHeight) / 4f;

			if (eventRawType == EventType.MouseUp)
			{
				DraggingPointIndex = -1;
			}

			// Point handles
			switch (eventType)
			{
				case EventType.MouseUp:
				case EventType.MouseDown:
				case EventType.MouseMove:
				case EventType.MouseDrag:
				case EventType.KeyDown:
				case EventType.KeyUp:
				case EventType.ScrollWheel:
				case EventType.Repaint:
				case EventType.Layout:
				case EventType.DragUpdated:
				case EventType.DragPerform:
				case EventType.DragExited:
				case EventType.Ignore:
				case EventType.Used:
				case EventType.ValidateCommand:
				case EventType.ExecuteCommand:
				case EventType.ContextClick:
					{
						if (Me.Points != null)
						{
							int selectedPointIndex = -1;

							if (DraggingPointIndex >= 0)
							{
								// Select currently dragged point
								selectedPointIndex = DraggingPointIndex;
							}
							else
							{
								// Find closest point
								float closestPointDistanceSqr = float.MaxValue;
								for (int i = 0; i < Me.Points.Count; i++)
								{
									var point = ConvertLocalToWorldPosition(Me.Points[i].Position);
									var diff = GetDifferenceBetweenMousePositionAndWorldPoint(camera, point, mousePosition, mouseVisibilityDistance);
									var distanceSqr = diff.sqrMagnitude;
									if (closestPointDistanceSqr > distanceSqr)
									{
										closestPointDistanceSqr = distanceSqr;
										selectedPointIndex = i;
									}
								}
							}

							if (selectedPointIndex >= 0)
							{
								var currentPosition = ConvertLocalToWorldPosition(Me.Points[selectedPointIndex].Position);
								GUIUtility.GetControlID(FocusType.Keyboard);
								var newPosition = Handles.PositionHandle(currentPosition, Quaternion.identity);
								if (newPosition != currentPosition)
								{
									Me.Points[selectedPointIndex] = Me.Points[selectedPointIndex].WithPosition(ConvertWorldToLocalPosition(newPosition));

									if (eventType == EventType.MouseDown ||
										eventType == EventType.MouseDrag ||
										eventType == EventType.MouseMove)
									{
										DraggingPointIndex = selectedPointIndex;
									}
								}
							}
						}
					}
					break;
					//default:
					//	throw new ArgumentOutOfRangeException("eventType", eventType, "Event type '" + eventType + "' is not implemented.");
			}

			Handles.BeginGUI();
			var savedBackgroundColor = GUI.backgroundColor;

			// "Insert point" buttons
			if (Me.Points != null && Me.Points.Count > 1 && DraggingPointIndex < 0)
			{
				rect.width = SmallButtonSize;
				rect.height = SmallButtonSize;
				GUI.backgroundColor = InsertButtonBackgroundColor;

				var previous = ConvertLocalToWorldPosition(Me.Points[0].Position);
				for (int i = 1; i < Me.Points.Count; i++)
				{
					var current = ConvertLocalToWorldPosition(Me.Points[i].Position);
					var center = current.Mid(previous);
					var screenPosition = camera.WorldToScreenPointWithReverseCheck(center);

					if (screenPosition.HasValue &&
						IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, mouseVisibilityDistance))
					{
						rect.x = screenPosition.Value.x - SmallButtonHalfSize;
						rect.y = screenHeight - screenPosition.Value.y - SmallButtonHalfSize;
						if (GUI.Button(rect, "+"))
						{
							Me.Points.Insert(i, Me.Points[i].Mid(Me.Points[i - 1]));
							break;
						}
					}

					previous = current;
				}
			}

			// "Add point to end" button
			if (Me.Points != null && Me.Points.Count > 0 && DraggingPointIndex < 0)
			{
				rect.width = MediumButtonSize;
				rect.height = MediumButtonSize;
				GUI.backgroundColor = InsertButtonBackgroundColor;

				var endingPoint = ConvertLocalToWorldPosition(Me.Points[Me.Points.Count - 1].Position);
				var cameraDistanceToEndingPoint = Vector3.Distance(camera.transform.position, endingPoint);
				var direction = Me.Points.Count == 1
					? Vector3.forward
					: (endingPoint - ConvertLocalToWorldPosition(Me.Points[Me.Points.Count - 2].Position)).normalized;

				var point = endingPoint + direction * (cameraDistanceToEndingPoint * 0.5f);
				var screenPosition = camera.WorldToScreenPointWithReverseCheck(point);

				if (screenPosition.HasValue)
				{
					rect.x = screenPosition.Value.x - MediumButtonHalfSize;
					rect.y = screenHeight - screenPosition.Value.y - MediumButtonHalfSize;
					if (GUI.Button(rect, "+"))
					{
						Me.Points.Add(new OrientedPoint(ConvertWorldToLocalPosition(point), Me.Points[Me.Points.Count - 1].Orientation));
					}
				}
			}

			// "Remove point" buttons
			if (Me.Points != null && Me.Points.Count > 0 && DraggingPointIndex < 0)
			{
				rect.width = SmallButtonSize;
				rect.height = SmallButtonSize;
				GUI.backgroundColor = RemoveButtonBackgroundColor;

				for (int i = 0; i < Me.Points.Count; i++)
				{
					var point = ConvertLocalToWorldPosition(Me.Points[i].Position);
					var screenPosition = camera.WorldToScreenPointWithReverseCheck(point);
					if (screenPosition.HasValue)
					{
						screenPosition -= new Vector3(0f, 30f, 0f);

						if (IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, mouseVisibilityDistance))
						{
							rect.x = screenPosition.Value.x - SmallButtonHalfSize;
							rect.y = screenHeight - screenPosition.Value.y - SmallButtonHalfSize;
							if (GUI.Button(rect, "-"))
							{
								Me.Points.RemoveAt(i);
								break;
							}
						}
					}
				}
			}

			GUI.backgroundColor = savedBackgroundColor;
			Handles.EndGUI();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
				// TODO: Not cool to always invalidate everything. But it's a quick and robust solution for now.
				Me.Invalidate();
			}
		}

		#region Local-World Conversion

		private Vector3 ConvertWorldToLocalPosition(Vector3 point)
		{
			return point;
			// TODO: Implement KeepDataInLocalCoordinates. See 1798515712.
			//return Me.KeepDataInLocalCoordinates
			//	? Me.transform.InverseTransformPoint(point)
			//	: point;
		}

		private Vector3 ConvertLocalToWorldPosition(Vector3 point)
		{
			return point;
			// TODO: Implement KeepDataInLocalCoordinates. See 1798515712.
			//return Me.KeepDataInLocalCoordinates
			//	? Me.transform.TransformPoint(point)
			//	: point;
		}

		#endregion

		#region Clipboard

		private void CopyToClipboard()
		{
			if (Me.Points.IsNullOrEmpty())
				return;

			var stringBuilder = new StringBuilder();
			for (int i = 0; i < Me.Points.Count; i++)
			{
				var point = Me.Points[i];
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
					Me.Points.Add(new OrientedPoint(position, orientation));
				}
			}
		}

		#endregion
	}

}
