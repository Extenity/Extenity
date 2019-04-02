using System;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillerToolbox.Editor
{

	public class Catalogue : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Catalogue",
			MinimumWindowSize = new Vector2(200f, 50f),
		};

		protected override bool EnableSavingStateToEditorPrefs => true;

		#endregion

		#region Initialization

		protected override void OnEnableDerived()
		{
			InitializeTools();
		}

		[MenuItem("Tools/Painkiller/Catalogue %&A")]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<Catalogue>();
		}

		#endregion

		#region GUI - Style

		// Toolbar
		[NonSerialized]
		private GUIStyle _ToolbarBackgroundStyle;
		private GUIStyle ToolbarBackgroundStyle
		{
			get
			{
				if (_ToolbarBackgroundStyle == null)
				{
					_ToolbarBackgroundStyle = new GUIStyle();
					_ToolbarBackgroundStyle.margin = new RectOffset();
					_ToolbarBackgroundStyle.padding = new RectOffset(0, 0, 6, 6);
					_ToolbarBackgroundStyle.normal.background = EditorGUIUtilityTools.DarkerDefaultBackgroundTexture;
				}
				return _ToolbarBackgroundStyle;
			}
		}
		private readonly GUILayoutOption[] ToolbarBackgroundLayoutOptions = { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false) };
		private readonly GUILayoutOption[] ToolbarLayoutOptions = { GUILayout.ExpandWidth(false), GUILayout.Height(34f) };
		private readonly string[] Tabs = { "Materials", "Canvases" };

		#endregion

		#region GUI - Window

		protected override void OnGUIDerived()
		{
			GUILayout.BeginHorizontal(ToolbarBackgroundStyle, ToolbarBackgroundLayoutOptions);
			ActiveToolIndex = GUILayout.Toolbar(ActiveToolIndex, Tabs, ToolbarLayoutOptions);
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset"))
			{
				Relaunch(ToggleWindow, true);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(8f);

			MaterialsTool.OnGUI();

			if (GUI.changed)
			{
				SceneView.RepaintAll();
			}
		}

		#endregion

		#region Tools

		public enum Tool
		{
			Materials,
			Canvases,
		}

		[SerializeField]
		private int ActiveToolIndex;

		[SerializeField]
		private MaterialsTool MaterialsTool;
		[SerializeField]
		private CanvasesTool CanvasesTool;

		private void InitializeTools()
		{
			if (MaterialsTool == null)
			{
				MaterialsTool = new MaterialsTool();
			}
			MaterialsTool.OnRepaintRequest -= Repaint;
			MaterialsTool.OnRepaintRequest += Repaint;

			if (CanvasesTool == null)
			{
				CanvasesTool = new CanvasesTool();
			}
			CanvasesTool.OnRepaintRequest -= Repaint;
			CanvasesTool.OnRepaintRequest += Repaint;
		}

		#endregion
	}

}
