using UnityEngine;
using UnityEditor;
using Extenity.CameraToolbox;
using Extenity.UnityEditorToolbox.Editor;

namespace Extenity.MathToolbox.Editor
{

	public abstract class LineInspectorBase<TLine> : ExtenityEditorBase<TLine> where TLine : Behaviour
	{
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

		#region Data

		protected abstract bool IsEditing { get; }

		protected abstract bool IsPointListAvailable { get; }
		protected abstract bool IsPointListAvailableAndNotEmpty { get; }
		protected abstract int PointCount { get; }

		protected abstract Vector3 GetPointPosition(int i);
		protected abstract void SetPoint(int i, Vector3 position);
		protected abstract void InsertPoint(int i, Vector3 position);
		protected abstract void AppendPoint(Vector3 position);
		protected abstract void RemovePoint(int i);
		protected abstract void InvalidatePoints();

		#endregion

		#region Local-World Conversion

		protected Vector3 ConvertWorldToLocalPosition(Vector3 point)
		{
			return point;
			// TODO: Implement KeepDataInLocalCoordinates. See 1798515712.
			//return Me.KeepDataInLocalCoordinates
			//	? Me.transform.InverseTransformPoint(point)
			//	: point;
		}

		protected Vector3 ConvertLocalToWorldPosition(Vector3 point)
		{
			return point;
			// TODO: Implement KeepDataInLocalCoordinates. See 1798515712.
			//return Me.KeepDataInLocalCoordinates
			//	? Me.transform.TransformPoint(point)
			//	: point;
		}

		#endregion
	}

}
