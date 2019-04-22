using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
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

		public bool Foldout_HiddenObjectsInLoadedScenes = true;
		public bool Foldout_HiddenObjectsOnTheLoose = true;
		public bool Foldout_VisibleObjectsInLoadedScenes = true;
		public bool Foldout_VisibleObjectsOnTheLoose = true;

		public enum DisplayMode
		{
			Name,
			FullName,
			AssetPath,
		}

		public DisplayMode CurrentDisplayMode = DisplayMode.Name;

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
					// Desperately try to unload assets in memory. But still, Unity is a greedy beach and won't free most of them (if any).
					// Also there is the possibility that we haven't looked into yet. Objects may use HideFlags.DontUnloadUnusedAsset.
					Resources.UnloadUnusedAssets();
					GC.Collect();
					Resources.UnloadUnusedAssets();
					GC.Collect();
					EditorUtility.UnloadUnusedAssetsImmediate(true);
					GC.Collect();
					EditorUtility.UnloadUnusedAssetsImmediate(true);
					GC.Collect();
					GatherObjects();
				}
				if (GUILayout.Button("Create\nTest Object", BigButtonHeight, ButtonWidth))
				{
					var go = new GameObject("HiddenTestObject");
					go.hideFlags = HideFlags.HideInHierarchy;
					GatherObjects();
				}
			}
			GUILayout.EndHorizontal();

			CurrentDisplayMode = (DisplayMode)EditorGUILayout.EnumPopup("Display Mode", CurrentDisplayMode, GUILayout.ExpandWidth(false));

			GUILayout.Space(10f);

			// Search bar
			if (EditorGUILayoutTools.SearchBar(ref SearchText))
			{
				RefreshFilteredLists();
			}
			var isFiltering = !string.IsNullOrEmpty(SearchText);
			GUILayout.Space(10f);

			// List
			ScrollPosition = GUILayout.BeginScrollView(ScrollPosition);
			{
				DrawGroup("Hidden Objects In Loaded Scenes", ref Foldout_HiddenObjectsInLoadedScenes, isFiltering, HiddenObjectsInLoadedScenes, DisplayedHiddenObjectsInLoadedScenes);
				DrawGroup("Hidden Objects On The Loose", ref Foldout_HiddenObjectsOnTheLoose, isFiltering, HiddenObjectsOnTheLoose, DisplayedHiddenObjectsOnTheLoose);
				DrawGroup("Visible Objects In Loaded Scenes", ref Foldout_VisibleObjectsInLoadedScenes, isFiltering, VisibleObjectsInLoadedScenes, DisplayedVisibleObjectsInLoadedScenes);
				DrawGroup("Visible Objects On The Loose", ref Foldout_VisibleObjectsOnTheLoose, isFiltering, VisibleObjectsOnTheLoose, DisplayedVisibleObjectsOnTheLoose);
			}
			GUILayout.EndScrollView();
		}

		private void DrawGroup(string header, ref bool foldout, bool isFiltering, List<GameObject> objects, List<GameObject> filteredObjects)
		{
			// Header
			GUILayout.BeginHorizontal();
			{
				foldout = EditorGUILayout.Foldout(foldout,
					isFiltering
						? $"{header} ({filteredObjects.Count} of {objects.Count})"
						: $"{header} ({objects.Count})"
				);

				if (foldout)
				{
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
			}
			GUILayout.EndHorizontal();

			if (foldout)
			{
				// Draw object lines
				for (int i = 0; i < filteredObjects.Count; i++)
				{
					DrawEntry(filteredObjects[i]);
				}
			}

			GUILayout.Space(20f);
		}

		private void DrawEntry(GameObject obj)
		{
			GUILayout.BeginHorizontal();
			{
				var gone = obj == null;
				string name;
				if (gone)
				{
					name = "null";
				}
				else
				{
					switch (CurrentDisplayMode)
					{
						case DisplayMode.Name:
							name = obj.name;
							break;
						case DisplayMode.FullName:
							name = obj.FullName();
							break;
						case DisplayMode.AssetPath:
							name = AssetDatabase.GetAssetPath(obj);
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				GUILayout.Label(name);
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
