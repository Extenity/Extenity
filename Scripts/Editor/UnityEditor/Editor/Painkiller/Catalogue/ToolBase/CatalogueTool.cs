using System;
using Extenity.IMGUIToolbox.Editor;
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
	}

}
