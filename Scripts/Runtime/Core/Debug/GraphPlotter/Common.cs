using System;
using UnityEngine;

namespace Extenity.DebugFlowTool.GraphPlotting
{

	public enum CoordinateSystem { Local, World };
	public enum ScaleCoordinateSystem { Local, Lossy };

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
		Adaptive,

		/// <summary>
		/// Adaptive that always includes Zero axis.
		/// </summary>
		ZeroBasedAdaptive,
	};

	[Serializable]
	public class ValueAxisRangeConfiguration
	{
		public ValueAxisSizing Sizing;
		public float Min;
		public float Max;

		public float Span => Max - Min;

		#region Initialization

		public static ValueAxisRangeConfiguration CreateFixed(float min, float max)
		{
			return new ValueAxisRangeConfiguration(ValueAxisSizing.Fixed, min, max);
		}

		public static ValueAxisRangeConfiguration CreateExpansive(float min, float max)
		{
			return new ValueAxisRangeConfiguration(ValueAxisSizing.Expansive, min, max);
		}

		public static ValueAxisRangeConfiguration CreateAdaptive()
		{
			return new ValueAxisRangeConfiguration(ValueAxisSizing.Adaptive, float.PositiveInfinity, float.NegativeInfinity);
		}

		public static ValueAxisRangeConfiguration CreateZeroBasedAdaptive()
		{
			return new ValueAxisRangeConfiguration(ValueAxisSizing.ZeroBasedAdaptive, float.PositiveInfinity, float.NegativeInfinity);
		}

		private ValueAxisRangeConfiguration(ValueAxisSizing sizing, float min, float max)
		{
			Sizing = sizing;
			Min = min;
			Max = max;
		}

		public void CopyFrom(ValueAxisRangeConfiguration other)
		{
			if (other == null)
			{
				SetToDefault();
			}
			else
			{
				Sizing = other.Sizing;
				Min = other.Min;
				Max = other.Max;
			}
		}

		public void SetToDefault()
		{
			Sizing = ValueAxisSizing.Adaptive;
			Min = float.PositiveInfinity;
			Max = float.NegativeInfinity;
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
		public static Color Green = new Color(0.25f, 1.00f, 0.53f);
		public static Color Blue = new Color(0.25f, 0.92f, 1.00f);
		public static Color Purple = new Color(0.87f, 0.37f, 1.00f);
		public static Color Yellow = new Color(1.00f, 0.86f, 0.30f);
		public static Color Red = new Color(1.00f, 0.47f, 0.22f);

		public static readonly Color[] AllColors = { Green, Blue, Purple, Yellow, Red };
	}

}