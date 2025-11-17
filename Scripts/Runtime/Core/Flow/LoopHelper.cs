#if UNITY_5_3_OR_NEWER

using Extenity.MessagingToolbox;

namespace Extenity.FlowToolbox
{

	public class LoopHelper
	{
		#region Callbacks

		public readonly ExtenityEvent PreFixedUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PreUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PreLateUpdateCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent FixedUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent LateUpdateCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent PostFixedUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PostUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PostLateUpdateCallbacks = new ExtenityEvent();

		#endregion

		#region Counters

		/* TODO: This functionality became lost when switching to use PlayerLoop API, instead of MonoBehaviour callbacks. It will be reimplemented later.
		[Title("Stats")]
		[NonSerialized, ShowInInspector]
		public int UpdateCount;
		[NonSerialized, ShowInInspector]
		public int FixedUpdateCount;
		[NonSerialized, ShowInInspector]
		public int LateUpdateCount;
		*/

		#endregion

		#region FPS Analyzer

		/* TODO: This functionality became lost when switching to use PlayerLoop API, instead of MonoBehaviour callbacks. It will be reimplemented later.
		[Title("FPS Analyzer")]
		[NonSerialized, ShowInInspector, InlineProperty, HideLabel]
		public TickAnalyzer FPSAnalyzer;
		private bool _IsFPSAnalyzerEnabled;

		[Button(ButtonSizes.Large), ButtonGroup("ToggleFPSAnalyzer"), DisableIf(nameof(_IsFPSAnalyzerEnabled))]
		public void EnableFPSAnalyzer()
		{
			EnableFPSAnalyzer(true);
		}

		[Button(ButtonSizes.Large), ButtonGroup("ToggleFPSAnalyzer"), EnableIf(nameof(_IsFPSAnalyzerEnabled))]
		public void DisableFPSAnalyzer()
		{
			EnableFPSAnalyzer(false);
		}

		public void EnableFPSAnalyzer(bool enable)
		{
			if (enable == _IsFPSAnalyzerEnabled)
				return;

			_IsFPSAnalyzerEnabled = enable;
			if (enable)
			{
				FPSAnalyzer = new TickAnalyzer(
					new TickPlotter("FPS", VerticalRange.ZeroBasedAdaptive(), gameObject),
					Loop.Time,
					TickAnalyzer.HistorySizeFor(60, 5));
			}
			else
			{
				// Deinitialize existing one.
				FPSAnalyzer = null;
			}
		}
		*/

		#endregion
	}

}

#endif
