using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting
{

	public class Channel
	{
		#region Initialization

		public Channel(Graph graph, string description) :
			this(graph, description, Color.red)
		{
		}

		public Channel(Graph graph, string description, Color color)
		{
			Graph = graph;

			InitializeMetadata(description, color);
			InitializeData();

			Graph.RegisterChannel(this);
		}

		#endregion

		#region Deinitialization

		public void Close()
		{
			Graph.DeregisterChannel(this);
		}

		#endregion

		#region Monitor

		private Graph Graph;

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

		public int CurrentSampleIndex;
		public int SampleBufferSize = 1000;
		public float[] SampleAxisY;
		public float[] SampleAxisX;
		public int[] SampleFrames;

		private void InitializeData()
		{
			CurrentSampleIndex = 0;
			SampleAxisY = new float[SampleBufferSize];
			SampleAxisX = new float[SampleBufferSize];
			SampleFrames = new int[SampleBufferSize];

			for (int i = 0; i < SampleAxisY.Length; i++)
			{
				SampleAxisY[i] = float.NaN;
				SampleAxisX[i] = float.NaN;
				SampleFrames[i] = -1;
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
			SampleAxisY[CurrentSampleIndex] = value;
			SampleAxisX[CurrentSampleIndex] = time;
			SampleFrames[CurrentSampleIndex] = frame;

			Graph.InformNewEntry(value, time);

			CurrentSampleIndex = (CurrentSampleIndex + 1) % SampleBufferSize;
		}

		#endregion

		#region Data - Get

		public void GetValueRangeInTimeWindow(float timeStart, float timeEnd, out float min, out float max)
		{
			min = float.PositiveInfinity;
			max = float.NegativeInfinity;

			for (int i = 0; i < SampleAxisX.Length; i++)
			{
				var time = SampleAxisX[i];
				if (time >= timeStart && time <= timeEnd)
				{
					var value = SampleAxisY[i];
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

			for (int i = 0; i < SampleAxisX.Length; i++)
			{
				var time = SampleAxisX[i];
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