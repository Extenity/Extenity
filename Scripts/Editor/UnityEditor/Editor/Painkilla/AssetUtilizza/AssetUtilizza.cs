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
			InitializeMaterialList();
		}

		[MenuItem("Tools/Painkilla/Asset Utilizza %&A", false, 100)]
		private static void ToggleWindow()
		{
			EditorWindowTools.ToggleWindow<AssetUtilizza>();
		}

		#endregion

		#region GUI - Window

		private readonly GUILayoutOption[] RefreshButtonOptions = { GUILayout.Width(100f), GUILayout.Height(30f) };
		private readonly GUIContent RefreshButtonContent = new GUIContent("Refresh", "Scans all objects.");

		protected override void OnGUIDerived()
		{
			GUILayout.Space(8f);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button(RefreshButtonContent, RefreshButtonOptions))
			{
				MaterialList.GatherData();
				Repaint();
			}
			GUILayout.EndHorizontal();

			MaterialList.OnGUI();

			if (GUI.changed)
			{
				//Calculate();
				SceneView.RepaintAll();
			}
		}

		#endregion

		#region Material

		[SerializeField]
		private MaterialList MaterialList;

		private void InitializeMaterialList()
		{
			if (MaterialList == null)
			{
				MaterialList = new MaterialList();
			}
		}

		#endregion
	}

}
