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

		public readonly ExtenityEvent PostUIToolkitRepaintCallbacks = new ExtenityEvent();

		#endregion

		#region All Callback Lists

		/// <summary>
		/// Every callback list above, paired with its name. This is the one place that enumerates them, so anything
		/// that needs to go through all callback lists (like the cleanup checks in <see cref="Loop"/>) stays in sync
		/// when a new list is added.
		/// </summary>
		public readonly (string Name, ExtenityEvent Callbacks)[] AllCallbackLists;

		public LoopCallbacks()
		{
			AllCallbackLists = new[]
			{
				("Time", TimeCallbacks),
				("Networking", NetworkingCallbacks),
				("InputUpdate", InputUpdateCallbacks),

				("PreFixedUpdate", PreFixedUpdateCallbacks),
				("PreUpdate", PreUpdateCallbacks),
				("PreLateUpdate", PreLateUpdateCallbacks),

				("FixedUpdate", FixedUpdateCallbacks),
				("Update", UpdateCallbacks),
				("LateUpdate", LateUpdateCallbacks),

				("PostFixedUpdate", PostFixedUpdateCallbacks),
				("PostUpdate", PostUpdateCallbacks),
				("PostLateUpdate", PostLateUpdateCallbacks),

				("UpdateEvery10Frames", UpdateEvery10FramesCallbacks),
				("UpdateEvery100Milliseconds", UpdateEvery100MillisecondsCallbacks),
				("UpdateEvery100MillisecondsUnscaled", UpdateEvery100MillisecondsUnscaledCallbacks),
				("UpdateEvery250Milliseconds", UpdateEvery250MillisecondsCallbacks),
				("UpdateEvery250MillisecondsUnscaled", UpdateEvery250MillisecondsUnscaledCallbacks),
				("UpdateEvery500Milliseconds", UpdateEvery500MillisecondsCallbacks),
				("UpdateEvery500MillisecondsUnscaled", UpdateEvery500MillisecondsUnscaledCallbacks),
				("UpdateEvery1000Milliseconds", UpdateEvery1000MillisecondsCallbacks),
				("UpdateEvery1000MillisecondsUnscaled", UpdateEvery1000MillisecondsUnscaledCallbacks),

				("CameraPlacementUpdate", CameraPlacementUpdateCallbacks),

				("PreRender", PreRenderCallbacks),
				("PreUI", PreUICallbacks),

				("PostUIToolkitRepaint", PostUIToolkitRepaintCallbacks),
			};
		}

		#endregion
	}

}

#endif
