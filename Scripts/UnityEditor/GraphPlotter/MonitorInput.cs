using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public class MonitorInput
	{
		private Monitor monitor;
		public string Description;
		public Color Color;

		public int sampleIndex;
		public int numberOfSamples = 1000;
		public float[] samples;
		public float[] times;
		public int[] frames;

		public MonitorInput(Monitor monitor, string description) : this(monitor, description, Color.red) { }
		public MonitorInput(Monitor monitor, string description, Color color)
		{
			this.monitor = monitor;
			Description = description;
			Color = color;

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

			monitor.Add(this);
		}

		public void Sample(float value)
		{
			float time = Time.time;
			int frame = Time.frameCount;

			samples[sampleIndex] = value;
			times[sampleIndex] = time;
			frames[sampleIndex] = frame;

			monitor.Resize(value, time);

			sampleIndex = (sampleIndex + 1) % numberOfSamples;
		}

		public void GetMinMax(float minTime, float maxTime, out float min, out float max)
		{
			min = float.PositiveInfinity;
			max = float.NegativeInfinity;

			for (int i = 0; i < samples.Length; i++)
			{
				float time = times[i];

				if (time >= minTime && time <= maxTime)
				{
					float value = samples[i];

					if (value < min)
					{
						min = value;
					}

					if (value > max)
					{
						max = value;
					}
				}
			}
		}

		public void GetMinMaxTime(out float minTime, out float maxTime)
		{
			minTime = float.PositiveInfinity;
			maxTime = float.NegativeInfinity;

			for (int i = 0; i < times.Length; i++)
			{
				var time = times[i];
				if (float.IsNaN(time))
				{
					continue;
				}

				if (minTime > time)
					minTime = time;
				if (maxTime < time)
					maxTime = time;
			}
		}

		public void Close()
		{
			monitor.Remove(this);
		}
	}

}