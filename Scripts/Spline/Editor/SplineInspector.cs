using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

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
	}

	protected override void OnMovementDetected()
	{
		Me.Invalidate();
	}

	protected override void OnAfterDefaultInspectorGUI()
	{
		GUILayout.Space(15f);
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
		//var screenWidth = camera.pixelWidth;
		var screenHeight = camera.pixelHeight;
		var mousePosition = MouseSceneViewPosition;
		//Handles.BeginGUI();
		//GUI.Button(new Rect(mousePosition.x - 10, mousePosition.y - 10, 20, 20), "#");
		//Handles.EndGUI();
		float mouseVisibilityDistance = screenHeight / 4f;


		if (eventRawType == EventType.MouseUp)
			DraggingPointIndex = -1;

		// Point handles
		if (Me.RawPoints != null)
		{
			for (int i = 0; i < Me.RawPoints.Count; i++)
			{
				var point = ConvertLocalToWorldPosition(Me.RawPoints[i]);
				if (IsMouseCloseToWorldPointInScreenCoordinates(camera, point, mousePosition, mouseVisibilityDistance))
				{
					if (DraggingPointIndex >= 0 && DraggingPointIndex != i)
						continue;

					var newPoint = Handles.PositionHandle(point, Quaternion.identity);
					if (newPoint != point)
					{
						Me.RawPoints[i] = ConvertWorldToLocalPosition(newPoint);

						if (eventType == EventType.MouseDown ||
							eventType == EventType.MouseDrag ||
							eventType == EventType.MouseMove)
						{
							if (newPoint != point)
							{
								DraggingPointIndex = i;
							}
						}
					}
				}
			}
		}

		Handles.BeginGUI();
		var savedBackgroundColor = GUI.backgroundColor;

		// "Insert point" buttons
		if (Me.RawPoints != null && Me.RawPoints.Count > 1)
		{
			rect.width = SmallButtonSize;
			rect.height = SmallButtonSize;
			GUI.backgroundColor = InsertButtonBackgroundColor;

			var previous = ConvertLocalToWorldPosition(Me.RawPoints[0]);
			for (int i = 1; i < Me.RawPoints.Count; i++)
			{
				var current = ConvertLocalToWorldPosition(Me.RawPoints[i]);
				var center = current.Mid(previous);
				var screenPosition = camera.WorldToScreenPointWithReverseCheck(center);

				if (screenPosition.HasValue &&
					IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, mouseVisibilityDistance))
				{
					rect.x = screenPosition.Value.x - SmallButtonHalfSize;
					rect.y = screenHeight - screenPosition.Value.y - SmallButtonHalfSize;
					if (GUI.Button(rect, "+"))
					{
						Me.RawPoints.Insert(i, ConvertWorldToLocalPosition(center));
						break;
					}
				}

				previous = current;
			}
		}

		// "Add point to end" button
		if (Me.RawPoints != null && Me.RawPoints.Count > 0)
		{
			rect.width = MediumButtonSize;
			rect.height = MediumButtonSize;
			GUI.backgroundColor = InsertButtonBackgroundColor;

			var endingPoint = ConvertLocalToWorldPosition(Me.RawPoints[Me.RawPoints.Count - 1]);
			var cameraDistanceToEndingPoint = Vector3.Distance(camera.transform.position, endingPoint);
			var direction = Me.RawPoints.Count == 1
				? Vector3.forward
				: (endingPoint - ConvertLocalToWorldPosition(Me.RawPoints[Me.RawPoints.Count - 2])).normalized;

			var point = endingPoint + direction * (cameraDistanceToEndingPoint * 0.5f);
			var screenPosition = camera.WorldToScreenPointWithReverseCheck(point);

			if (screenPosition.HasValue)
			{
				rect.x = screenPosition.Value.x - MediumButtonHalfSize;
				rect.y = screenHeight - screenPosition.Value.y - MediumButtonHalfSize;
				if (GUI.Button(rect, "+"))
				{
					Me.RawPoints.Add(ConvertWorldToLocalPosition(point));
				}
			}
		}

		// "Remove point" buttons
		if (Me.RawPoints != null && Me.RawPoints.Count > 0)
		{
			rect.width = SmallButtonSize;
			rect.height = SmallButtonSize;
			GUI.backgroundColor = RemoveButtonBackgroundColor;

			for (int i = 0; i < Me.RawPoints.Count; i++)
			{
				var point = ConvertLocalToWorldPosition(Me.RawPoints[i]);
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
							Me.RawPoints.RemoveAt(i);
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

	private Vector3 ConvertWorldToLocalPosition(Vector3 point)
	{
		return Me.KeepDataInLocalCoordinates
			? Me.transform.InverseTransformPoint(point)
			: point;
	}

	private Vector3 ConvertLocalToWorldPosition(Vector3 point)
	{
		return Me.KeepDataInLocalCoordinates
			? Me.transform.TransformPoint(point)
			: point;
	}
}
