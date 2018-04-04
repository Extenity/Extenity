using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;

namespace ExtenityExamples.UnityEditorToolbox
{

	[CustomPropertyDrawer(typeof(Example_ConditionalHideIn_BigClass))]
	public class Example_ConditionalHideIn_BigClass_PropertyDrawer : ExtenityPropertyDrawerBase<Example_ConditionalHideIn_BigClass>
	{
	}

	[CustomPropertyDrawer(typeof(Example_ConditionalHideIn_Entry))]
	public class Example_ConditionalHideIn_Entry_PropertyDrawer : ExtenityPropertyDrawerBase<Example_ConditionalHideIn_Entry>
	{
	}

}
