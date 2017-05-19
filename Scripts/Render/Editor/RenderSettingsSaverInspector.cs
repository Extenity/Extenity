using UnityEditor;
using UnityEngine;

namespace Extenity.RenderingToolbox
{

	[CustomEditor(typeof(RenderSettingsSaver))]
	public class RenderSettingsSaverInspector : Editor
	{
		#region Initialization

		private SerializedObject configuration;
		private RenderSettingsSaver me;

		private void OnEnable()
		{
			me = (RenderSettingsSaver)target;
			configuration = new SerializedObject(target);
		}

		#endregion

		#region Inspector GUI

		public override void OnInspectorGUI()
		{
			configuration.Update();

			// Display configuration as readonly
			{
				EditorGUI.BeginDisabledGroup(true);
				DrawDefaultInspector();
				EditorGUI.EndDisabledGroup();
			}

			GUILayout.Space(20);

			// Reset Configuration
			{
				if (GUILayout.Button("Update"))
				{
					me.GetConfigurationFromRenderSettings();
				}
			}

			configuration.ApplyModifiedProperties();
		}

		#endregion
	}

	//public class RenderSettingsAssetModificationProcessor : AssetModificationProcessor
	//{
	//	public static string[] OnWillSaveAssets(string[] paths)
	//	{
	//		var renderSettingsSavers = Object.FindObjectsOfType<RenderSettingsSaver>();

	//		foreach (var renderSettingsSaver in renderSettingsSavers)
	//		{
	//			renderSettingsSaver.GetConfigurationFromRenderSettings();
	//		}

	//		return paths;
	//	}
	//}

}
