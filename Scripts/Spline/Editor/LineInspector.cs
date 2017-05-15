using System;
using UnityEngine;
using UnityEditor;
using System.Text;
using Extenity.CameraToolbox;
using Extenity.OperatingSystem;

namespace Extenity.GeometryToolbox
{

	[CustomEditor(typeof(Line))]
	public class LineInspector : ExtenityEditorBase<Line>
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
					var point = Me.Points[i];
					point = previousTransformMatrix.MultiplyPoint(point);
					point = Me.transform.TransformPoint(point);
					Me.Points[i] = point;
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
				if (GUILayout.Button("Copy To Clipboard", BigButtonHeight))
				{
					CopyToClipboard();
				}
				if (GUILayout.Button("Paste", BigButtonHeight))
				{
					PasteClipboard();
				}
			}

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();

			// Invalidate
			{
				if (GUILayout.Button("Clear Data", BigButtonHeight))
				{
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
									var point = Me.GetPoint(i);
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
								var currentPosition = Me.GetPoint(selectedPointIndex);
								GUIUtility.GetControlID(FocusType.Keyboard);
								var newPosition = Handles.PositionHandle(currentPosition, Quaternion.identity);
								if (newPosition != currentPosition)
								{
									Me.Points[selectedPointIndex] = newPosition;

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
				default:
					throw new ArgumentOutOfRangeException();
			}

			Handles.BeginGUI();
			var savedBackgroundColor = GUI.backgroundColor;

			// "Insert point" buttons
			if (Me.Points != null && Me.Points.Count > 1 && DraggingPointIndex < 0)
			{
				rect.width = SmallButtonSize;
				rect.height = SmallButtonSize;
				GUI.backgroundColor = InsertButtonBackgroundColor;

				var previous = Me.GetPoint(0);
				for (int i = 1; i < Me.Points.Count; i++)
				{
					var current = Me.GetPoint(i);
					var center = current.Mid(previous);
					var screenPosition = camera.WorldToScreenPointWithReverseCheck(center);

					if (screenPosition.HasValue &&
						IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, mouseVisibilityDistance))
					{
						rect.x = screenPosition.Value.x - SmallButtonHalfSize;
						rect.y = screenHeight - screenPosition.Value.y - SmallButtonHalfSize;
						if (GUI.Button(rect, "+"))
						{
							Me.Points.Insert(i, center);
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

				var endingPoint = Me.GetPoint(Me.Points.Count - 1);
				var cameraDistanceToEndingPoint = Vector3.Distance(camera.transform.position, endingPoint);
				var direction = Me.Points.Count == 1
					? Vector3.forward
					: (endingPoint - Me.GetPoint(Me.Points.Count - 2)).normalized;

				var point = endingPoint + direction * (cameraDistanceToEndingPoint * 0.5f);
				var screenPosition = camera.WorldToScreenPointWithReverseCheck(point);

				if (screenPosition.HasValue)
				{
					rect.x = screenPosition.Value.x - MediumButtonHalfSize;
					rect.y = screenHeight - screenPosition.Value.y - MediumButtonHalfSize;
					if (GUI.Button(rect, "+"))
					{
						Me.Points.Add(point);
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
					var point = Me.GetPoint(i);
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
				Me.Invalidate();
			}
		}

		#region Clipboard

		private void CopyToClipboard()
		{
			if (Me.Points.IsNullOrEmpty())
				return;

			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < Me.Points.Count; i++)
			{
				var point = Me.Points[i];
				stringBuilder.AppendLine(point.x + " " + point.y + " " + point.z);
			}

			Clipboard.SetClipboardText(stringBuilder.ToString());
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
					Me.Points.Add(point);
				}
			}
		}

		#endregion
	}

}
