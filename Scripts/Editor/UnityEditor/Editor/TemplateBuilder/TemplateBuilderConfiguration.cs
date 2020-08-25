using System.Collections.Generic;
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

		[Title("Include Filters"), PropertySpace(10)]
		[InlineProperty, HideLabel]
		public StringFilter Include = new StringFilter(
			new StringFilterEntry(StringFilterType.Exactly, ".gitignore"),
			new StringFilterEntry(StringFilterType.StartsWith, "Assets/"),
			new StringFilterEntry(StringFilterType.Exactly, "Packages/manifest.json"),
			new StringFilterEntry(StringFilterType.StartsWith, "ProjectSettings/")
		);

		[Title("Ignore Filters")]
		[InlineProperty, HideLabel]
		public StringFilter Ignore = new StringFilter(
			new StringFilterEntry(StringFilterType.StartsWith, ".git/"),
			new StringFilterEntry(StringFilterType.StartsWith, ".hg/"),
			new StringFilterEntry(StringFilterType.StartsWith, ".vs/"),
			new StringFilterEntry(StringFilterType.StartsWith, ".idea/"),
			new StringFilterEntry(StringFilterType.StartsWith, "obj/"),
			new StringFilterEntry(StringFilterType.StartsWith, "Temp/"),
			new StringFilterEntry(StringFilterType.StartsWith, "Library/"),
			new StringFilterEntry(StringFilterType.Exactly, "ProjectSettings/ProjectVersion.txt")
		);

		[Title("Output")]
		[PropertySpace(10), LabelWidth(100)]
		[FolderPath(UseBackslashes = true)]
		public string OutputDirectory = "Export/UnityTemplate/";

		[PropertySpace(10), Button(ButtonSizes.Large)]
		public void Build()
		{
			TemplateBuilder.BuildTemplate(this);
		}

		public void CheckConsistency(ref List<ConsistencyError> errors)
		{
			Metadata.CheckConsistency(ref errors);
		}
	}

}
