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
	}

}
