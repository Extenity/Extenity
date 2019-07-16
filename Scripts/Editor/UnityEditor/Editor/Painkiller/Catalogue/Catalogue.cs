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

		#endregion

		#region GUI - Window

		protected override void OnGUIDerived()
		{
			GUILayout.BeginHorizontal(ToolbarBackgroundStyle, ToolbarBackgroundLayoutOptions);
			var newActiveToolIndex = GUILayout.Toolbar(ActiveToolIndex, ToolNames, ToolbarLayoutOptions);
			if (ActiveToolIndex != newActiveToolIndex)
			{
				ChangeActiveTool(newActiveToolIndex);
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset"))
			{
				Relaunch(ToggleWindow, true);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(8f);

			ActiveTool.OnGUI();

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
			Resources,
		}

		private readonly string[] ToolNames =
		{
			"Materials",
			"Canvases",
			"Resources",
		};

		[SerializeField]
		private MaterialsTool MaterialsTool;
		[SerializeField]
		private CanvasesTool CanvasesTool;
		[SerializeField]
		private ResourcesTool ResourcesTool;

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

			if (ResourcesTool == null)
			{
				ResourcesTool = new ResourcesTool();
			}
			ResourcesTool.OnRepaintRequest -= Repaint;
			ResourcesTool.OnRepaintRequest += Repaint;

			// Initialize the active tool.
			ChangeActiveTool(ActiveToolIndex);
		}

		#endregion

		#region Active Tool

		[SerializeField]
		private int ActiveToolIndex = -1;
		[NonSerialized]
		private CatalogueTool ActiveTool;

		private void ChangeActiveTool(int newActiveToolIndex)
		{
			// Nope. This is not checked here. Otherwise initialization code gets complicated. Checking this in GUI instead.
			//if (ActiveToolIndex == newActiveToolIndex)
			//	return;

			if (newActiveToolIndex < 0)
			{
				newActiveToolIndex = 0; // Select the first tool if initializing just now.
			}

			ActiveToolIndex = newActiveToolIndex;

			switch ((Tool)ActiveToolIndex)
			{
				case Tool.Materials: ActiveTool = MaterialsTool; break;
				case Tool.Canvases: ActiveTool = CanvasesTool; break;
				case Tool.Resources: ActiveTool = ResourcesTool; break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion
	}

}
