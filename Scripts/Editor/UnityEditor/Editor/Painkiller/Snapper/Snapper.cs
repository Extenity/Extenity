using System;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	public class Snapper : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Snapper",
			Icon = SnapperIcons.Texture_ArrowStraight,
			MinimumWindowSize = new Vector2(200f, 50f),
		};

		private const float GizmoSize = 1f;
		private const float SpeedModifierFactor = 5f;

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

		protected override void OnEnableDerived()
		{
			InitializeKeyboard();
		}

		[MenuItem(ExtenityMenu.Edit + "Snapper", priority = ExtenityMenu.UnitySnapSettingsMenuPriority)]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<Snapper>();
		}

		#endregion

		#region GUI - Window

		private readonly GUILayoutOption[] ActiveButtonOptions = { GUILayout.Width(100f), GUILayout.Height(30f) };
		private readonly GUILayoutOption[] SnapButtonOptions = { GUILayout.Width(30f), GUILayout.Height(30f) };
		private readonly GUIContent ActiveButtonContent = new GUIContent("Active", $"Toggle whole {nameof(Snapper)} tool functionality. Useful for temporarily deactivating the tool.");
		private readonly GUIContent LinearSnapButtonContent = new GUIContent("P", "Snap position of selected objects.");
		private readonly GUIContent AngularSnapButtonContent = new GUIContent("R", "Snap rotation of selected objects.");

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			GUILayout.BeginHorizontal();
			IsActive = GUILayout.Toggle(IsActive, ActiveButtonContent, "Button", ActiveButtonOptions);
			if (GUILayout.Button(LinearSnapButtonContent, SnapButtonOptions))
			{
				LinearSnapSelected();
			}
			if (GUILayout.Button(AngularSnapButtonContent, SnapButtonOptions))
			{
				AngularSnapSelected();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(8f);
			DrawHelp();

			if (GUI.changed)
			{
				SceneView.RepaintAll();
			}
		}

		private void DrawHelp()
		{
			GUILayout.Label("Move objects with keyboard");
			foreach (var key in Keys)
			{
				GUILayout.Label($"  {key.KeyCode} : {key.Action}");
			}
		}

		#endregion

		#region GUI - Scene

		private bool IsMouseDown;

		protected override void DuringSceneGUI(SceneView sceneView)
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

			if (!IsActive)
				return;
			if (Tools.current != Tool.Move && Tools.current != Tool.Rotate)
				return;
			//Tools.hidden = true;

			// Get SceneView camera
			if (SceneView.currentDrawingSceneView == null || SceneView.currentDrawingSceneView.camera == null)
				return;
			var camera = SceneView.currentDrawingSceneView.camera;
			var cameraTransform = camera.transform;

			// Get selected object
			var selectedObject = SelectionTools.GetSingleTransformInScene();
			if (selectedObject == null)
				return;

			// Calculate gizmo position and rotation
			//var gizmoPosition = cameraTransform.position + cameraTransform.forward * Distance;
			var gizmoPosition = selectedObject.position;
			var gizmoRotation = CalculateGizmoRotation(cameraTransform);

			// Move and rotate with keyboard
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
							if (Tools.current == Tool.Move)
							{
								DoMovementAction(action, speedModifier, selectedObject, gizmoRotation);
							}
							else // Tools.current == Tool.Rotation
							{
								DoRotationAction(action, speedModifier, selectedObject, gizmoRotation);
							}
							currentEvent.Use();
						}
					}
				}
			}

			// Draw
			if (currentEventType == EventType.Repaint)
			{
				if (Tools.current == Tool.Move)
				{
					var isPositionSnapped = IsPositionSnapped(selectedObject.position);
					DrawMovementArrows(camera, gizmoPosition, gizmoRotation, GizmoSize, !isPositionSnapped);
				}
				//else // Tools.current == Tool.Rotation
				//{
				//	var isRotationSnapped = IsRotationSnapped(selectedObject.eulerAngles);
				//	DrawRotationArrows(camera, gizmoPosition, gizmoRotation, GizmoSize, !isRotationSnapped);
				//}
			}
		}

		#endregion

		#region Enabled/Disabled

		public bool IsActive = true;

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

		private static Vector3 GetLinearDirection(KeyAction action)
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
					throw new ArgumentOutOfRangeException(nameof(action), action, null);
			}
		}

		private static Vector3 GetAngularDirection(KeyAction action)
		{
			switch (action)
			{
				case KeyAction.Forward: return Vector3.right;
				case KeyAction.Backward: return Vector3.left;
				case KeyAction.Left: return Vector3.down;
				case KeyAction.Right: return Vector3.up;
				case KeyAction.Up: return Vector3.back;
				case KeyAction.Down: return Vector3.forward;
				default:
					throw new ArgumentOutOfRangeException(nameof(action), action, null);
			}
		}

		private void DoMovementAction(KeyAction action, bool speedModifier, Transform transform, Quaternion gizmoRotation)
		{
			Undo.RecordObject(transform, "Snapper Move");

			var shift = speedModifier
				? LinearSnappingStep * SpeedModifierFactor
				: LinearSnappingStep;

			// Magic! If the object is not snapped currently, this will make it snap to the closest position in movement direction.
			if (!transform.position.IsSnapped(LinearSnappingStep, LinearSnappingOffset, LinearSnappingPrecision))
			{
				shift -= LinearSnappingStep * 0.5f;
			}

			if (IsSnappingEnabled)
			{
				var targetPosition = transform.position + (gizmoRotation * GetLinearDirection(action)) * shift;
				transform.position = targetPosition.Snap(LinearSnappingStep, LinearSnappingOffset);
			}
			else
			{
				transform.position += (gizmoRotation * GetLinearDirection(action)) * shift;
			}
		}

		private void DoRotationAction(KeyAction action, bool speedModifier, Transform transform, Quaternion gizmoRotation)
		{
			Undo.RecordObject(transform, "Snapper Rotate");

			var shift = speedModifier
				? AngularSnappingStep * SpeedModifierFactor
				: AngularSnappingStep;

			// Magic! If the object is not snapped currently, this will make it snap to the closest position in movement direction.
			if (!transform.eulerAngles.IsSnapped(AngularSnappingStep, AngularSnappingOffset, AngularSnappingPrecision))
			{
				shift -= AngularSnappingStep * 0.5f;
			}

			if (IsSnappingEnabled)
			{
				var targetRotation = transform.eulerAngles + (gizmoRotation * GetAngularDirection(action)) * shift;
				transform.eulerAngles = targetRotation.Snap(AngularSnappingStep, AngularSnappingOffset);
			}
			else
			{
				transform.rotation *= Quaternion.AngleAxis(shift, gizmoRotation * GetAngularDirection(action));
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

		private void DrawMovementArrows(Camera camera, Vector3 gizmoPosition, Quaternion gizmoRotation, float gizmoSize, bool drawRed)
		{
			var distanceToCameraFactor = (camera.transform.position - gizmoPosition).magnitude * 0.2f * gizmoSize;

			GL.PushMatrix();
			GL.LoadProjectionMatrix(camera.projectionMatrix);
			var baseMatrix = camera.worldToCameraMatrix * Matrix4x4.TRS(gizmoPosition, gizmoRotation * Quaternion.Euler(-90f, 0f, 0f), Vector3.one * distanceToCameraFactor);

			var colors = drawRed ? RedArrowColors : NormalArrowColors;

			var texture = SnapperIcons.Texture_ArrowStraight;
			// Front arrow
			GL.modelview = baseMatrix * FrontArrowMatrix;
			Graphics.DrawTexture(IdentityRect, texture, IdentityRect, 0, 0, 0, 0, colors.Front);
			// Back arrow
			GL.modelview = baseMatrix * BackArrowMatrix;
			Graphics.DrawTexture(IdentityRect, texture, IdentityRect, 0, 0, 0, 0, colors.Back);

			texture = SnapperIcons.Texture_ArrowBent;
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

		public float LinearSnappingStep = 1f;
		public float LinearSnappingOffset = 0.5f;
		public float LinearSnappingPrecision = 0.001f;

		public float AngularSnappingStep = 30f;
		public float AngularSnappingOffset = 0f;
		public float AngularSnappingPrecision = 0.001f;

		public bool IsPositionSnapped(Vector3 point)
		{
			return point.IsSnapped(LinearSnappingStep, LinearSnappingOffset, LinearSnappingPrecision);
		}

		public bool IsRotationSnapped(Vector3 euler)
		{
			return euler.IsSnapped(AngularSnappingStep, AngularSnappingOffset, AngularSnappingPrecision);
		}

		#endregion

		#region Snap Selected Objects

		public void LinearSnapSelected()
		{
			DoSnapOnSelected(transform =>
			{
				transform.position = transform.position.Snap(LinearSnappingStep, LinearSnappingOffset);
			});
		}

		public void AngularSnapSelected()
		{
			DoSnapOnSelected(transform =>
			{
				transform.eulerAngles = transform.eulerAngles.Snap(AngularSnappingStep, AngularSnappingOffset);
			});
		}

		private void DoSnapOnSelected(Action<Transform> onApplySnap)
		{
			var transforms = Selection.GetTransforms(SelectionMode.ExcludePrefab | SelectionMode.TopLevel);
			if (transforms.IsNullOrEmpty())
			{
				Log.Info("Nothing to snap.");
			}
			else
			{
				Log.Info($"Snapping objects ({transforms.Length}): \n" + transforms.Select(item => item.FullGameObjectName()).ToList().Serialize('\n'));

				Undo.RecordObjects(transforms, $"Snap {transforms.Length.ToStringWithEnglishPluralPostfix("object")}");

				foreach (var transform in transforms)
				{
					onApplySnap(transform);
				}
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Snapper));

		#endregion
	}

}
