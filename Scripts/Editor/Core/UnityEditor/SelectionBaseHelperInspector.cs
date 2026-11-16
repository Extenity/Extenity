using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="SelectionBaseHelper"/>: the component has no settings, so not even its Script
	/// field is shown.
	/// </summary>
	[CustomEditor(typeof(SelectionBaseHelper))]
	[CanEditMultipleObjects]
	public sealed class SelectionBaseHelperInspector : ObjectInspector<SelectionBaseHelper>
	{
		protected override void Configure(InspectorBuilder<SelectionBaseHelper> b)
		{
			b.HideScriptField();
		}
	}

}
