// Work in progress.
/*
using System;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	public class Replacea : ExtenityEditorWindowBase
	{
		#region Configuration

		private static readonly Vector2 MinimumWindowSize = new Vector2(200f, 50f);

		#endregion

		#region Initialization

		[MenuItem("Edit/Replacea", false, 1010)] // Just below Unity's "Snap Settings"
		private static void ShowWindow()
		{
			var window = GetWindow<Replacea>();
			window.Show();
		}

		private void OnEnable()
		{
			SetTitleAndIcon("Replacea", null);
			minSize = MinimumWindowSize;

			//SceneView.onSceneGUIDelegate -= OnSceneGUI;
			//SceneView.onSceneGUIDelegate += OnSceneGUI;
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//	//SceneView.onSceneGUIDelegate -= OnSceneGUI;
		//}

		#endregion

		#region GUI - Window

		private readonly GUILayoutOption[] ReplaceButtonOptions = { GUILayout.Width(100f), GUILayout.Height(30f) };
		private readonly GUIContent ReplaceButtonContent = new GUIContent("Replace", "Replaces all selected objects with the specified object.");

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(ReplaceButtonContent, "Button", ReplaceButtonOptions))
			{
				Replace();
			}
			GUILayout.EndHorizontal();

			if (GUI.changed)
			{
				SceneView.RepaintAll();
			}
		}

		#endregion

		#region Replace

		private void Replace()
		{
			throw new NotImplementedException();
		}

		#endregion
	}

}
*/
