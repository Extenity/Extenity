using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Extenity.EditorUtilities
{

	public class HiddenGameObjectTools : EditorWindow
	{
		#region Menu Command

		[MenuItem("Tools/Hidden GameObject Tools")]
		public static void Create()
		{
			var window = GetWindow<HiddenGameObjectTools>();
			window.titleContent = new GUIContent("Hidden GOs");
			window.GatherHiddenObjects();
			window.Repaint();
		}

		#endregion

		#region Initialization

		//protected void OnEnable()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDisable()
		//{
		//}

		#endregion

		#region GUI

		private static readonly GUILayoutOption ButtonWidth = GUILayout.Width(80);
		private static readonly GUILayoutOption BigButtonHeight = GUILayout.Height(35);

		void OnGUI()
		{
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();

			if (GUILayout.Button("Refresh", BigButtonHeight))
			{
				GatherHiddenObjects();
				Repaint();
			}
			if (GUILayout.Button("Test", BigButtonHeight, ButtonWidth))
			{
				var go = new GameObject("HiddenTestObject");
				go.hideFlags = HideFlags.HideInHierarchy;
				GatherHiddenObjects();
			}

			GUILayout.EndHorizontal();
			GUILayout.Space(10f);

			//int removeFromListAt = -1;

			GUILayout.Label("Hidden Objects (" + HiddenObjects.Count + ")");
			for (int i = 0; i < HiddenObjects.Count; i++)
			{
				var hiddenObject = HiddenObjects[i];
				GUILayout.BeginHorizontal();
				var gone = hiddenObject == null;
				GUILayout.Label(gone ? "null" : hiddenObject.name);
				GUILayout.FlexibleSpace();
				if (gone)
				{
					GUILayout.Box("Select", ButtonWidth);
					GUILayout.Box("Reveal", ButtonWidth);
					GUILayout.Box("Delete", ButtonWidth);
				}
				else
				{
					if (GUILayout.Button("Select", ButtonWidth))
					{
						Selection.activeGameObject = hiddenObject;
					}

					var hidden = (hiddenObject.hideFlags & HideFlags.HideInHierarchy) != 0;
					if (hidden)
					{
						if (GUILayout.Button("Reveal", ButtonWidth))
						{
							hiddenObject.hideFlags ^= HideFlags.HideInHierarchy;
							EditorSceneManager.MarkSceneDirty(hiddenObject.scene);
						}
					}
					else
					{
						if (GUILayout.Button("Hide", ButtonWidth))
						{
							hiddenObject.hideFlags ^= HideFlags.HideInHierarchy;
							EditorSceneManager.MarkSceneDirty(hiddenObject.scene);
						}
					}

					if (GUILayout.Button("Delete", ButtonWidth))
					{
						//removeFromListAt = i;
						var scene = hiddenObject.scene;
						DestroyImmediate(hiddenObject);
						EditorSceneManager.MarkSceneDirty(scene);
					}
				}
				GUILayout.EndHorizontal();
			}

			//if (removeFromListAt >= 0)
			//{
			//	HiddenObjects.RemoveAt(removeFromListAt);
			//}
		}

		#endregion

		#region Hidden Objects

		private List<GameObject> HiddenObjects = new List<GameObject>();

		private void GatherHiddenObjects()
		{
			HiddenObjects.Clear();

			var allObjects = FindObjectsOfType<GameObject>();
			foreach (var go in allObjects)
			{
				if ((go.hideFlags & HideFlags.HideInHierarchy) != 0)
				{
					HiddenObjects.Add(go);
				}
			}
		}

		#endregion
	}

}
