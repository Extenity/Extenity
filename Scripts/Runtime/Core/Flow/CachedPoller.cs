using System;

namespace Extenity.FlowToolbox
{

	public class CachedPoller<TResult>
	{
		#region Initialization

		public CachedPoller(float interval, Func<float> timeGetter)
		{
			Interval = interval;
			TimeGetter = timeGetter;
		}

		#endregion

		#region Result

		TResult _CachedResult;
		public TResult CachedResult
		{
			get => _CachedResult;
			set
			{
				_CachedResult = value;
				NextProcessTime = TimeGetter() + Interval;
			}
		}

		#endregion

		#region Timing

		public float NextProcessTime = float.MinValue;
		public float Interval;
		private Func<float> TimeGetter;

		public bool IsTimeToProcess => NextProcessTime < TimeGetter();

		public void Invalidate()
		{
			NextProcessTime = float.MinValue;
		}

		#endregion
	}

}
