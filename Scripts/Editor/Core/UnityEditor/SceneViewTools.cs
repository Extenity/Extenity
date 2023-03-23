using System;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class SceneViewTools
	{
		public static Vector3 MouseWorldPositionInSceneView()
		{
			return HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
			//return SceneView.lastActiveSceneView.camera.ScreenToWorldPoint(Event.current.mousePosition);
		}

		public static void PreventSelectionForCurrentMouseClick()
		{
			var currentEvent = Event.current;
			if (currentEvent.type != EventType.MouseDown)
			{
				Log.Error($"Tried to prevent mouse selection in an event with type '{currentEvent.type}' which should be 'MouseDown'.");
				return;
			}
			var passiveControlId = GUIUtility.GetControlID(FocusType.Passive);
			GUIUtility.hotControl = passiveControlId;
			currentEvent.Use();
		}

		public static void MoveObjectToView(Transform transform)
		{
			var view = SceneView.lastActiveSceneView;
			if (view != null)
				view.MoveToView(transform);
		}

		#region Camera

		public static Transform GetCameraTransformOfActiveSceneViewOrDisplayErrorMessageBox()
		{
			if (SceneView.lastActiveSceneView &&
			    SceneView.lastActiveSceneView.camera)
			{
				SceneView.lastActiveSceneView.Show();
				return SceneView.lastActiveSceneView.camera.transform;
			}
			if (SceneView.currentDrawingSceneView &&
			    SceneView.currentDrawingSceneView.camera)
			{
				SceneView.currentDrawingSceneView.Show();
				return SceneView.currentDrawingSceneView.camera.transform;
			}
			EditorUtility.DisplayDialog("SceneView Required", "You need to open a SceneView first.", "OK");
			throw new Exception("You need to open a SceneView first.");
		}

		#endregion

		#region Instantiation Point

		public static Vector3 CalculateReasonableInstantiationPointOnGround(float groundLevelY = 0f, float resultOffsetY = 0f)
		{
			var cameraTransform = GetCameraTransformOfActiveSceneViewOrDisplayErrorMessageBox();

			var direction = cameraTransform.forward.WithY(0);
			direction = direction.sqrMagnitude < 0.001f
				? Vector3.zero
				: direction.normalized;

			var cameraPosition = cameraTransform.position;
			var cameraHeightFromGround = cameraPosition.y - groundLevelY;
			var groundPositionBelowCamera = cameraPosition.WithY(groundLevelY);
			var offsetThroughLookingDirection = direction * cameraHeightFromGround;
			var result = groundPositionBelowCamera + offsetThroughLookingDirection;
			result.y += resultOffsetY;
			return result;
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(SceneViewTools));

		#endregion
	}

}
