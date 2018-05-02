using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class HiddenGameObjectTools : ExtenityEditorWindowBase
	{
		#region Configuration

		private const float StringMatcherTolerance = 0.8f;

		#endregion

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

		private string SearchText = "";

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

			// Search bar
			if (EditorGUILayoutTools.SearchBar(ref SearchText))
			{
				RefreshFilteredLists();
			}
			var isFiltering = !string.IsNullOrEmpty(SearchText);

			// List
			ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
			{

				// List hidden objects
				EditorGUILayout.LabelField(
					isFiltering
						? "Hidden Objects (" + DisplayedHiddenObjects.Count + " of " + HiddenObjects.Count + ")"
						: "Hidden Objects (" + HiddenObjects.Count + ")",
					EditorStyles.boldLabel);
				for (int i = 0; i < DisplayedHiddenObjects.Count; i++)
				{
					DrawEntry(DisplayedHiddenObjects[i]);
				}

				GUILayout.Space(20f);

				// List visible objects
				EditorGUILayout.LabelField(
					isFiltering
						? "Visible Objects (" + DisplayedVisibleObjects.Count + " of " + VisibleObjects.Count + ")"
						: "Visible Objects (" + VisibleObjects.Count + ")",
					EditorStyles.boldLabel);
				for (int i = 0; i < DisplayedVisibleObjects.Count; i++)
				{
					DrawEntry(DisplayedVisibleObjects[i]);
				}
			}
			GUILayout.EndScrollView();
		}

		private static void DrawEntry(GameObject obj)
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

		private List<GameObject> DisplayedHiddenObjects = new List<GameObject>();
		private List<GameObject> DisplayedVisibleObjects = new List<GameObject>();


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

			RefreshFilteredLists();

			Repaint();
		}

		private void RefreshFilteredLists()
		{
			DisplayedHiddenObjects.Clear();
			DisplayedVisibleObjects.Clear();

			foreach (var obj in HiddenObjects)
			{
				if (!obj ||
					string.IsNullOrEmpty(SearchText) ||
					LiquidMetalStringMatcher.Score(obj.name, SearchText) > StringMatcherTolerance
				)
				{
					DisplayedHiddenObjects.Add(obj);
				}
			}

			foreach (var obj in VisibleObjects)
			{
				if (!obj ||
					string.IsNullOrEmpty(SearchText) ||
					LiquidMetalStringMatcher.Score(obj.name, SearchText) > StringMatcherTolerance
				)
				{
					DisplayedVisibleObjects.Add(obj);
				}
			}
		}

		private static bool IsHidden(GameObject go)
		{
			return (go.hideFlags & HideFlags.HideInHierarchy) != 0;
		}

		#endregion
	}

}
