using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class HiddenGameObjectTools : ExtenityEditorWindowBase
	{
		#region Configuration

		private const float StringMatcherTolerance = 0.8f;

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Hidden GOs",
			EnableRightMouseButtonScrolling = true,
		};

		#endregion

		#region Initialization

		protected override void OnEnableDerived()
		{
			GatherObjects();
		}

		[MenuItem("Tools/Hidden GameObject Tools")]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<HiddenGameObjectTools>();
		}

		#endregion

		#region GUI

		private static readonly GUILayoutOption ButtonWidth = GUILayout.Width(80);

		protected override void OnGUIDerived()
		{
			GUILayout.Space(10f);
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Refresh", BigButtonHeight, GUILayout.ExpandWidth(true)))
				{
					GatherObjects();
				}
				if (GUILayout.Button("Unload\nUnused", BigButtonHeight, ButtonWidth))
				{
					Resources.UnloadUnusedAssets();
					GatherObjects();
				}
				if (GUILayout.Button("Test", BigButtonHeight, ButtonWidth))
				{
					var go = new GameObject("HiddenTestObject");
					go.hideFlags = HideFlags.HideInHierarchy;
					GatherObjects();
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
				GUILayout.Space(10f);

				DrawGroup("Hidden Objects In Loaded Scenes", isFiltering, HiddenObjectsInLoadedScenes, DisplayedHiddenObjectsInLoadedScenes);
				DrawGroup("Hidden Objects On The Loose", isFiltering, HiddenObjectsOnTheLoose, DisplayedHiddenObjectsOnTheLoose);
				DrawGroup("Visible Objects In Loaded Scenes", isFiltering, VisibleObjectsInLoadedScenes, DisplayedVisibleObjectsInLoadedScenes);
				DrawGroup("Visible Objects On The Loose", isFiltering, VisibleObjectsOnTheLoose, DisplayedVisibleObjectsOnTheLoose);
			}
			GUILayout.EndScrollView();
		}

		private void DrawGroup(string header, bool isFiltering, List<GameObject> objects, List<GameObject> filteredObjects)
		{
			// Header
			GUILayout.BeginHorizontal();
			{
				EditorGUILayout.LabelField(
					isFiltering
						? $"{header} ({filteredObjects.Count} of {objects.Count})"
						: $"{header} ({objects.Count})",
					EditorStyles.boldLabel);

				var enabled = filteredObjects.IsNotNullAndEmpty();

				if (GUILayoutTools.Button("Select All", enabled, ButtonWidth))
				{
					Selection.objects = filteredObjects.ToArray();
				}

				if (GUILayoutTools.Button("Reveal All", enabled, ButtonWidth))
				{
					RevealOrHideObjects(filteredObjects);
				}

				if (GUILayoutTools.Button("Delete All", enabled, ButtonWidth))
				{
					DeleteObjects(filteredObjects);
				}
			}
			GUILayout.EndHorizontal();

			// Draw object lines
			for (int i = 0; i < filteredObjects.Count; i++)
			{
				DrawEntry(filteredObjects[i]);
			}

			GUILayout.Space(20f);
		}

		private void DrawEntry(GameObject obj)
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
						RevealOrHideObject(obj);
					}
					if (GUILayout.Button("Delete", ButtonWidth))
					{
						DeleteObject(obj);
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Gather Objects

		private readonly List<GameObject> HiddenObjectsInLoadedScenes = new List<GameObject>();
		private readonly List<GameObject> HiddenObjectsOnTheLoose = new List<GameObject>();
		private readonly List<GameObject> VisibleObjectsInLoadedScenes = new List<GameObject>();
		private readonly List<GameObject> VisibleObjectsOnTheLoose = new List<GameObject>();

		private void GatherObjects()
		{
			HiddenObjectsInLoadedScenes.Clear();
			HiddenObjectsOnTheLoose.Clear();
			VisibleObjectsInLoadedScenes.Clear();
			VisibleObjectsOnTheLoose.Clear();

			//var allObjects = FindObjectsOfType<GameObject>(); This one does not reveal objects marked as DontDestroyOnLoad.
			var allObjects = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
			for (int i = 0; i < allObjects.Length; i++)
			{
				var go = allObjects[i];

				if (IsHidden(go))
				{
					if (go.scene.isLoaded)
					{
						HiddenObjectsInLoadedScenes.Add(go);
					}
					else
					{
						HiddenObjectsOnTheLoose.Add(go);
					}
				}
				else
				{
					if (go.scene.isLoaded)
					{
						VisibleObjectsInLoadedScenes.Add(go);
					}
					else
					{
						VisibleObjectsOnTheLoose.Add(go);
					}
				}
			}

			RefreshFilteredLists();
		}

		private static bool IsHidden(GameObject go)
		{
			return (go.hideFlags & HideFlags.HideInHierarchy) != 0;
		}

		#endregion

		#region Search and Filtering

		private readonly List<GameObject> DisplayedHiddenObjectsOnTheLoose = new List<GameObject>();
		private readonly List<GameObject> DisplayedHiddenObjectsInLoadedScenes = new List<GameObject>();
		private readonly List<GameObject> DisplayedVisibleObjectsOnTheLoose = new List<GameObject>();
		private readonly List<GameObject> DisplayedVisibleObjectsInLoadedScenes = new List<GameObject>();

		private string SearchText = "";

		private void RefreshFilteredLists()
		{
			RefreshFilteredList(HiddenObjectsInLoadedScenes, DisplayedHiddenObjectsInLoadedScenes);
			RefreshFilteredList(HiddenObjectsOnTheLoose, DisplayedHiddenObjectsOnTheLoose);
			RefreshFilteredList(VisibleObjectsInLoadedScenes, DisplayedVisibleObjectsInLoadedScenes);
			RefreshFilteredList(VisibleObjectsOnTheLoose, DisplayedVisibleObjectsOnTheLoose);

			Repaint();
		}

		private void RefreshFilteredList(List<GameObject> objects, List<GameObject> filteredObjects)
		{
			filteredObjects.Clear();

			foreach (var obj in objects)
			{
				if (!obj ||
					string.IsNullOrEmpty(SearchText) ||
					LiquidMetalStringMatcher.Score(obj.name, SearchText) > StringMatcherTolerance
				)
				{
					filteredObjects.Add(obj);
				}
			}
		}

		#endregion

		#region Operations - RevealOrHide / Delete

		private void RevealOrHideObject(GameObject obj)
		{
			EditorApplication.delayCall += () =>
			{
				InternalRevealOrHideObject(obj);
				GatherObjects();
			};
		}

		private void RevealOrHideObjects(IEnumerable<GameObject> objects)
		{
			EditorApplication.delayCall += () =>
			{
				foreach (var obj in objects)
					InternalRevealOrHideObject(obj);
				GatherObjects();
			};
		}

		private static void InternalRevealOrHideObject(GameObject obj)
		{
			obj.hideFlags ^= HideFlags.HideInHierarchy;
			if (!Application.isPlaying)
			{
				EditorSceneManager.MarkSceneDirty(obj.scene);
			}
		}

		private void DeleteObject(GameObject obj)
		{
			EditorApplication.delayCall += () =>
			{
				InternalDeleteObject(obj);
				GatherObjects();
			};
		}

		private void DeleteObjects(IEnumerable<GameObject> objects)
		{
			EditorApplication.delayCall += () =>
			{
				foreach (var obj in objects)
					InternalDeleteObject(obj);
				GatherObjects();
			};
		}

		private static void InternalDeleteObject(GameObject obj)
		{
			var scene = obj.scene;
			DestroyImmediate(obj);
			EditorSceneManager.MarkSceneDirty(scene);
		}

		#endregion
	}

}
