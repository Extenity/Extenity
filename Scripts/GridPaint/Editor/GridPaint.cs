using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.GridPaintTool.Editor
{

	public class GridPaint : ExtenityEditorWindowBase
	{
		#region Initialization

		[MenuItem("Window/Extenity GridPaint", false, 1003)]
		private static void ShowWindow()
		{
			var window = GetWindow<GridPaint>();
			window.Show();
		}

		private void OnEnable()
		{
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

		public float Distance = 5f;

		Vector3 GizmoPosition;
		Quaternion GizmoRotation;

		private void OnSceneGUI(SceneView sceneview)
		{
			var cameraTransform = SceneView.currentDrawingSceneView.camera.transform;

			GizmoPosition = cameraTransform.position + cameraTransform.forward * Distance;
			var euler = cameraTransform.eulerAngles;

			//var before = rotation.y;
			euler.x = Mathf.Round((euler.x - 45f) / 90f + 0.5f) * 90f;
			euler.y = Mathf.Round((euler.y - 45f) / 90f + 0.5f) * 90f;
			euler.z = Mathf.Round((euler.z - 45f) / 90f + 0.5f) * 90f;
			//Debug.Log("before: " + before + "    \t   after: " + rotation.y);

			GizmoRotation = Quaternion.Euler(euler);

			Handles.DoPositionHandle(GizmoPosition, GizmoRotation);
			//Gizmos.DrawIcon();
		}

		protected override void OnGUIDerived()
		{
		}
	}

}
