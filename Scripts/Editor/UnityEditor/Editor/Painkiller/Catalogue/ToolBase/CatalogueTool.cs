using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.FileSystemToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.SceneManagementToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	public abstract class CatalogueTool
	{
		#region Style

		private readonly GUILayoutOption[] RefreshButtonOptions = { GUILayout.Width(100f), GUILayout.Height(24f) };
		private readonly GUIContent RefreshButtonContent = new GUIContent("Refresh", "Scans all objects.");

		#endregion

		#region GUI

		public abstract void OnGUI();

		#endregion

		#region Top Bar

		[NonSerialized]
		private SearchField SearchField;

		protected virtual string SearchString { get; set; }
		protected virtual void OnRefreshButtonClicked() { }

		protected void InitializeSearchField(SearchField.SearchFieldCallback downOrUpArrowKeyPressed)
		{
			SearchField = new SearchField();
			SearchField.downOrUpArrowKeyPressed += downOrUpArrowKeyPressed;
		}

		protected void DrawTopBar()
		{
			GUILayout.BeginHorizontal();

			// Refresh button
			if (GUILayout.Button(RefreshButtonContent, RefreshButtonOptions))
			{
				OnRefreshButtonClicked();
			}

			// Search field
			GUILayout.BeginVertical();
			GUILayout.Space(6f);
			var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label, GUILayoutTools.ExpandWidth);
			SearchString = SearchField.OnGUI(rect, SearchString);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		#endregion

		#region GUI Needs Repaint

		public event Action OnRepaintRequest;

		protected void SendRepaintRequest()
		{
			OnRepaintRequest?.Invoke();
		}

		#endregion

		#region Gather Object In Scene

		protected static List<TTreeElement> BuildElementsListByCollectingDependenciesReferencedInScenes<TObject, TTreeElement>(Func<TObject, string, TTreeElement, TTreeElement> treeElementCreator, Func<TTreeElement> rootCreator, SceneListFilter sceneListFilter)
			where TObject : UnityEngine.Object
			where TTreeElement : CatalogueElement<TTreeElement>, new()
		{
			var objectsInScenes = EditorUtilityTools.CollectDependenciesReferencedInScenes<TObject>(sceneListFilter);

			var rootElement = rootCreator();
			var elementsByObjects = new Dictionary<TObject, TTreeElement>(objectsInScenes.Sum(item => item.Value.Length));

			foreach (var objectsInScene in objectsInScenes)
			{
				var scene = objectsInScene.Key;
				var objects = objectsInScene.Value;

				foreach (TObject obj in objects)
				{
					if (!elementsByObjects.TryGetValue(obj, out var element))
					{
						element = treeElementCreator(obj, scene.name, rootElement);
						elementsByObjects.Add(obj, element);
					}
					else
					{
						element.AddScene(scene.name);
					}
				}
			}

			var elements = elementsByObjects.Values.ToList();
			elements.Insert(0, rootElement);
			return elements;
		}

		#endregion

		#region Gather Asests in Resources

		protected static List<TTreeElement> BuildElementsListByCollectingResources<TObject, TTreeElement>(Func<TObject, string, string, TTreeElement, TTreeElement> treeElementCreator, Func<TTreeElement> rootCreator, string loadPath, bool includeInEditorFolders)
			where TObject : UnityEngine.Object
			where TTreeElement : CatalogueElement<TTreeElement>, new()
		{
			var assets = Resources.LoadAll(loadPath, typeof(TObject)).Cast<TObject>().ToArray();

			var rootElement = rootCreator();
			var elementsByObjects = new Dictionary<TObject, TTreeElement>(assets.Length);

			foreach (var asset in assets)
			{
				if (!asset)
				{
					Log.WarningWithContext(asset, "Failed to load an asset.");
					continue;
				}

				var assetPath = AssetDatabase.GetAssetPath(asset).FixDirectorySeparatorChars();
				var split = assetPath.Split(PathTools.DirectorySeparatorChar);
				var editorIndex = split.IndexOf("Editor", StringComparison.OrdinalIgnoreCase);
				var resourcesIndex = split.IndexOf("Resources", StringComparison.OrdinalIgnoreCase);
				var resourcePath = string.Join("/", split.GetRange(resourcesIndex + 1, split.Length - (resourcesIndex + 1)));

				if (!includeInEditorFolders)
				{
					if (editorIndex >= 0 && editorIndex < resourcesIndex) // TODO: Not sure about how an Editor folder being in Resources folder is processed by Unity. Needs more tests.
					{
						continue;
					}
				}

				if (!elementsByObjects.TryGetValue(asset, out var element))
				{
					element = treeElementCreator(asset, assetPath, resourcePath, rootElement);
					elementsByObjects.Add(asset, element);
				}
				else
				{
					// Asset was already added. This is not expected.
					Log.WarningWithContext(asset, $"Asset '{asset}' was already added.");
				}
			}

			var elements = elementsByObjects.Values.ToList();
			elements.Insert(0, rootElement);
			return elements;
		}

		#endregion

		#region Log

		protected static readonly Logger Log = new(nameof(CatalogueTool));

		#endregion
	}

}
