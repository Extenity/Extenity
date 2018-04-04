using System;
using Extenity.UnityEditorToolbox;
using UnityEngine;

namespace ExtenityExamples.UnityEditorToolbox
{

	[Serializable]
	public class Example_ConditionalHideIn_Container
	{
		public int InnerValue;

		[ConditionalHideInInspector("InnerValue", 1)]
		public int InnerVariableShownIfValueIs1;

		[ConditionalHideInInspector("InnerValue", 2, true)]
		public int InnerVariableShownIfValueIsNot2;

		[ConditionalHideInInspector("InnerValue", 5, true, HideOrDisable.Disable)]
		public int InnerVariableDisabledIfValueIs5;

		[ConditionalHideInInspector("InnerValue", 5, false, HideOrDisable.Disable)]
		public int InnerVariableEnabledIfValueIs5;
	}

	[Serializable]
	public class Example_ConditionalHideIn_Entry
	{
		public int Value;

		[ConditionalHideInInspector("Value", 3)]
		public int VariableEnabledIfValueIs3;
	}

	public class Example_ConditionalHideInInspector : MonoBehaviour
	{
		public int SomeValue;

		[ConditionalHideInInspector("SomeValue", 1)]
		public int VariableShownIfValueIs1;

		[ConditionalHideInInspector("SomeValue", 2, true)]
		public int VariableShownIfValueIsNot2;

		[ConditionalHideInInspector("SomeValue", 5, true, HideOrDisable.Disable)]
		public int VariableDisabledIfValueIs5;

		[ConditionalHideInInspector("SomeValue", 5, false, HideOrDisable.Disable)]
		public int VariableEnabledIfValueIs5;

		[ConditionalHideInInspector("SomeValue", 6)]
		public int[] ArrayEnabledIfValueIs6;

		[Header("Inner Class Example")]
		public Example_ConditionalHideIn_Container Container;

		[Header("Custom Type List")]
		public Example_ConditionalHideIn_Entry[] ListWithCustomType;
	}

}
