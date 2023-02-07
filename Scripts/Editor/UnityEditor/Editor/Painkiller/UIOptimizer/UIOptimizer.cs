using System.Collections.Generic;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.TextureToolbox;
using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.PainkillerToolbox.Editor
{

	public class UIOptimizer : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "UI Optimizer",
			MinimumWindowSize = new Vector2(200f, 50f),
		};

		#endregion

		#region Initialization

		[MenuItem(ExtenityMenu.Painkiller + "UI Optimizer", priority = ExtenityMenu.PainkillerPriority + 4)]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<UIOptimizer>();
		}

		#endregion

		#region Serialized Properties

		//private SerializedProperty ReplaceAsPrefabProperty;

		#endregion

		#region GUI - Window

		private readonly GUILayoutOption[] FindButtonOptions = { GUILayout.Width(100f), GUILayout.Height(30f) };
		private readonly GUIContent FindButtonContent = new GUIContent("Find", "Finds all UI elements which has a Raycast Target option in selected and children objects.");
		private readonly GUILayoutOption[] ListEntryLeftButtonOptions = { GUILayout.Height(18f), GUILayout.Width(18f) };
		private readonly GUILayoutOption[] ListEntryRightButtonOptions = { GUILayout.Height(18f) };
		private readonly GUIContent ListEntryLeftButtonActiveContent = new GUIContent("", "Currently active");
		private readonly GUIContent ListEntryLeftButtonInactiveContent = new GUIContent("", "Currently inactive");
		private GUIStyle ListEntryStyleActive;
		private GUIStyle ListEntryStyleInactive;

		protected override void OnGUIDerived()
		{
			if (ListEntryStyleActive == null)
			{
				ListEntryStyleActive = new GUIStyle(EditorStyles.toggle);
				ListEntryStyleActive.alignment = TextAnchor.MiddleLeft;
				ListEntryStyleActive.normal.background = TextureTools.CreateSimpleTexture(Color.blue);
				ListEntryStyleInactive = new GUIStyle(ListEntryStyleActive);
				ListEntryStyleInactive.normal.background = TextureTools.CreateSimpleTexture(Color.grey);
			}

			GUILayout.Space(8f);

			if (GUILayout.Button(FindButtonContent, FindButtonOptions))
			{
				FindInSelection();
			}

			GUILayout.Space(20f);

			var filteredCount = FilteredSelection.SafeCount();
			EditorGUILayoutTools.DrawHeader("Selected Graphics (" + filteredCount + ")");
			if (!FilteredSelection.IsNullOrEmpty())
			{
				ScrollPosition = EditorGUILayout.BeginScrollView(ScrollPosition, false, false);
				for (var i = 0; i < FilteredSelection.Count; i++)
				{
					var selection = FilteredSelection[i];
					var currentlyEnabled = selection.Graphic.raycastTarget;
					var style = currentlyEnabled ? ListEntryStyleActive : ListEntryStyleInactive;
					var leftButtonContent = currentlyEnabled ? ListEntryLeftButtonActiveContent : ListEntryLeftButtonInactiveContent;
					EditorGUILayout.BeginHorizontal();
					var toggle = GUILayout.Toggle(currentlyEnabled, leftButtonContent, style, ListEntryLeftButtonOptions);
					if (toggle != currentlyEnabled)
					{
						selection.Graphic.raycastTarget = !currentlyEnabled;
						Repaint();
					}
					if (GUILayout.Button(selection.GUIContent, style, ListEntryRightButtonOptions))
					{
						Selection.activeGameObject = selection.Graphic.gameObject;
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndScrollView();
			}
		}

		#endregion

		#region Selection

		public struct SelectionEntry
		{
			public string Name;
			public Graphic Graphic;
			public GUIContent GUIContent;

			public SelectionEntry(string name, Graphic graphic)
			{
				Name = name;
				Graphic = graphic;
				GUIContent = new GUIContent(name);
			}
		}

		public List<SelectionEntry> FilteredSelection;

		private void FindInSelection()
		{
			var selectedObjects = Selection.GetFiltered<Graphic>(SelectionMode.Deep);

			if (FilteredSelection != null)
				FilteredSelection.Clear();
			else
				FilteredSelection = new List<SelectionEntry>(100);

			foreach (var selectedObject in selectedObjects)
			{
				FilteredSelection.Add(new SelectionEntry(selectedObject.FullGameObjectName(), selectedObject));
			}

			Repaint();
		}

		private void SelectFilteredObjects()
		{
			if (FilteredSelection.IsNullOrEmpty())
			{
				Selection.activeObject = null;
			}
			Selection.objects = FilteredSelection.Select(selected => selected.Graphic.gameObject).ToArray();
		}

		#endregion
	}

}
