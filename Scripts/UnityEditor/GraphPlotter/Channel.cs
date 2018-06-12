using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public class Channel
	{
		#region Initialization

		public Channel(Monitor monitor, string description) :
			this(monitor, description, Color.red)
		{
		}

		public Channel(Monitor monitor, string description, Color color)
		{
			Monitor = monitor;

			InitializeMetadata(description, color);
			InitializeData();

			Monitor.RegisterChannel(this);
		}

		#endregion

		#region Deinitialization

		public void Close()
		{
			Monitor.DeregisterChannel(this);
		}

		#endregion

		#region Monitor

		private Monitor Monitor;

		#endregion

		#region Metadata

		public string Description;
		public Color Color;

		private void InitializeMetadata(string description, Color color)
		{
			Description = description;
			Color = color;
		}

		#endregion

		#region Data

		public int sampleIndex;
		public int numberOfSamples = 1000;
		public float[] samples;
		public float[] times;
		public int[] frames;

		private void InitializeData()
		{
			sampleIndex = 0;
			samples = new float[numberOfSamples];
			times = new float[numberOfSamples];
			frames = new int[numberOfSamples];

			for (int i = 0; i < samples.Length; i++)
			{
				samples[i] = float.NaN;
				times[i] = float.NaN;
				frames[i] = -1;
			}
		}

		#endregion

		#region Data - Add

		public void Sample(float value)
		{
			Sample(value, Time.time, Time.frameCount);
		}

		public void Sample(float value, float time, int frame)
		{
			samples[sampleIndex] = value;
			times[sampleIndex] = time;
			frames[sampleIndex] = frame;

			Monitor.InformNewEntry(value, time);

			sampleIndex = (sampleIndex + 1) % numberOfSamples;
		}

		#endregion

		#region Data - Get

		public void GetValueRangeInTimeWindow(float timeStart, float timeEnd, out float min, out float max)
		{
			min = float.PositiveInfinity;
			max = float.NegativeInfinity;

			for (int i = 0; i < samples.Length; i++)
			{
				var time = times[i];
				if (time >= timeStart && time <= timeEnd)
				{
					var value = samples[i];
					if (min > value)
						min = value;
					if (max < value)
						max = value;
				}
			}
		}

		public void GetMinMaxTime(out float timeStart, out float timeEnd)
		{
			var start = float.PositiveInfinity;
			var end = float.NegativeInfinity;

			for (int i = 0; i < times.Length; i++)
			{
				var time = times[i];
				if (float.IsNaN(time))
					continue;

				if (start > time)
					start = time;
				if (end < time)
					end = time;
			}

			timeStart = start;
			timeEnd = end;
		}

		#endregion
	}

}