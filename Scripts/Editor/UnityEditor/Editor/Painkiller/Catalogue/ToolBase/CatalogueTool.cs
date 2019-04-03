using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.IMGUIToolbox.Editor;
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

		protected static List<TTreeElement> BuildElementsListByCollectingDependenciesReferencedInLoadedScenes<TObject, TTreeElement>(Func<TObject, string, TTreeElement, TTreeElement> treeElementCreator, Func<TTreeElement> rootCreator)
			where TObject : UnityEngine.Object
			where TTreeElement : CatalogueElement<TTreeElement>, new()
		{
			var objectsInScenes = EditorUtilityTools.CollectDependenciesReferencedInLoadedScenes<TObject>();

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
	}

}
