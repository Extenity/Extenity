#if UNITY_5_3_OR_NEWER

using Extenity.MessagingToolbox;

namespace Extenity.FlowToolbox
{

	public class LoopCallbacks
	{
		#region Callbacks

		public readonly ExtenityEvent TimeCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent NetworkingCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent InputUpdateCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent PreFixedUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PreUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PreLateUpdateCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent FixedUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent LateUpdateCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent PostFixedUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PostUpdateCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PostLateUpdateCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent UpdateEvery10FramesCallbacks = new ExtenityEvent();

		// Periodic, scaled (game) time. Pauses with Time.timeScale = 0.
		public readonly ExtenityEvent UpdateEvery100MillisecondsCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateEvery250MillisecondsCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateEvery500MillisecondsCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateEvery1000MillisecondsCallbacks = new ExtenityEvent();

		// Periodic, unscaled (wall-clock) time. Keeps ticking when Time.timeScale = 0.
		public readonly ExtenityEvent UpdateEvery100MillisecondsUnscaledCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateEvery250MillisecondsUnscaledCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateEvery500MillisecondsUnscaledCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent UpdateEvery1000MillisecondsUnscaledCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent CameraPlacementUpdateCallbacks = new ExtenityEvent();

		public readonly ExtenityEvent PreRenderCallbacks = new ExtenityEvent();
		public readonly ExtenityEvent PreUICallbacks = new ExtenityEvent();

		#endregion
	}

}

#endif
