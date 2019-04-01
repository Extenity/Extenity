using System;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	public class AssetUtilizza : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "Asset Utilizza",
			MinimumWindowSize = new Vector2(200f, 50f),
		};

		#endregion

		#region Initialization

		protected override void OnEnableDerived()
		{
			InitializeTools();
		}

		[MenuItem("Tools/Painkilla/Asset Utilizza %&A", false, 100)]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<AssetUtilizza>();
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
			GUILayout.EndHorizontal();

			GUILayout.Space(8f);

			MaterialsTool.OnGUI(this);

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

		[NonSerialized]
		private AssetUtilizzaTool[] Tools;
		[SerializeField]
		private int ActiveToolIndex;

		[SerializeField]
		private MaterialsTool MaterialsTool;
		//[SerializeField]
		//private CanvasesTool CanvasesTool;

		private void InitializeTools()
		{
			if (MaterialsTool == null)
			{
				MaterialsTool = new MaterialsTool();
			}
		}

		#endregion
	}

}
