using UnityEditor;
using Extenity.UnityEditorToolbox.Editor;

namespace Extenity.DLLBuilder
{

	[CustomEditor(typeof(DLLBuilderConfiguration))]
	public class DLLBuilderConfigurationInspector : ExtenityObjectEditorBase<DLLBuilderConfiguration>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = true;
			IsDefaultInspectorScriptFieldEnabled = false;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
