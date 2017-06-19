using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public class DLLBuilderWindow : ExtenityEditorWindowBase
	{
		#region Initialization

		private void OnEnable()
		{
			DLLBuilder.OnRepaintRequested.AddListener(ThreadSafeRepaint);
		}

		#endregion

		#region Show

		[MenuItem(Constants.MenuItemPrefix + "Open Builder", priority = 1000)]
		public static void ShowWindow()
		{
			var window = GetWindow<DLLBuilderWindow>();
			window.titleContent = new GUIContent("DLL Builder");
		}

		#endregion

		#region GUI

		public static readonly GUILayoutOption[] ThinButtonOptions = { GUILayout.Width(230f), GUILayout.Height(24f) };
		public static readonly GUILayoutOption[] ThickButtonOptions = { GUILayout.Width(230f), GUILayout.Height(46f) };

		protected override void OnGUIDerived()
		{
			GUILayout.Space(20f);

			GUI.enabled = !DLLBuilder.IsProcessing;
			if (GUILayout.Button("Build And Distribute", ThickButtonOptions))
			{
				DLLBuilder.StartProcess(BuildTriggerSource.UI);
			}
			GUI.enabled = true;

			GUILayout.Space(20f);
			if (GUILayout.Button("Configuration", ThickButtonOptions))
			{
				DLLBuilderConfiguration.SelectOrCreateConfigurationAsset();
			}

			GUILayout.Space(40f);
			GUILayout.Label("Cleanup");

			if (GUILayout.Button("Clear All Output DLLs", ThinButtonOptions))
			{
				Cleaner.ClearAllOutputDLLs(DLLBuilderConfiguration.Instance);
			}

			//if (GUILayout.Button("Build DLLs", ThinButtonOptions))
			//{
			//	DLLBuilder.BuildDLLs();
			//}

			//GUILayout.Space(40f);
			//GUILayout.Label("Package In Output Project");

			//if (GUILayout.Button("Copy Assets To Output Project", ThinButtonOptions))
			//{
			//	PackageBuilder.CopyExtenityAssetsToOutputProject();
			//}

			//GUILayout.Space(40f);
			//GUILayout.Label("Distribute To Outside Projects");

			//if (GUILayout.Button("Distribute", ThinButtonOptions))
			//{
			//	Distributer.DistributeToAll();
			//}
		}

		#endregion
	}

}
