#if UNITY
#if ExtenitySubsystems

using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.SubsystemManagementToolbox
{

	public class SubsystemSettingsProvider : SettingsProvider
	{
		#region Configuration

		class Styles
		{
			public static GUIContent TargetInterface = new GUIContent("Target Interface");
			public static GUIContent Shortcut = new GUIContent("Shortcut");
			public static GUIContent InvalidKey = new GUIContent("Invalid key");
		}

		#endregion

		#region Initialization

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			base.OnActivate(searchContext, rootElement);
		}

		private SubsystemSettingsProvider(string path, SettingsScope scope)
			: base(path, scope)
		{
		}

		[SettingsProvider]
		public static SettingsProvider Create()
		{
			var provider = new SubsystemSettingsProvider("Project/Subsystems", SettingsScope.Project);

			// Automatically extract all keywords from the Styles.
			provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
			return provider;
		}

		#endregion

		#region Deinitialization

		public override void OnDeactivate()
		{
			base.OnDeactivate();
		}

		#endregion

		#region GUI

		[SerializeField]
		private OdinEditor _OdinEditor;

		private void InitializeGUI()
		{
			if (_OdinEditor == null ||
			    _OdinEditor.target == null ||
			    !(_OdinEditor.target as SubsystemSettings).IsFileNameValid())
			{
				if (_OdinEditor)
				{
					OdinEditor.DestroyImmediate(_OdinEditor);
					_OdinEditor = null;
				}

				if (SubsystemSettings.GetInstance(out var settings, SubsystemConstants.ConfigurationFileNameWithoutExtension))
				{
					_OdinEditor = (OdinEditor)OdinEditor.CreateEditor(settings, typeof(OdinEditor));
				}
				else
				{
					_OdinEditor = null;
				}
			}
		}

		public override void OnGUI(string searchContext)
		{
			InitializeGUI();

			EditorGUI.BeginChangeCheck();

			if (_OdinEditor != null)
			{
				_OdinEditor.OnInspectorGUI();
			}
			else
			{
				if (GUILayout.Button("Create Default Settings"))
				{
					SubsystemSettings.Create(SubsystemConstants.ConfigurationDefaultFilePath);
				}
			}

			if (EditorGUI.EndChangeCheck())
			{
			}
		}

		#endregion
	}

}

#endif
#endif
