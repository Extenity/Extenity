using System;
using Extenity.MathToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.SnappaTool.Editor
{

	public class Snappa : ExtenityEditorWindowBase
	{
		#region Configuration

		private const float GizmoSize = 1f;
		private const float SpeedModifierMovementFactor = 5f;

		private readonly Key[] KeyScheme_LeftHand =
		{
			new Key{ KeyCode = KeyCode.S, Action = KeyAction.Forward },
			new Key{ KeyCode = KeyCode.X, Action = KeyAction.Backward },
			new Key{ KeyCode = KeyCode.Z, Action = KeyAction.Left },
			new Key{ KeyCode = KeyCode.C, Action = KeyAction.Right },
			new Key{ KeyCode = KeyCode.D, Action = KeyAction.Up },
			new Key{ KeyCode = KeyCode.A, Action = KeyAction.Down },
		};

		//private readonly Key[] KeyScheme_Keypad =
		//{
		//	new Key{ KeyCode = KeyCode.Keypad8, Action = KeyAction.Forward },
		//	new Key{ KeyCode = KeyCode.Keypad5, Action = KeyAction.Backward },
		//	new Key{ KeyCode = KeyCode.Keypad4, Action = KeyAction.Left },
		//	new Key{ KeyCode = KeyCode.Keypad6, Action = KeyAction.Right },
		//	new Key{ KeyCode = KeyCode.Keypad9, Action = KeyAction.Up },
		//	new Key{ KeyCode = KeyCode.Keypad3, Action = KeyAction.Down },
		//};

		//private readonly Key[] KeyScheme_Arrows =
		//{
		//	new Key{ KeyCode = KeyCode.UpArrow, Action = KeyAction.Forward },
		//	new Key{ KeyCode = KeyCode.DownArrow, Action = KeyAction.Backward },
		//	new Key{ KeyCode = KeyCode.LeftArrow, Action = KeyAction.Left },
		//	new Key{ KeyCode = KeyCode.RightArrow, Action = KeyAction.Right },
		//	new Key{ KeyCode = KeyCode.Keypad1, Action = KeyAction.Up },
		//	new Key{ KeyCode = KeyCode.Keypad0, Action = KeyAction.Down },
		//};

		private readonly ArrowColors NormalArrowColors = new ArrowColors()
		{
			Front = new Color(0.5f, 0.7f, 0.5f, 0.5f),
			LeftRight = new Color(0.5f, 0.5f, 0.5f, 0.3f),
			Back = new Color(0.5f, 0.5f, 0.5f, 0.1f),
		};
		private readonly ArrowColors RedArrowColors = new ArrowColors()
		{
			Front = new Color(0.9f, 0.4f, 0.4f, 0.5f),
			LeftRight = new Color(0.9f, 0.5f, 0.5f, 0.3f),
			Back = new Color(0.9f, 0.5f, 0.5f, 0.1f),
		};

		private const float FrontArrowScale = 0.3f;
		private const float LeftRightArrowScale = 0.25f;
		private const float BackArrowScale = 0.2f;

		#endregion

		#region Initialization

		[MenuItem("Edit/Snappa", false, 1000)] // Just below Unity's "Snap Settings"
		private static void ShowWindow()
		{
			var window = GetWindow<Snappa>();
			window.Show();
		}

		private void OnEnable()
		{
			SetTitleAndIcon("Snappa", SnappaIcons.Texture_ArrowStraight);
			minSize = new Vector2(200f, 50f);

			InitializeKeyboard();
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
			SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		#endregion

		#region GUI - Window

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);
			IsEnabled = GUILayout.Toggle(IsEnabled, "Enabled", "Button", GUILayout.Width(100f), GUILayout.Height(30f));

			if (GUI.changed)
			{
				SceneView.RepaintAll();
			}
		}

		#endregion

		#region GUI - Scene

		private bool IsMouseDown;

		private void OnSceneGUI(SceneView sceneview)
		{
			var currentEvent = Event.current;
			var currentEventType = currentEvent.type;

			// Keep track of mouse events
			switch (currentEventType)
			{
				case EventType.MouseDown:
					IsMouseDown = true;
					break;
				case EventType.MouseUp:
					IsMouseDown = false;
					break;
			}

			if (!IsEnabled)
				return;

			// Get SceneView camera
			if (SceneView.currentDrawingSceneView == null || SceneView.currentDrawingSceneView.camera == null)
				return;
			var camera = SceneView.currentDrawingSceneView.camera;
			var cameraTransform = camera.transform;

			// Get selected object
			if (Selection.objects.Length == 0)
				return;
			if (Selection.objects.Length > 1)
				return;
			var selectedObject = Selection.activeTransform;
			if (selectedObject == null || !selectedObject.gameObject.scene.isLoaded)
				return;

			// Calculate gizmo position and rotation
			//var gizmoPosition = cameraTransform.position + cameraTransform.forward * Distance;
			var gizmoPosition = selectedObject.position;
			var gizmoRotation = CalculateGizmoRotation(cameraTransform);
			var isSnapped = IsSnapped(gizmoPosition);

			// Move with keyboard
			if (currentEventType == EventType.KeyDown)
			{
				var modifiers = currentEvent.modifiers;
				var speedModifier = modifiers == EventModifiers.Shift;
				if (!IsMouseDown && (modifiers == EventModifiers.None || speedModifier))
				{
					var keyCode = currentEvent.keyCode;
					if (keyCode != 0)
					{
						var action = GetKeyAction(keyCode);
						if (action != KeyAction.Unspecified)
						{
							DoAction(action, speedModifier, selectedObject, gizmoRotation);
							currentEvent.Use();
						}
					}
				}
			}

			// Draw
			if (currentEventType == EventType.Repaint)
			{
				//Handles.DoPositionHandle(GizmoPosition, GizmoRotation);
				DrawArrows(camera, gizmoPosition, gizmoRotation, GizmoSize, !isSnapped);
			}
		}

		#endregion

		#region Enabled/Disabled

		public bool IsEnabled = true;

		#endregion

		#region Keyboard

		private enum KeyAction
		{
			Unspecified,
			Forward,
			Backward,
			Left,
			Right,
			Up,
			Down,
		}

		private class Key
		{
			public KeyCode KeyCode;
			public KeyAction Action;
		}

		//private KeyValuePair<string, Key[]>[] PredefinedKeySchemes;

		private Key[] Keys;

		private void InitializeKeyboard()
		{
			Keys = KeyScheme_LeftHand;

			//PredefinedKeySchemes = new[]
			//{
			//	new KeyValuePair<string, Key[]>("LeftHand", KeyScheme_LeftHand),
			//	new KeyValuePair<string, Key[]>("Keypad", KeyScheme_Keypad),
			//	new KeyValuePair<string, Key[]>("Arrows", KeyScheme_Arrows),
			//};
		}

		private KeyAction GetKeyAction(KeyCode keyCode)
		{
			for (var i = 0; i < Keys.Length; i++)
			{
				if (Keys[i].KeyCode == keyCode)
					return Keys[i].Action;
			}
			return KeyAction.Unspecified;
		}

		private static Vector3 GetDirection(KeyAction action)
		{
			switch (action)
			{
				case KeyAction.Forward: return Vector3.forward;
				case KeyAction.Backward: return Vector3.back;
				case KeyAction.Left: return Vector3.left;
				case KeyAction.Right: return Vector3.right;
				case KeyAction.Up: return Vector3.up;
				case KeyAction.Down: return Vector3.down;
				default:
					throw new ArgumentOutOfRangeException("action", action, null);
			}
		}

		private void DoAction(KeyAction action, bool speedModifier, Transform transform, Quaternion gizmoRotation)
		{
			//Debug.Log("Action: " + action);

			var distance = speedModifier
				? SnappingDistance * SpeedModifierMovementFactor
				: SnappingDistance;

			if (IsSnappingEnabled)
			{
				var targetPosition = transform.position + (gizmoRotation * GetDirection(action)) * distance;
				transform.position = targetPosition.Snap(SnappingDistance, SnappingOffset);
			}
			else
			{
				transform.position += (gizmoRotation * GetDirection(action)) * distance;
			}
		}

		#endregion

		#region Gizmo Rotation

		private Quaternion CalculateGizmoRotation(Transform cameraTransform)
		{
			var euler = cameraTransform.eulerAngles;

			euler.x = Mathf.Round((euler.x - 45f) / 90f + 0.5f) * 90f;
			euler.y = Mathf.Round((euler.y - 45f) / 90f + 0.5f) * 90f;
			euler.z = Mathf.Round((euler.z - 45f) / 90f + 0.5f) * 90f;

			return Quaternion.Euler(euler);
		}

		#endregion

		#region Draw Arrows

		private class ArrowColors
		{
			public Color Front;
			public Color LeftRight;
			public Color Back;
		}

		private readonly Rect IdentityRect = new Rect(0f, 0f, 1f, 1f);
		private readonly Matrix4x4 FrontArrowMatrix = Matrix4x4.TRS(new Vector3(-0.5f, -1.5f, 0f) * FrontArrowScale, Quaternion.identity, new Vector3(1f, 1f, 1f) * FrontArrowScale);
		private readonly Matrix4x4 BackArrowMatrix = Matrix4x4.TRS(new Vector3(-0.5f, 1.5f, 0f) * BackArrowScale, Quaternion.identity, new Vector3(1f, -1f, 1f) * BackArrowScale);
		private readonly Matrix4x4 RightArrowMatrix = Matrix4x4.TRS(new Vector3(0.5f, -0.5f, 0f) * LeftRightArrowScale, Quaternion.identity, new Vector3(1f, 1f, 1f) * LeftRightArrowScale);
		private readonly Matrix4x4 LeftArrowMatrix = Matrix4x4.TRS(new Vector3(-0.5f, -0.5f, 0f) * LeftRightArrowScale, Quaternion.identity, new Vector3(-1f, 1f, 1f) * LeftRightArrowScale);

		private void DrawArrows(Camera camera, Vector3 gizmoPosition, Quaternion gizmoRotation, float gizmoSize, bool drawRed)
		{
			var distanceToCameraFactor = (camera.transform.position - gizmoPosition).magnitude * 0.2f * gizmoSize;

			GL.PushMatrix();
			GL.LoadProjectionMatrix(camera.projectionMatrix);
			var baseMatrix = camera.worldToCameraMatrix * Matrix4x4.TRS(gizmoPosition, gizmoRotation * Quaternion.Euler(-90f, 0f, 0f), Vector3.one * distanceToCameraFactor);

			var colors = drawRed ? RedArrowColors : NormalArrowColors;

			var texture = SnappaIcons.Texture_ArrowStraight;
			// Front arrow
			GL.modelview = baseMatrix * FrontArrowMatrix;
			Graphics.DrawTexture(IdentityRect, texture, IdentityRect, 0, 0, 0, 0, colors.Front);
			// Back arrow
			GL.modelview = baseMatrix * BackArrowMatrix;
			Graphics.DrawTexture(IdentityRect, texture, IdentityRect, 0, 0, 0, 0, colors.Back);

			texture = SnappaIcons.Texture_ArrowBent;
			// Right arrow
			GL.modelview = baseMatrix * RightArrowMatrix;
			Graphics.DrawTexture(IdentityRect, texture, IdentityRect, 0, 0, 0, 0, colors.LeftRight);
			// Left arrow
			GL.modelview = baseMatrix * LeftArrowMatrix;
			Graphics.DrawTexture(IdentityRect, texture, IdentityRect, 0, 0, 0, 0, colors.LeftRight);

			GL.PopMatrix();
		}

		#endregion

		#region Snapping

		public bool IsSnappingEnabled = true;
		public float SnappingDistance = 1f;
		public float SnappingOffset = 0.5f;

		public bool IsSnapped(Vector3 point, float precision = 0.001f)
		{
			return point.IsSnapped(SnappingDistance, SnappingOffset, precision);
		}

		#endregion
	}

}
