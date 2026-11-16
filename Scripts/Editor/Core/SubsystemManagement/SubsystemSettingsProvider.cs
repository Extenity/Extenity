#if UNITY_5_3_OR_NEWER
#if ExtenitySubsystems

using Extenity.InspectorToolbox.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Extenity.SubsystemManagementToolbox
{

	/// <summary>
	/// The Subsystems page of the Project Settings: the inspector of the project's <see cref="SubsystemSettings"/>,
	/// or a button that creates the settings with their defaults when the project has none.
	/// </summary>
	public class SubsystemSettingsProvider : SettingsProvider
	{
		internal const string CreateButtonName = "subsystem-settings-create-button";

		#region Initialization

		private SubsystemSettingsProvider(string path, SettingsScope scope)
			: base(path, scope)
		{
		}

		[SettingsProvider]
		public static SettingsProvider Create()
		{
			return new SubsystemSettingsProvider("Project/Subsystems", SettingsScope.Project)
			{
				keywords = new[] { "Subsystem", "Subsystems", "Application Subsystems", "Scene Subsystems", "Scene Setup" },
			};
		}

		#endregion

		#region Activation

		private VisualElement Root;
		private UnityEditor.Editor SettingsEditor;

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			base.OnActivate(searchContext, rootElement);
			Root = rootElement;
			Rebuild();
		}

		public override void OnDeactivate()
		{
			DestroySettingsEditor();
			Root = null;
			base.OnDeactivate();
		}

		#endregion

		#region GUI

		/// <summary>Builds the page again for the settings that exist now.</summary>
		internal void Rebuild()
		{
			if (Root == null)
				throw new System.InvalidOperationException("The settings page is not active.");

			DestroySettingsEditor();
			Root.Clear();
			InspectorStyles.Apply(Root);

			var title = new Label("Subsystems");
			title.style.fontSize = 19;
			title.style.unityFontStyleAndWeight = UnityEngine.FontStyle.Bold;
			title.style.marginLeft = 9;
			title.style.marginTop = 1;
			title.style.marginBottom = 8;
			Root.Add(title);

			if (SubsystemSettings.GetInstance(out var settings, SubsystemConstants.ConfigurationFileNameWithoutExtension) &&
			    settings.IsFileNameValid())
			{
				SettingsEditor = UnityEditor.Editor.CreateEditor(settings);
				var scroll = new ScrollView(ScrollViewMode.Vertical);
				scroll.style.flexGrow = 1;
				scroll.Add(new InspectorElement(SettingsEditor));
				Root.Add(scroll);
			}
			else
			{
				var box = new BoxGroup();
				box.style.marginLeft = 9;
				box.style.marginRight = 9;
				box.Add(new MessageBox("The project has no subsystem settings yet.", MessageSeverity.Info));
				var button = new Button(() =>
				{
					SubsystemSettings.Create(SubsystemConstants.ConfigurationDefaultFilePath);
					Rebuild();
				})
				{
					name = CreateButtonName,
					text = "Create Default Settings",
				};
				box.Add(button);
				Root.Add(box);
			}
		}

		private void DestroySettingsEditor()
		{
			if (SettingsEditor)
			{
				UnityEngine.Object.DestroyImmediate(SettingsEditor);
			}
			SettingsEditor = null;
		}

		#endregion
	}

}

#endif
#endif
