using System;
using System.Collections.Generic;
using System.Text;
using Extenity.ApplicationToolbox;
using UnityEngine;
using UnityEditor;
using Extenity.CameraToolbox;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox;
using Extenity.UnityEditorToolbox.Editor;

namespace Extenity.MathToolbox.Editor
{

	[CustomEditor(typeof(Spline))]
	public class SplineInspector : ExtenityEditorBase<Spline>
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
					var point = Points[i];
					point = previousTransformMatrix.MultiplyPoint(point);
					point = Me.transform.TransformPoint(point);
					Points[i] = point;
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
			if (Me.IsEditing)
			{
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
							if (Points != null)
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
									for (int i = 0; i < Points.Count; i++)
									{
										var point = ConvertLocalToWorldPosition(Points[i]);
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
									var currentPosition = ConvertLocalToWorldPosition(Points[selectedPointIndex]);
									GUIUtility.GetControlID(FocusType.Keyboard);
									var newPosition = Handles.PositionHandle(currentPosition, Quaternion.identity);
									if (newPosition != currentPosition)
									{
										Points[selectedPointIndex] = ConvertWorldToLocalPosition(newPosition);

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
				if (Points != null && Points.Count > 1 && DraggingPointIndex < 0)
				{
					rect.width = SmallButtonSize;
					rect.height = SmallButtonSize;
					GUI.backgroundColor = InsertButtonBackgroundColor;

					var previous = ConvertLocalToWorldPosition(Points[0]);
					for (int i = 1; i < Points.Count; i++)
					{
						var current = ConvertLocalToWorldPosition(Points[i]);
						var center = current.Mid(previous);
						var screenPosition = camera.WorldToScreenPointWithReverseCheck(center);

						if (screenPosition.HasValue &&
							IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, mouseVisibilityDistance))
						{
							rect.x = screenPosition.Value.x - SmallButtonHalfSize;
							rect.y = screenHeight - screenPosition.Value.y - SmallButtonHalfSize;
							if (GUI.Button(rect, "+"))
							{
								Points.Insert(i, ConvertWorldToLocalPosition(center));
								break;
							}
						}

						previous = current;
					}
				}

				// "Add point to end" button
				if (Points != null && Points.Count > 0 && DraggingPointIndex < 0)
				{
					rect.width = MediumButtonSize;
					rect.height = MediumButtonSize;
					GUI.backgroundColor = InsertButtonBackgroundColor;

					var endingPoint = ConvertLocalToWorldPosition(Points[Points.Count - 1]);
					var cameraDistanceToEndingPoint = Vector3.Distance(camera.transform.position, endingPoint);
					var direction = Points.Count == 1
						? Vector3.forward
						: (endingPoint - ConvertLocalToWorldPosition(Points[Points.Count - 2])).normalized;

					var point = endingPoint + direction * (cameraDistanceToEndingPoint * 0.5f);
					var screenPosition = camera.WorldToScreenPointWithReverseCheck(point);

					if (screenPosition.HasValue)
					{
						rect.x = screenPosition.Value.x - MediumButtonHalfSize;
						rect.y = screenHeight - screenPosition.Value.y - MediumButtonHalfSize;
						if (GUI.Button(rect, "+"))
						{
							Points.Add(ConvertWorldToLocalPosition(point));
						}
					}
				}

				// "Remove point" buttons
				if (Points != null && Points.Count > 0 && DraggingPointIndex < 0)
				{
					rect.width = SmallButtonSize;
					rect.height = SmallButtonSize;
					GUI.backgroundColor = RemoveButtonBackgroundColor;

					for (int i = 0; i < Points.Count; i++)
					{
						var point = ConvertLocalToWorldPosition(Points[i]);
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
									Points.RemoveAt(i);
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
					InvalidatePoints();
				}
			}
		}

		#region Data

		private List<Vector3> Points => Me.RawPoints;

		private void InvalidatePoints()
		{
			Me.InvalidateRawLine();
		}

		#endregion

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
