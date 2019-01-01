using UnityEngine;
using UnityEditor;
using Extenity.CameraToolbox;
using Extenity.IMGUIToolbox;
using Extenity.UnityEditorToolbox.Editor;

namespace Extenity.MathToolbox.Editor
{

	public abstract class LineInspectorBase<TLine> : ExtenityEditorBase<TLine> where TLine : Behaviour
	{
		#region Initialization

		protected override void OnEnableDerived()
		{
			IsAutoRepaintSceneViewEnabled = true;
			IsMovementDetectionEnabled = true;
		}

		#endregion

		#region Deinitialization

		protected override void OnDisableDerived()
		{
			// TODO: That did not work as expected. The second we hit the Start Editing button, OnDisableDerived is called for some reason.
			//Me.StopEditing();
		}

		#endregion

		#region Movement

		protected override void OnMovementDetected()
		{
			if (KeepDataInLocalCoordinates)
				return;

			if (MovementDetectionPreviousPosition.IsAnyNaN() ||
				MovementDetectionPreviousRotation.IsAnyNaN() ||
				MovementDetectionPreviousScale.IsAnyNaN())
				return;

			// Move all points
			{
				var previousTransformMatrix = new Matrix4x4();
				previousTransformMatrix.SetTRS(MovementDetectionPreviousPosition, MovementDetectionPreviousRotation, MovementDetectionPreviousScale);

				for (int i = 0; i < PointCount; i++)
				{
					TransformPointFromLocalToLocal(i, previousTransformMatrix.inverse, Me.transform.localToWorldMatrix);
				}
			}

			// Invalidate
			InvalidatePoints();
		}

		#endregion

		#region Inspector GUI

		protected void Draw_StartStopEditing()
		{
			GUILayout.BeginHorizontal();
			if (GUILayoutTools.Button("Start Editing", !IsEditing, BigButtonHeight))
			{
				StartEditing();
			}
			if (GUILayoutTools.Button("Stop Editing", IsEditing, BigButtonHeight))
			{
				StopEditing();
			}
			GUILayout.EndHorizontal();
		}

		protected void Draw_Operations_Mirror()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Mirror X", BigButtonHeight))
			{
				Undo.RecordObject(Me, "Line mirror X");
				MirrorX();
			}
			if (GUILayout.Button("Mirror Y", BigButtonHeight))
			{
				Undo.RecordObject(Me, "Line mirror Y");
				MirrorY();
			}
			if (GUILayout.Button("Mirror Z", BigButtonHeight))
			{
				Undo.RecordObject(Me, "Line mirror Z");
				MirrorZ();
			}
			GUILayout.EndHorizontal();
		}

		protected void Draw_Operations_Position()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Move To Zero", BigButtonHeight))
			{
				Undo.RecordObject(Me, "Move to zero");
				MoveToZero(true);
			}
			GUILayout.EndHorizontal();
		}

		protected void Draw_Data_Clipboard()
		{
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
		}

		protected void Draw_Data_General()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Clear Data", BigButtonHeight))
			{
				Undo.RecordObject(Me, "Clear data");
				ClearData();
			}
			if (GUILayout.Button("Invalidate", BigButtonHeight))
			{
				InvalidatePoints();
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Scene GUI

		private static readonly Color InsertButtonBackgroundColor = new Color(0.1f, 1f, 0.1f, 1f);
		private static readonly Color RemoveButtonBackgroundColor = new Color(1f, 0.6f, 0.6f, 1f);
		private const int SmallButtonSize = 20;
		private const int SmallButtonHalfSize = SmallButtonSize / 2;
		private const int MediumButtonSize = 26;
		private const int MediumButtonHalfSize = MediumButtonSize / 2;

		private int DraggingPointIndex = -1;

		protected void OnSceneGUI()
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
			if (IsEditing)
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
							if (IsPointListAvailable)
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
									for (int i = 0; i < PointCount; i++)
									{
										var point = ConvertLocalToWorldPosition(GetPointPosition(i));
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
									var currentPosition = ConvertLocalToWorldPosition(GetPointPosition(selectedPointIndex));
									GUIUtility.GetControlID(FocusType.Keyboard);
									var newPosition = Handles.PositionHandle(currentPosition, Quaternion.identity);
									if (newPosition != currentPosition)
									{
										Undo.RecordObject(Me, "Move line point");
										SetPoint(selectedPointIndex, ConvertWorldToLocalPosition(newPosition));

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
				if (IsPointListAvailable && PointCount > 1 && DraggingPointIndex < 0)
				{
					rect.width = SmallButtonSize;
					rect.height = SmallButtonSize;
					GUI.backgroundColor = InsertButtonBackgroundColor;

					var previous = ConvertLocalToWorldPosition(GetPointPosition(0));
					for (int i = 1; i < PointCount; i++)
					{
						var current = ConvertLocalToWorldPosition(GetPointPosition(i));
						var center = current.Mid(previous);
						var screenPosition = camera.WorldToScreenPointWithReverseCheck(center);

						if (screenPosition.HasValue &&
							IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, mouseVisibilityDistance))
						{
							rect.x = screenPosition.Value.x - SmallButtonHalfSize;
							rect.y = screenHeight - screenPosition.Value.y - SmallButtonHalfSize;
							if (GUI.Button(rect, "+"))
							{
								Undo.RecordObject(Me, "Insert line point");
								InsertPoint(i, ConvertWorldToLocalPosition(center));
								break;
							}
						}

						previous = current;
					}
				}

				// "Add point to end" button
				if (IsPointListAvailableAndNotEmpty && DraggingPointIndex < 0)
				{
					rect.width = MediumButtonSize;
					rect.height = MediumButtonSize;
					GUI.backgroundColor = InsertButtonBackgroundColor;

					var endingPoint = ConvertLocalToWorldPosition(GetPointPosition(PointCount - 1));
					var cameraDistanceToEndingPoint = Vector3.Distance(camera.transform.position, endingPoint);
					var direction = PointCount == 1
						? Vector3.forward
						: (endingPoint - ConvertLocalToWorldPosition(GetPointPosition(PointCount - 2))).normalized;

					var point = endingPoint + direction * (cameraDistanceToEndingPoint * 0.5f);
					var screenPosition = camera.WorldToScreenPointWithReverseCheck(point);

					if (screenPosition.HasValue)
					{
						rect.x = screenPosition.Value.x - MediumButtonHalfSize;
						rect.y = screenHeight - screenPosition.Value.y - MediumButtonHalfSize;
						if (GUI.Button(rect, "+"))
						{
							Undo.RecordObject(Me, "Add line point");
							AppendPoint(ConvertWorldToLocalPosition(point));
						}
					}
				}

				// "Remove point" buttons
				if (IsPointListAvailableAndNotEmpty && DraggingPointIndex < 0)
				{
					rect.width = SmallButtonSize;
					rect.height = SmallButtonSize;
					GUI.backgroundColor = RemoveButtonBackgroundColor;

					for (int i = 0; i < PointCount; i++)
					{
						var point = ConvertLocalToWorldPosition(GetPointPosition(i));
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
									Undo.RecordObject(Me, "Remove line point");
									RemovePoint(i);
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

		#endregion

		#region Generalization

		protected abstract bool IsEditing { get; }

		protected abstract bool IsPointListAvailable { get; }
		protected abstract bool IsPointListAvailableAndNotEmpty { get; }
		protected abstract int PointCount { get; }

		protected abstract bool KeepDataInLocalCoordinates { get; }

		protected abstract Vector3 GetPointPosition(int i);
		protected abstract void SetPoint(int i, Vector3 position);
		protected abstract void InsertPoint(int i, Vector3 position);
		protected abstract void AppendPoint(Vector3 position);
		protected abstract void RemovePoint(int i);
		protected abstract void ClearData();
		protected abstract void InvalidatePoints();

		protected abstract void MirrorX();
		protected abstract void MirrorY();
		protected abstract void MirrorZ();
		protected abstract void MoveToZero(bool keepWorldPosition);

		protected abstract void StartEditing();
		protected abstract void StopEditing();

		#endregion

		#region Space Conversions

		protected Vector3 ConvertWorldToLocalPosition(Vector3 point)
		{
			return KeepDataInLocalCoordinates
				? Me.transform.InverseTransformPoint(point)
				: point;
		}

		protected Vector3 ConvertLocalToWorldPosition(Vector3 point)
		{
			return KeepDataInLocalCoordinates
				? Me.transform.TransformPoint(point)
				: point;
		}

		protected abstract void TransformPointFromLocalToLocal(int pointIndex, Matrix4x4 currentMatrix, Matrix4x4 newMatrix);

		#endregion

		#region Clipboard

		protected abstract void CopyToClipboard();
		protected abstract void PasteClipboard();

		#endregion
	}

}
