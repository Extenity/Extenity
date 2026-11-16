using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtenityTests.InspectorToolbox.Demo
{

	public enum DemoQuality
	{
		Low,
		Medium,
		High,
	}

	[Serializable]
	public struct DemoIdentifier
	{
		public string Value;

		public DemoIdentifier(string value)
		{
			Value = value;
		}
	}

	[Serializable]
	public struct DemoVersionSettings
	{
		public string MinimumSupportedVersion;
		public bool WarnOnOlderVersions;
	}

	/// <summary>
	/// Shows the single-field building blocks, groups, rows, buttons, messages and live values of the inspector
	/// toolbox next to plain fields, so that their look can be judged. Only used by tests and screenshots.
	/// </summary>
	public class InspectorToolboxDemo : ScriptableObject
	{
		[Header("Plain fields, drawn by Unity")]
		public string Name = "Demo";
		public int Count = 3;
		[Range(0f, 1f)] public float Ratio = 0.4f;
		public DemoQuality Quality = DemoQuality.Medium;
		public Vector3 Offset = new Vector3(1f, 2f, 3f);
		public Color Tint = new Color(0.9f, 0.6f, 0.2f);

		[Header("Field rules")]
		public float Duration = 2.5f;
		public string DisplayName = "Shown under another label";
		public int ReadOnlyNumber = 42;
		public string ReadOnlyText = "Cannot be edited";
		public string RequiredText = "";
		public int Percentage = 140;
		public bool HasDetails = true;
		public string Details = "Visible while Has Details is on";
		public float OnlyEditableWithDetails = 1f;

		[Header("Dropdowns")]
		public string Category = "Fruit/Apple";
		public string UnknownCategory = "Removed Entry";
		public string OptionalCategory = "";
		public DemoIdentifier Identifier = new DemoIdentifier("second");
		public int Level = 2;

		[Header("Field variants")]
		public bool ToggleOnTheLeft = true;
		public string OutputFolder = "Assets/Output";
		public Texture2D Icon;
		public ScriptableObject LinkedSettings;
		public DemoVersionSettings Version = new DemoVersionSettings { MinimumSupportedVersion = "1.2.0", WarnOnOlderVersions = true };

		[Header("Long labels")]
		public float ShortLabel = 1f;
		public float ALabelThatIsTooLongForOneLineInTheColumn = 2f;
		public float AnExtremelyLongLabelThatDoesNotEvenFitOnTwoLinesOfTheLabelColumnAndIsThereforeStackedAboveItsField = 3f;
		public string LongLabelOnATextFieldForComparison = "text";

		// Grouped fields, declared apart on purpose: groups collect them.
		public bool EnableShadows = true;
		public DemoQuality ShadowQuality = DemoQuality.High;
		[Range(0f, 1f)] public float ShadowStrength = 0.8f;
		public float FadeStartAngle = 5f;
		public float FadeEndAngle = 15f;
		public int BoxedNumber = 7;
		public string BoxedText = "In a box";
		public float FoldedValue = 0.5f;
		public string FoldedText = "In a foldout group";
		public float TitledValue = 12f;
		public string TitledText = "Under a title";
		public int AccentNumber = 5;
		public string AccentText = "In an accent group";
		public float GeneralSpeed = 3f;
		public float GeneralRange = 10f;
		public bool AdvancedFlag;
		public int AdvancedIterations = 8;
		public float RowLeft = 1f;
		public float RowRight = 2f;

		[NonSerialized] public float DebugTime = 0.35f;
		[NonSerialized] public int ClickCount;
		[NonSerialized] public List<string> RuntimeNames = new List<string> { "Alpha", "Beta", "Gamma" };
		[NonSerialized] public Dictionary<string, int> RuntimeScores = new Dictionary<string, int> { { "Alpha", 12 }, { "Beta", 7 } };

		public float SunAngle => Mathf.Lerp(-90f, 90f, DebugTime);
		public string Summary => $"{Name}: {Count} items at {Ratio:P0}, quality {Quality}.\nClicked {ClickCount} times.";
	}

}
