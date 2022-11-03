using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UnityProjectTemplateToolbox.Editor
{

	[CreateAssetMenu(fileName = nameof(TemplateBuilderConfiguration), menuName = "Template Builder Configuration", order = 7000)]
	public class TemplateBuilderConfiguration : ScriptableObject, IConsistencyChecker
	{
		[Title("Metadata")]
		[InlineProperty, HideLabel]
		public TemplateMetadata Metadata;

		[InfoBox("First, include filters, then ignore filters are applied to the list of all files in project directory.")]
		[Title("Include Filters"), PropertySpace(10)]
		[InlineProperty, HideLabel]
		public StringFilter Include = new StringFilter(
			new StringFilterEntry(StringFilterType.Exactly, ".gitignore"),
			new StringFilterEntry(StringFilterType.StartsWith, "Assets/"),
			new StringFilterEntry(StringFilterType.StartsWith, "Packages/"), // Including 'manifest.json' and 'packages-lock.json' which defines the dependency packages and their exact versions to be used.
			new StringFilterEntry(StringFilterType.StartsWith, "ProjectSettings/")
		);

		[Title("Ignore Filters")]
		[InlineProperty, HideLabel]
		public StringFilter Ignore = new StringFilter(
			// new StringFilterEntry(StringFilterType.Exactly, "ProjectSettings/ProjectVersion.txt") Not sure it is a good idea to exclude version info.
		);

		[Title("Output")]
		[PropertySpace(10), LabelWidth(100)]
		[FolderPath(UseBackslashes = true)]
		public string OutputDirectory = "Export/UnityProjectTemplate/";

		// Decided not to provide this functionality in Configuration. Template build operation is generally triggered
		// in one of the steps of larger build operations and putting that Build button here might be confusing.
		// [HorizontalGroup("BuildLine", Order = 6, MarginLeft = 0.25f, MarginRight = 0.25f)]
		// [PropertySpace(10), Button(ButtonSizes.Gigantic)]
		// public void Build()
		// {
		// 	TemplateBuilder.BuildProjectTemplateAsZip(this);
		// }

		public void CheckConsistency(ConsistencyChecker checker)
		{
			checker.ProceedTo(nameof(Metadata), Metadata);
		}
	}

}
