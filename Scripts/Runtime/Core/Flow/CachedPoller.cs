using UnityEngine;

namespace Extenity.FlowToolbox
{

	public class CachedPoller<TResult>
	{
		#region Initialization

		public CachedPoller(float interval)
		{
			Interval = interval;
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
				NextProcessTime = Time.realtimeSinceStartup + Interval;
			}
		}

		#endregion

		#region Timing

		public float NextProcessTime = float.MinValue;
		public float Interval;

		public bool IsTimeToProcess => NextProcessTime < Time.realtimeSinceStartup;

		public void Invalidate()
		{
			NextProcessTime = float.MinValue;
		}

		#endregion
	}

}
