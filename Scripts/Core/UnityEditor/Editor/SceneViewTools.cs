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
	}

}
