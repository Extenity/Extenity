using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.SceneManagementToolbox.Editor;

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

		[MenuItem(ExtenityMenu.Analysis + "Scene/Hidden GameObject Tools", priority = ExtenityMenu.AnalysisPriority + 3)]
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
				if (GUILayout.Button("Prepare For\nUnload", BigButtonHeight, ButtonWidth))
				{
					EditorSceneManagerTools.UnloadAllScenes(false);
					EditorUtilityTools.RequestScriptReload();
				}
				if (GUILayout.Button("Unload\nUnused", BigButtonHeight, ButtonWidth))
				{
					// Desperately try to unload assets in memory. Assembly reloading helps a lot.
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
					Log.Info("Completed unloading unused assets. Make sure to press Prepare For Unload for best cleanup.");
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

		private void DrawGroup(string header, ref bool foldout, bool isFiltering, List<WeakReference> objects, List<WeakReference> filteredObjects)
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
						Selection.objects = filteredObjects
							.Where(entry => entry.IsAlive && (GameObject)entry.Target)
							.Select(entry => (GameObject)entry.Target)
							.ToArray();
					}

					if (GUILayoutTools.Button("Reveal All", enabled, ButtonWidth))
					{
						RevealOrHideObjects(filteredObjects
							.Where(entry => entry.IsAlive && (GameObject)entry.Target)
							.Select(entry => (GameObject)entry.Target)
							.ToArray()
						);
					}

					if (GUILayoutTools.Button("Delete All", enabled, ButtonWidth))
					{
						DeleteObjects(filteredObjects
							.Where(entry => entry.IsAlive && (GameObject)entry.Target)
							.Select(entry => (GameObject)entry.Target)
							.ToArray()
						);
					}
				}
			}
			GUILayout.EndHorizontal();

			if (foldout)
			{
				// Draw object lines
				for (int i = 0; i < filteredObjects.Count; i++)
				{
					var entry = filteredObjects[i];
					if (!entry.IsAlive)
						continue;
					var go = (GameObject)entry.Target;

					DrawEntry(go);
				}
			}

			GUILayout.Space(20f);
		}

		private void DrawEntry(GameObject go)
		{
			GUILayout.BeginHorizontal();
			{
				var gone = go == null;
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
							name = go.name;
							break;
						case DisplayMode.FullName:
							name = go.FullName();
							break;
						case DisplayMode.AssetPath:
							name = AssetDatabase.GetAssetPath(go);
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
						Selection.activeGameObject = go;
					}
					if (GUILayout.Button(IsHidden(go) ? "Reveal" : "Hide", ButtonWidth))
					{
						RevealOrHideObject(go);
					}
					if (GUILayout.Button("Delete", ButtonWidth))
					{
						DeleteObject(go);
					}
				}
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Gather Objects

		private readonly List<WeakReference> HiddenObjectsInLoadedScenes = new List<WeakReference>();
		private readonly List<WeakReference> HiddenObjectsOnTheLoose = new List<WeakReference>();
		private readonly List<WeakReference> VisibleObjectsInLoadedScenes = new List<WeakReference>();
		private readonly List<WeakReference> VisibleObjectsOnTheLoose = new List<WeakReference>();

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
						HiddenObjectsInLoadedScenes.Add(new WeakReference(go));
					}
					else
					{
						HiddenObjectsOnTheLoose.Add(new WeakReference(go));
					}
				}
				else
				{
					if (go.scene.isLoaded)
					{
						VisibleObjectsInLoadedScenes.Add(new WeakReference(go));
					}
					else
					{
						VisibleObjectsOnTheLoose.Add(new WeakReference(go));
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

		private readonly List<WeakReference> DisplayedHiddenObjectsOnTheLoose = new List<WeakReference>();
		private readonly List<WeakReference> DisplayedHiddenObjectsInLoadedScenes = new List<WeakReference>();
		private readonly List<WeakReference> DisplayedVisibleObjectsOnTheLoose = new List<WeakReference>();
		private readonly List<WeakReference> DisplayedVisibleObjectsInLoadedScenes = new List<WeakReference>();

		private string SearchText = "";

		private void RefreshFilteredLists()
		{
			RefreshFilteredList(HiddenObjectsInLoadedScenes, DisplayedHiddenObjectsInLoadedScenes);
			RefreshFilteredList(HiddenObjectsOnTheLoose, DisplayedHiddenObjectsOnTheLoose);
			RefreshFilteredList(VisibleObjectsInLoadedScenes, DisplayedVisibleObjectsInLoadedScenes);
			RefreshFilteredList(VisibleObjectsOnTheLoose, DisplayedVisibleObjectsOnTheLoose);

			Repaint();
		}

		private void RefreshFilteredList(List<WeakReference> objects, List<WeakReference> filteredObjects)
		{
			filteredObjects.Clear();

			foreach (var obj in objects)
			{
				if (!obj.IsAlive)
					continue;
				var go = (GameObject)obj.Target;
				if (!go ||
					string.IsNullOrEmpty(SearchText) ||
					LiquidMetalStringMatcher.Score(go.name, SearchText) > StringMatcherTolerance
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

		#region Log

		private static readonly Logger Log = new(nameof(HiddenGameObjectTools));

		#endregion
	}

}
