using System;
using Extenity.UnityEditorToolbox;
using UnityEngine;

namespace ExtenityExamples.UnityEditorToolbox
{

	public enum Example_ConditionalHideIn_Enum
	{
		Unspecified,
		EnumOn,
		EnumOff,
	}

	[Serializable]
	public class Example_ConditionalHideIn_BigClass
	{
		[Header("Values")]
		public int IntValue;
		public Example_ConditionalHideIn_Enum EnumValue;

		// IntValue
		[Header("Conditions based on " + nameof(IntValue))]
		[ConditionalHideInInspector(nameof(IntValue), 1)]
		public int VariableShownIfValueIs1;

		[ConditionalHideInInspector(nameof(IntValue), 2, true)]
		public int VariableShownIfValueIsNot2;

		[ConditionalHideInInspector(nameof(IntValue), 5, true, HideOrDisable.Disable)]
		public int VariableDisabledIfValueIs5;

		[ConditionalHideInInspector(nameof(IntValue), 5, false, HideOrDisable.Disable)]
		public int VariableEnabledIfValueIs5;

		[ConditionalHideInInspector(nameof(IntValue), 6)]
		public int[] IntArrayEnabledIfValueIs6;

		[ConditionalHideInInspector(nameof(IntValue), 6)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfValueIs6;

		[ConditionalHideInInspector(nameof(IntValue), 6, true)]
		public int[] IntArrayEnabledIfValueIsNot6;

		[ConditionalHideInInspector(nameof(IntValue), 6, true)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfValueIsNot6;

		// EnumValue
		[Header("Conditions based on " + nameof(EnumValue))]
		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn)]
		public int VariableShownIfEnumValueIsOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOff)]
		public int VariableShownIfEnumValueIsOff;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn, true)]
		public int VariableShownIfEnumValueIsNotOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOff, HideOrDisable.Disable)]
		public int VariableDisabledIfEnumValueIsOff;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn)]
		public int[] IntArrayEnabledIfEnumValueIsOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfEnumValueIsOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn, true)]
		public int[] IntArrayEnabledIfEnumValueIsNotOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn, true)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfEnumValueIsNotOn;
	}

	[Serializable]
	public class Example_ConditionalHideIn_Entry
	{
		public int SomeValue;

		[ConditionalHideInInspector(nameof(SomeValue), 3)]
		public int VariableEnabledIfValueIs3;

		[ConditionalHideInInspector(nameof(SomeValue), 6)]
		public int[] IntArrayEnabledIfValueIs6;
	}

	public class Example_ConditionalHideInInspector : MonoBehaviour
	{
		[Header("Values")]
		public int IntValue;
		public Example_ConditionalHideIn_Enum EnumValue;

		// IntValue
		[Header("Conditions based on " + nameof(IntValue))]
		[ConditionalHideInInspector(nameof(IntValue), 1)]
		public int VariableShownIfValueIs1;

		[ConditionalHideInInspector(nameof(IntValue), 2, true)]
		public int VariableShownIfValueIsNot2;

		[ConditionalHideInInspector(nameof(IntValue), 5, true, HideOrDisable.Disable)]
		public int VariableDisabledIfValueIs5;

		[ConditionalHideInInspector(nameof(IntValue), 5, false, HideOrDisable.Disable)]
		public int VariableEnabledIfValueIs5;

		[ConditionalHideInInspector(nameof(IntValue), 6)]
		public int[] IntArrayEnabledIfValueIs6;

		[ConditionalHideInInspector(nameof(IntValue), 6)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfValueIs6;

		[ConditionalHideInInspector(nameof(IntValue), 6, true)]
		public int[] IntArrayEnabledIfValueIsNot6;

		[ConditionalHideInInspector(nameof(IntValue), 6, true)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfValueIsNot6;

		// EnumValue
		[Header("Conditions based on " + nameof(EnumValue))]
		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn)]
		public int VariableShownIfEnumValueIsOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOff)]
		public int VariableShownIfEnumValueIsOff;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn, true)]
		public int VariableShownIfEnumValueIsNotOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOff, HideOrDisable.Disable)]
		public int VariableDisabledIfEnumValueIsOff;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn)]
		public int[] IntArrayEnabledIfEnumValueIsOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfEnumValueIsOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn, true)]
		public int[] IntArrayEnabledIfEnumValueIsNotOn;

		[ConditionalHideInInspector(nameof(EnumValue), Example_ConditionalHideIn_Enum.EnumOn, true)]
		public Example_ConditionalHideIn_Entry[] ObjectArrayEnabledIfEnumValueIsNotOn;

		// Container
		[Header("Class Referenced In MonoBehaviour")]
		public Example_ConditionalHideIn_BigClass BigClass;
	}

}
