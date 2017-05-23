using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class HiddenGameObjectTools : ExtenityEditorWindowBase
	{
		#region Menu Command

		[MenuItem("Tools/Hidden GameObject Tools")]
		public static void ShowWindow()
		{
			var window = GetWindow<HiddenGameObjectTools>();
			window.titleContent = new GUIContent("Hidden GOs");
			window.GatherHiddenObjects();
		}

		#endregion

		#region Initialization

		private void OnEnable()
		{
			IsRightMouseButtonScrollingEnabled = true;
		}

		#endregion

		#region GUI

		private static readonly GUILayoutOption ButtonWidth = GUILayout.Width(80);
		private static readonly GUILayoutOption BigButtonHeight = GUILayout.Height(35);

		protected override void OnGUIDerived()
		{
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Refresh", BigButtonHeight))
				{
					GatherHiddenObjects();
				}
				if (GUILayout.Button("Test", BigButtonHeight, ButtonWidth))
				{
					var go = new GameObject("HiddenTestObject");
					go.hideFlags = HideFlags.HideInHierarchy;
					GatherHiddenObjects();
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(10f);

			ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);

			EditorGUILayout.LabelField("Hidden Objects (" + HiddenObjects.Count + ")", EditorStyles.boldLabel);
			for (int i = 0; i < HiddenObjects.Count; i++)
			{
				DrawLine(HiddenObjects[i]);
			}

			GUILayout.Space(20f);

			EditorGUILayout.LabelField("Visible Objects (" + VisibleObjects.Count + ")", EditorStyles.boldLabel);
			for (int i = 0; i < VisibleObjects.Count; i++)
			{
				DrawLine(VisibleObjects[i]);
			}

			GUILayout.EndScrollView();
		}

		private static void DrawLine(GameObject obj)
		{
			GUILayout.BeginHorizontal();
			{
				var gone = obj == null;
				GUILayout.Label(gone ? "null" : obj.name);
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
						Selection.activeGameObject = obj;
					}
					if (GUILayout.Button(IsHidden(obj) ? "Reveal" : "Hide", ButtonWidth))
					{
						obj.hideFlags ^= HideFlags.HideInHierarchy;
						if (!Application.isPlaying)
						{
							EditorSceneManager.MarkSceneDirty(obj.scene);
						}
					}
					if (GUILayout.Button("Delete", ButtonWidth))
					{
						var scene = obj.scene;
						DestroyImmediate(obj);
						EditorSceneManager.MarkSceneDirty(scene);
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Hidden Objects

		private List<GameObject> HiddenObjects = new List<GameObject>();
		private List<GameObject> VisibleObjects = new List<GameObject>();

		private void GatherHiddenObjects()
		{
			HiddenObjects.Clear();
			VisibleObjects.Clear();
			//var allObjects = FindObjectsOfType<GameObject>(); This one does not reveal objects marked as DontDestroyOnLoad.
			var allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
			for (int i = 0; i < allObjects.Length; i++)
			{
				var go = allObjects[i];

				if (IsHidden(go))
				{
					HiddenObjects.Add(go);
				}
				else
				{
					VisibleObjects.Add(go);
				}
			}

			Repaint();
		}

		private static bool IsHidden(GameObject go)
		{
			return (go.hideFlags & HideFlags.HideInHierarchy) != 0;
		}

		#endregion
	}

}
