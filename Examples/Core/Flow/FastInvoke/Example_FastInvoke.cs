using Extenity.FlowToolbox;
using UnityEngine;
using Logger = Extenity.Logger;

namespace ExtenityExamples.FlowToolbox
{

	public class Example_FastInvoke : MonoBehaviour
	{
		protected void Awake()
		{
			Time.timeScale = 0.5f;

			this.FastInvoke(Method, 3f);
			this.FastInvoke(UnscaledTimeMethod, 3f, true);
		}

		protected void Update()
		{
			var remainingTimeForMethod = this.RemainingTimeUntilNextFastInvoke(Method);
			var remainingTimeForUnscaledTimeMethod = this.RemainingTimeUntilNextUnscaledFastInvoke(UnscaledTimeMethod);
			if (!double.IsNaN(remainingTimeForMethod))
				Debug.LogFormat("{0:N3} \t Remaining time to 'Method': {1:N2}", Time.time, remainingTimeForMethod);
			if (!double.IsNaN(remainingTimeForUnscaledTimeMethod))
				Debug.LogFormat("{0:N3} \t Remaining time to 'UnscaledTimeMethod': {1:N2}", Time.unscaledTime, remainingTimeForUnscaledTimeMethod);
		}

		private void Method()
		{
			Log.Info("------------------------------------- 'Method' called.");
		}

		private void UnscaledTimeMethod()
		{
			Log.Info("------------------------------------- 'UnscaledTimeMethod' called.");
		}

		#region Log

		private static readonly Logger Log = new("Example");

		#endregion
	}

}
