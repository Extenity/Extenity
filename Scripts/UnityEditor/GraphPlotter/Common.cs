using System;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public enum SampleTime
	{
		FixedUpdate = 0,
		Update = 1,
		LateUpdate = 2,

		/// <summary>
		/// Need to manually call Sample.
		/// </summary>
		Custom = 7,
	}

	public enum ValueAxisSizing
	{
		Fixed,
		Expansive,
		Adaptive
	};

	[Serializable]
	public class ValueAxisRangeConfiguration
	{
		public ValueAxisSizing Sizing;
		public float Min;
		public float Max;

		public float Span => Max - Min;

		#region Initialization

		public ValueAxisRangeConfiguration(ValueAxisSizing sizing, float min, float max)
		{
			Sizing = sizing;
			Min = min;
			Max = max;
		}

		public void CopyFrom(ValueAxisRangeConfiguration other)
		{
			Sizing = other.Sizing;
			Min = other.Min;
			Max = other.Max;
		}

		#endregion

		#region Operations

		public void Expand(float value)
		{
			if (Min > value)
				Min = value;
			if (Max < value)
				Max = value;
		}

		#endregion
	}

	public class PlotColors
	{
		public static Color Green = new Color(64f / 255f, 255f / 255f, 136f / 255f);
		public static Color Blue = new Color(64f / 255f, 234f / 255f, 255f / 255f);
		public static Color Purple = new Color(223f / 255f, 95f / 255f, 255f / 255f);
		public static Color Yellow = new Color(255f / 255f, 220f / 255f, 78f / 255f);
		public static Color Red = new Color(255f / 255f, 121f / 255f, 57f / 255f);

		public static readonly Color[] AllColors = { Green, Blue, Purple, Yellow, Red };
	}

}