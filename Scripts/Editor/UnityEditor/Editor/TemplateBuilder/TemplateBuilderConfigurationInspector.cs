using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.UnityProjectTemplateToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="TemplateBuilderConfiguration"/>: metadata, the two filter lists and the output
	/// folder, each under its own title.
	/// </summary>
	[CustomEditor(typeof(TemplateBuilderConfiguration))]
	public sealed class TemplateBuilderConfigurationInspector : ObjectInspector<TemplateBuilderConfiguration>
	{
		internal const string FilterOrderInfo = "First, include filters, then ignore filters are applied to the list of all files in project directory.";

		protected override void Configure(InspectorBuilder<TemplateBuilderConfiguration> b)
		{
			b.Group("Metadata", GroupStyle.Title, g => g.Fields(x => x.Metadata));
			b.Field(x => x.Metadata).Inline();

			b.Group("Include Filters", GroupStyle.Title, g =>
			{
				g.Info(FilterOrderInfo);
				g.Fields(x => x.Include);
			});
			b.Field(x => x.Include).Inline();

			b.Group("Ignore Filters", GroupStyle.Title, g => g.Fields(x => x.Ignore));
			b.Field(x => x.Ignore).Inline();

			b.Group("Output", GroupStyle.Title, g => g.Fields(x => x.OutputDirectory));
			b.Field(x => x.OutputDirectory).FolderPicker();
		}
	}

}
