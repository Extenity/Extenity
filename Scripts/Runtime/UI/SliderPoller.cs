using System;
using Extenity.FlowToolbox;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class SliderPoller : MonoBehaviour
	{
		public Slider Slider;
		public float PollInterval = 1f;

		/// <summary>
		/// This method is called for getting the value that will be assigned to the slider on each polling.
		/// Must be set for the poller to work.
		/// </summary>
		public Func<float> AcquireValue;

		protected void OnEnable()
		{
			this.FastInvokeRepeating(OnTimeToPoll, PollInterval, PollInterval, true);
		}

		protected void OnDisable()
		{
			this.CancelFastInvoke(OnTimeToPoll);
		}

		private void OnTimeToPoll()
		{
			if (Slider && AcquireValue != null)
			{
				Slider.value = AcquireValue();
			}
		}
	}

}
