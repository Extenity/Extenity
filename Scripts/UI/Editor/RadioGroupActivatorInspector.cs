using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;

namespace Extenity.UIToolbox.Editor
{

	// This class's existence is needed for conditional attributes to work, even if it looks like not being used.
	[CustomEditor(typeof(RadioGroupActivator))]
	public class RadioGroupActivatorInspector : ExtenityEditorBase<RadioGroupActivator>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}
