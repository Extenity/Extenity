#if UNITY_5_3_OR_NEWER

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Extenity.MathToolbox;
using Extenity.MessagingToolbox;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Extenity.FlowToolbox
{

	public static class Loop
	{
		#region Singleton

		public static LoopCallbacks Instance;

		#endregion

		#region Counters

		public static int FixedUpdateCount;

		#endregion

		#region Constants

		/// <summary>How many frames between consecutive invocations of <see cref="RegisterUpdateEvery10Frames"/> callbacks. The callback fires on frames where <c>FrameCount % UpdateEvery10FramesPeriod == 0</c>.</summary>
		private const int UpdateEvery10FramesPeriod = 10;

		private const float UpdateEvery100MillisecondsPeriod  = 0.1f;
		private const float UpdateEvery250MillisecondsPeriod  = 0.25f;
		private const float UpdateEvery500MillisecondsPeriod  = 0.5f;
		private const float UpdateEvery1000MillisecondsPeriod = 1.0f;

		#endregion

		#region Periodic Update Last-Fire Timestamps

		// Scaled (game) time, using Loop.Time. Affected by Time.timeScale.
		private static float _LastFire_UpdateEvery100Milliseconds;
		private static float _LastFire_UpdateEvery250Milliseconds;
		private static float _LastFire_UpdateEvery500Milliseconds;
		private static float _LastFire_UpdateEvery1000Milliseconds;

		// Unscaled (wall-clock) time, using Loop.UnscaledTime. Keeps ticking when Time.timeScale = 0.
		private static float _LastFire_UpdateEvery100MillisecondsUnscaled;
		private static float _LastFire_UpdateEvery250MillisecondsUnscaled;
		private static float _LastFire_UpdateEvery500MillisecondsUnscaled;
		private static float _LastFire_UpdateEvery1000MillisecondsUnscaled;

		private static void InitializePeriodicUpdateTimers()
		{
			_LastFire_UpdateEvery100Milliseconds          = Time;
			_LastFire_UpdateEvery250Milliseconds          = Time;
			_LastFire_UpdateEvery500Milliseconds          = Time;
			_LastFire_UpdateEvery1000Milliseconds         = Time;
			_LastFire_UpdateEvery100MillisecondsUnscaled  = UnscaledTime;
			_LastFire_UpdateEvery250MillisecondsUnscaled  = UnscaledTime;
			_LastFire_UpdateEvery500MillisecondsUnscaled  = UnscaledTime;
			_LastFire_UpdateEvery1000MillisecondsUnscaled = UnscaledTime;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void CheckAndFirePeriodic(ref float lastFire, float now, float period, ExtenityEvent callbacks)
		{
			var elapsed = now - lastFire;
			if (elapsed >= period)
			{
				// Advance 'lastFire' by exactly one period so the cadence stays anchored
				// to a fixed grid (lastFire + N*period) instead of drifting forward by
				// frame jitter on each fire. If more than one period was missed
				// (editor pause/resume, hitch, freeze), jump to 'now' so we don't burn
				// the next several frames firing catch-up calls.
				lastFire = elapsed >= 2f * period
					? now
					: lastFire + period;

				InvokeSafeIfEnabled(callbacks);
			}
		}

		#endregion

		#region Initialization

		// Instantiating game objects in SubsystemRegistration and AfterAssembliesLoaded is a bad idea.
		// It works in Editor but observed not working in Windows and Android builds and probably other
		// platforms too. Game objects are destroyed just before BeforeSceneLoad for some reason.
		// So decided to initialize our subsystems at BeforeSceneLoad stage. See 119392241.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Instantiate()
		{
			// Debug.Assert(Instance == null); Decided not to deinitialize Loop system anytime. If it's up, it stays up until application quits, or Editor recompiles.
			InitializeSystem();
		}

		public static void InitializeSystem()
		{
			// DeinitializeSystem(); Decided not to deinitialize Loop system anytime. If it's up, it stays up until application quits, or Editor recompiles.
			if (Instance != null)
			{
				return;
			}

			Invoker.InitializeSystem();

			// Initialize cached times here at start.
			// Otherwise, cached time initialization will be delayed until Unity calls one of LoopHelper's Update methods.
			SetCachedTimesFromUnityTimes();

			// Seed periodic-update timers from "now" so the first fire happens one period after startup,
			// not on the first frame (which is already the heaviest frame).
			InitializePeriodicUpdateTimers();

			Instance = new LoopCallbacks();

			// Inject custom update callbacks into Unity's PlayerLoop
			var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
			InjectIntoPlayerLoop(ref playerLoop);
			PlayerLoop.SetPlayerLoop(playerLoop);
		}

		#endregion

		#region Deinitialization

		public static void DeinitializeSystem()
		{
			Invoker.DeinitializeSystem();

			if (Instance != null)
			{
				// Remove custom callbacks from Unity's PlayerLoop
				var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
				RemoveFromPlayerLoop(ref playerLoop);
				PlayerLoop.SetPlayerLoop(playerLoop);

				Instance = null;
			}

			ResetCachedTimes();
		}

		#endregion

		#region Callback Registration

		// @formatter:off

		/// <summary>Registers a callback for custom time calculations. Runs right after Unity updates its internal time values, before any Update callbacks. Use this to implement custom time management or time scaling.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterTime                 (Action callback, int order = 0) { Instance.TimeCallbacks                 .AddListener(callback, order); }

		/// <summary>Registers a callback for networking operations. Runs right after Time callbacks and before any Update callbacks. Network updates happen early so game logic can consume fresh network data.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterNetworking           (Action callback, int order = 0) { Instance.NetworkingCallbacks           .AddListener(callback, order); }

		/// <summary>Registers a callback for input update operations. Runs after Networking callbacks and before PreFixedUpdate. This is where hardware input is processed and its data is made available to the application, ensuring all game logic operations have access to updated input without delay.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterInputUpdate          (Action callback, int order = 0) { Instance.InputUpdateCallbacks          .AddListener(callback, order); }

		/// <summary>Registers a callback to run before FixedUpdate. Runs before Unity's MonoBehaviour FixedUpdate callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPreFixedUpdate       (Action callback, int order = 0) { Instance.PreFixedUpdateCallbacks       .AddListener(callback, order); }

		/// <summary>Registers a callback to run before Update. Runs before Unity's MonoBehaviour Update callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPreUpdate            (Action callback, int order = 0) { Instance.PreUpdateCallbacks            .AddListener(callback, order); }

		/// <summary>Registers a callback to run before LateUpdate. Runs before Unity's MonoBehaviour LateUpdate callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPreLateUpdate        (Action callback, int order = 0) { Instance.PreLateUpdateCallbacks        .AddListener(callback, order); }

		/// <summary>Registers a callback to run during FixedUpdate. Runs after Unity's MonoBehaviour FixedUpdate callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterFixedUpdate          (Action callback, int order = 0) { Instance.FixedUpdateCallbacks          .AddListener(callback, order); }

		/// <summary>Registers a callback to run during Update. Runs after Unity's MonoBehaviour Update callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdate               (Action callback, int order = 0) { Instance.UpdateCallbacks               .AddListener(callback, order); }

		/// <summary>Registers a callback to run during LateUpdate. Runs after Unity's MonoBehaviour LateUpdate callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterLateUpdate           (Action callback, int order = 0) { Instance.LateUpdateCallbacks           .AddListener(callback, order); }

		/// <summary>Registers a callback to run after FixedUpdate. Runs after Unity's MonoBehaviour FixedUpdate callbacks and after FixedUpdate callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPostFixedUpdate      (Action callback, int order = 0) { Instance.PostFixedUpdateCallbacks      .AddListener(callback, order); }
		/// <summary>Registers a callback to run after Update. Runs after Unity's MonoBehaviour Update callbacks and after Update callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPostUpdate           (Action callback, int order = 0) { Instance.PostUpdateCallbacks           .AddListener(callback, order); }
		/// <summary>Registers a callback to run after LateUpdate. Runs after Unity's MonoBehaviour LateUpdate callbacks and after LateUpdate callbacks.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPostLateUpdate       (Action callback, int order = 0) { Instance.PostLateUpdateCallbacks       .AddListener(callback, order); }

		/// <summary>Registers a callback that runs once every <see cref="UpdateEvery10FramesPeriod"/> frames, right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Cadence is gated on <c>FrameCount % 10 == 0</c>, so every registered callback fires together on the same frame.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery10Frames                (Action callback, int order = 0) { Instance.UpdateEvery10FramesCallbacks                .AddListener(callback, order); }

		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery100MillisecondsPeriod"/> seconds of <b>scaled</b> game time (<see cref="Time"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Affected by <c>Time.timeScale</c> — the cadence slows and pauses with the game. See <see cref="RegisterUpdateEvery100MillisecondsUnscaled"/> for a wall-clock variant.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery100Milliseconds         (Action callback, int order = 0) { Instance.UpdateEvery100MillisecondsCallbacks         .AddListener(callback, order); }
		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery100MillisecondsPeriod"/> seconds of <b>unscaled</b> wall-clock time (<see cref="UnscaledTime"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Keeps ticking when <c>Time.timeScale = 0</c>; the right default for UI/diagnostics work that shouldn't pause with the game. See <see cref="RegisterUpdateEvery100Milliseconds"/> for a variant that pauses with the game.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery100MillisecondsUnscaled (Action callback, int order = 0) { Instance.UpdateEvery100MillisecondsUnscaledCallbacks .AddListener(callback, order); }

		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery250MillisecondsPeriod"/> seconds of <b>scaled</b> game time (<see cref="Time"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Affected by <c>Time.timeScale</c> — the cadence slows and pauses with the game. See <see cref="RegisterUpdateEvery250MillisecondsUnscaled"/> for a wall-clock variant.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery250Milliseconds         (Action callback, int order = 0) { Instance.UpdateEvery250MillisecondsCallbacks         .AddListener(callback, order); }
		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery250MillisecondsPeriod"/> seconds of <b>unscaled</b> wall-clock time (<see cref="UnscaledTime"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Keeps ticking when <c>Time.timeScale = 0</c>; the right default for UI/diagnostics work that shouldn't pause with the game. See <see cref="RegisterUpdateEvery250Milliseconds"/> for a variant that pauses with the game.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery250MillisecondsUnscaled (Action callback, int order = 0) { Instance.UpdateEvery250MillisecondsUnscaledCallbacks .AddListener(callback, order); }

		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery500MillisecondsPeriod"/> seconds of <b>scaled</b> game time (<see cref="Time"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Affected by <c>Time.timeScale</c> — the cadence slows and pauses with the game. See <see cref="RegisterUpdateEvery500MillisecondsUnscaled"/> for a wall-clock variant.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery500Milliseconds         (Action callback, int order = 0) { Instance.UpdateEvery500MillisecondsCallbacks         .AddListener(callback, order); }
		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery500MillisecondsPeriod"/> seconds of <b>unscaled</b> wall-clock time (<see cref="UnscaledTime"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Keeps ticking when <c>Time.timeScale = 0</c>; the right default for UI/diagnostics work that shouldn't pause with the game. See <see cref="RegisterUpdateEvery500Milliseconds"/> for a variant that pauses with the game.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery500MillisecondsUnscaled (Action callback, int order = 0) { Instance.UpdateEvery500MillisecondsUnscaledCallbacks .AddListener(callback, order); }

		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery1000MillisecondsPeriod"/> seconds of <b>scaled</b> game time (<see cref="Time"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Affected by <c>Time.timeScale</c> — the cadence slows and pauses with the game. See <see cref="RegisterUpdateEvery1000MillisecondsUnscaled"/> for a wall-clock variant.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery1000Milliseconds        (Action callback, int order = 0) { Instance.UpdateEvery1000MillisecondsCallbacks        .AddListener(callback, order); }
		/// <summary>Registers a callback that runs at most once every <see cref="UpdateEvery1000MillisecondsPeriod"/> seconds of <b>unscaled</b> wall-clock time (<see cref="UnscaledTime"/>), right after PostUpdate. Intended for code that doesn't need to run every frame and can tolerate the resulting latency (e.g. periodic refreshes, low-frequency polling, throttled diagnostics). Keeps ticking when <c>Time.timeScale = 0</c>; the right default for UI/diagnostics work that shouldn't pause with the game. See <see cref="RegisterUpdateEvery1000Milliseconds"/> for a variant that pauses with the game.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterUpdateEvery1000MillisecondsUnscaled(Action callback, int order = 0) { Instance.UpdateEvery1000MillisecondsUnscaledCallbacks.AddListener(callback, order); }

		/// <summary>Registers a callback for camera placement updates. Runs after PostLateUpdate and before PreRender, allowing all game logic calculations to complete before camera positioning is finalized. This ensures that all PreRender operations know the camera is properly positioned.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterCameraPlacementUpdate(Action callback, int order = 0) { Instance.CameraPlacementUpdateCallbacks.AddListener(callback, order); }

		/// <summary>Registers a callback for setting rendering-related data. Runs after all update logic but before rendering begins. Use this to update shader properties, material values, or other render state that should be set right before rendering.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPreRender            (Action callback, int order = 0) { Instance.PreRenderCallbacks            .AddListener(callback, order); }

		/// <summary>Registers a callback for setting UI rendering-related data. Runs after PreRender callbacks and before rendering begins. Use this to update UI element properties that should be set right before UI rendering. UI typically renders on top of the scene.</summary>
		/// <param name="callback">The callback method to register.</param>
		/// <param name="order">Lesser ordered callbacks are called earlier. Negative values are allowed. Callbacks that have the same order are called in registration order. You can easily see order of all callbacks in Tools>Extenity>Application>Loop window.</param>
		public static void RegisterPreUI                (Action callback, int order = 0) { Instance.PreUICallbacks                .AddListener(callback, order); }

		// @formatter:on

		#endregion

		#region Callback Deregistration

		// @formatter:off

		public static void DeregisterTime                 (Action callback) { Instance.TimeCallbacks                 .RemoveListener(callback); }
		public static void DeregisterNetworking           (Action callback) { Instance.NetworkingCallbacks           .RemoveListener(callback); }
		public static void DeregisterInputUpdate          (Action callback) { Instance.InputUpdateCallbacks          .RemoveListener(callback); }
		public static void DeregisterPreFixedUpdate       (Action callback) { Instance.PreFixedUpdateCallbacks       .RemoveListener(callback); }
		public static void DeregisterPreUpdate            (Action callback) { Instance.PreUpdateCallbacks            .RemoveListener(callback); }
		public static void DeregisterPreLateUpdate        (Action callback) { Instance.PreLateUpdateCallbacks        .RemoveListener(callback); }
		public static void DeregisterFixedUpdate          (Action callback) { Instance.FixedUpdateCallbacks          .RemoveListener(callback); }
		public static void DeregisterUpdate               (Action callback) { Instance.UpdateCallbacks               .RemoveListener(callback); }
		public static void DeregisterLateUpdate           (Action callback) { Instance.LateUpdateCallbacks           .RemoveListener(callback); }
		public static void DeregisterPostFixedUpdate      (Action callback) { Instance.PostFixedUpdateCallbacks      .RemoveListener(callback); }
		public static void DeregisterPostUpdate           (Action callback) { Instance.PostUpdateCallbacks           .RemoveListener(callback); }
		public static void DeregisterPostLateUpdate                     (Action callback) { Instance.PostLateUpdateCallbacks                     .RemoveListener(callback); }
		public static void DeregisterUpdateEvery10Frames                (Action callback) { Instance.UpdateEvery10FramesCallbacks                .RemoveListener(callback); }
		public static void DeregisterUpdateEvery100Milliseconds         (Action callback) { Instance.UpdateEvery100MillisecondsCallbacks         .RemoveListener(callback); }
		public static void DeregisterUpdateEvery100MillisecondsUnscaled (Action callback) { Instance.UpdateEvery100MillisecondsUnscaledCallbacks .RemoveListener(callback); }
		public static void DeregisterUpdateEvery250Milliseconds         (Action callback) { Instance.UpdateEvery250MillisecondsCallbacks         .RemoveListener(callback); }
		public static void DeregisterUpdateEvery250MillisecondsUnscaled (Action callback) { Instance.UpdateEvery250MillisecondsUnscaledCallbacks .RemoveListener(callback); }
		public static void DeregisterUpdateEvery500Milliseconds         (Action callback) { Instance.UpdateEvery500MillisecondsCallbacks         .RemoveListener(callback); }
		public static void DeregisterUpdateEvery500MillisecondsUnscaled (Action callback) { Instance.UpdateEvery500MillisecondsUnscaledCallbacks .RemoveListener(callback); }
		public static void DeregisterUpdateEvery1000Milliseconds        (Action callback) { Instance.UpdateEvery1000MillisecondsCallbacks        .RemoveListener(callback); }
		public static void DeregisterUpdateEvery1000MillisecondsUnscaled(Action callback) { Instance.UpdateEvery1000MillisecondsUnscaledCallbacks.RemoveListener(callback); }
		public static void DeregisterCameraPlacementUpdate              (Action callback) { Instance.CameraPlacementUpdateCallbacks              .RemoveListener(callback); }
		public static void DeregisterPreRender            (Action callback) { Instance.PreRenderCallbacks            .RemoveListener(callback); }
		public static void DeregisterPreUI                (Action callback) { Instance.PreUICallbacks                .RemoveListener(callback); }

		// @formatter:on

		#endregion

		#region PlayerLoop Injection

		private static void InjectIntoPlayerLoop(ref PlayerLoopSystem playerLoop)
		{
			InsertLoopSystemAfter<TimeUpdate>(ref playerLoop, typeof(TimeUpdate.WaitForLastPresentationAndUpdateTime), CreateLoopSystem<TimeRunner>());
			InsertLoopSystemAfter<TimeUpdate>(ref playerLoop, typeof(TimeRunner), CreateLoopSystem<NetworkingRunner>());
			InsertLoopSystemAfter<TimeUpdate>(ref playerLoop, typeof(NetworkingRunner), CreateLoopSystem<InputUpdateRunner>());

			InsertLoopSystemBefore<FixedUpdate>(ref playerLoop, typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate), CreateLoopSystem<PreFixedUpdateRunner>());
			InsertLoopSystemAfter<FixedUpdate>(ref playerLoop, typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate), CreateLoopSystem<FixedUpdateRunner>());
			InsertLoopSystemAfter<FixedUpdate>(ref playerLoop, typeof(FixedUpdateRunner), CreateLoopSystem<PostFixedUpdateRunner>());

			InsertLoopSystemBefore<Update>(ref playerLoop, typeof(Update.ScriptRunBehaviourUpdate), CreateLoopSystem<PreUpdateRunner>());
			InsertLoopSystemAfter<Update>(ref playerLoop, typeof(Update.ScriptRunBehaviourUpdate), CreateLoopSystem<UpdateRunner>());
			InsertLoopSystemAfter<Update>(ref playerLoop, typeof(UpdateRunner), CreateLoopSystem<PostUpdateRunner>());
			InsertLoopSystemAfter<Update>(ref playerLoop, typeof(PostUpdateRunner), CreateLoopSystem<InfrequentUpdatesRunner>());

			InsertLoopSystemBefore<PreLateUpdate>(ref playerLoop, typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), CreateLoopSystem<PreLateUpdateRunner>());
			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), CreateLoopSystem<LateUpdateRunner>());
			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(LateUpdateRunner), CreateLoopSystem<PostLateUpdateRunner>());

			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(PostLateUpdateRunner), CreateLoopSystem<CameraPlacementUpdateRunner>());

			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(CameraPlacementUpdateRunner), CreateLoopSystem<PreRenderRunner>());
			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(PreRenderRunner), CreateLoopSystem<PreUIRunner>());
		}

		private static void RemoveFromPlayerLoop(ref PlayerLoopSystem playerLoop)
		{
			RemoveLoopSystem<TimeUpdate>(ref playerLoop, typeof(TimeRunner));
			RemoveLoopSystem<TimeUpdate>(ref playerLoop, typeof(NetworkingRunner));
			RemoveLoopSystem<TimeUpdate>(ref playerLoop, typeof(InputUpdateRunner));

			RemoveLoopSystem<FixedUpdate>(ref playerLoop, typeof(PreFixedUpdateRunner));
			RemoveLoopSystem<FixedUpdate>(ref playerLoop, typeof(FixedUpdateRunner));
			RemoveLoopSystem<FixedUpdate>(ref playerLoop, typeof(PostFixedUpdateRunner));

			RemoveLoopSystem<Update>(ref playerLoop, typeof(PreUpdateRunner));
			RemoveLoopSystem<Update>(ref playerLoop, typeof(UpdateRunner));
			RemoveLoopSystem<Update>(ref playerLoop, typeof(PostUpdateRunner));
			RemoveLoopSystem<Update>(ref playerLoop, typeof(InfrequentUpdatesRunner));

			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(PreLateUpdateRunner));
			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(LateUpdateRunner));
			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(PostLateUpdateRunner));
			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(CameraPlacementUpdateRunner));
			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(PreRenderRunner));
			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(PreUIRunner));
		}

		private static PlayerLoopSystem CreateLoopSystem<T>() where T : struct
		{
			return new PlayerLoopSystem
			{
				type = typeof(T),
				updateDelegate = GetUpdateDelegateFor<T>()
			};
		}

		private static void InsertLoopSystemBefore<TParent>(ref PlayerLoopSystem rootLoop, Type beforeType, PlayerLoopSystem systemToInsert)
		{
			for (int i = 0; i < rootLoop.subSystemList.Length; i++)
			{
				if (rootLoop.subSystemList[i].type == typeof(TParent))
				{
					var subsystems = rootLoop.subSystemList[i].subSystemList;
					for (int j = 0; j < subsystems.Length; j++)
					{
						if (subsystems[j].type == beforeType)
						{
							var newSubsystems = new PlayerLoopSystem[subsystems.Length + 1];
							Array.Copy(subsystems, 0, newSubsystems, 0, j);
							newSubsystems[j] = systemToInsert;
							Array.Copy(subsystems, j, newSubsystems, j + 1, subsystems.Length - j);
							rootLoop.subSystemList[i].subSystemList = newSubsystems;
							return;
						}
					}
				}
			}
		}

		private static void InsertLoopSystemAfter<TParent>(ref PlayerLoopSystem rootLoop, Type afterType, PlayerLoopSystem systemToInsert)
		{
			for (int i = 0; i < rootLoop.subSystemList.Length; i++)
			{
				if (rootLoop.subSystemList[i].type == typeof(TParent))
				{
					var subsystems = rootLoop.subSystemList[i].subSystemList;
					for (int j = 0; j < subsystems.Length; j++)
					{
						if (subsystems[j].type == afterType)
						{
							var newSubsystems = new PlayerLoopSystem[subsystems.Length + 1];
							Array.Copy(subsystems, 0, newSubsystems, 0, j + 1);
							newSubsystems[j + 1] = systemToInsert;
							Array.Copy(subsystems, j + 1, newSubsystems, j + 2, subsystems.Length - j - 1);
							rootLoop.subSystemList[i].subSystemList = newSubsystems;
							return;
						}
					}
				}
			}
		}

		private static void RemoveLoopSystem<TParent>(ref PlayerLoopSystem rootLoop, Type systemType)
		{
			for (int i = 0; i < rootLoop.subSystemList.Length; i++)
			{
				if (rootLoop.subSystemList[i].type == typeof(TParent))
				{
					var subsystems = rootLoop.subSystemList[i].subSystemList;
					for (int j = 0; j < subsystems.Length; j++)
					{
						if (subsystems[j].type == systemType)
						{
							var newSubsystems = new PlayerLoopSystem[subsystems.Length - 1];
							Array.Copy(subsystems, 0, newSubsystems, 0, j);
							Array.Copy(subsystems, j + 1, newSubsystems, j, subsystems.Length - j - 1);
							rootLoop.subSystemList[i].subSystemList = newSubsystems;
							return;
						}
					}
				}
			}
		}

		private static PlayerLoopSystem.UpdateFunction GetUpdateDelegateFor<T>() where T : struct
		{
			// @formatter:off

			if (typeof(T) == typeof(TimeRunner                 )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.TimeCallbacks); };
			if (typeof(T) == typeof(NetworkingRunner           )) return () => {                                 InvokeSafeIfEnabled(Instance.NetworkingCallbacks); };
			if (typeof(T) == typeof(InputUpdateRunner          )) return () => {                                 InvokeSafeIfEnabled(Instance.InputUpdateCallbacks); };
			if (typeof(T) == typeof(PreFixedUpdateRunner       )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PreFixedUpdateCallbacks); };
			if (typeof(T) == typeof(FixedUpdateRunner          )) return () => { SetCachedTimesFromUnityTimes(); FixedUpdateCount++; Invoker.Handler.CustomFixedUpdate(Time); InvokeSafeIfEnabled(Instance.FixedUpdateCallbacks); };
			if (typeof(T) == typeof(PostFixedUpdateRunner      )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PostFixedUpdateCallbacks); };
			if (typeof(T) == typeof(PreUpdateRunner            )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PreUpdateCallbacks); };
			if (typeof(T) == typeof(UpdateRunner               )) return () => { SetCachedTimesFromUnityTimes(); Invoker.Handler.CustomUpdate(UnscaledTime); InvokeSafeIfEnabled(Instance.UpdateCallbacks); };
			if (typeof(T) == typeof(PostUpdateRunner           )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PostUpdateCallbacks); };
			if (typeof(T) == typeof(InfrequentUpdatesRunner    )) return () =>
			{
				SetCachedTimesFromUnityTimes();
				// Frame-based: all callbacks fire together on frames where FrameCount % 10 == 0.
				if (FrameCount % UpdateEvery10FramesPeriod == 0) 
				{
					InvokeSafeIfEnabled(Instance.UpdateEvery10FramesCallbacks);
				}
				var t = Time;
				var u = UnscaledTime;
				// Time-based, scaled (Loop.Time). Pauses with Time.timeScale = 0. Fire fastest-to-slowest.
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery100Milliseconds,          t, UpdateEvery100MillisecondsPeriod,  Instance.UpdateEvery100MillisecondsCallbacks);
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery250Milliseconds,          t, UpdateEvery250MillisecondsPeriod,  Instance.UpdateEvery250MillisecondsCallbacks);
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery500Milliseconds,          t, UpdateEvery500MillisecondsPeriod,  Instance.UpdateEvery500MillisecondsCallbacks);
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery1000Milliseconds,         t, UpdateEvery1000MillisecondsPeriod, Instance.UpdateEvery1000MillisecondsCallbacks);
				// Time-based, unscaled (Loop.UnscaledTime). Keeps ticking through pause.
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery100MillisecondsUnscaled,  u, UpdateEvery100MillisecondsPeriod,  Instance.UpdateEvery100MillisecondsUnscaledCallbacks);
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery250MillisecondsUnscaled,  u, UpdateEvery250MillisecondsPeriod,  Instance.UpdateEvery250MillisecondsUnscaledCallbacks);
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery500MillisecondsUnscaled,  u, UpdateEvery500MillisecondsPeriod,  Instance.UpdateEvery500MillisecondsUnscaledCallbacks);
				CheckAndFirePeriodic(ref _LastFire_UpdateEvery1000MillisecondsUnscaled, u, UpdateEvery1000MillisecondsPeriod, Instance.UpdateEvery1000MillisecondsUnscaledCallbacks);
			};
			if (typeof(T) == typeof(PreLateUpdateRunner        )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PreLateUpdateCallbacks); };
			if (typeof(T) == typeof(LateUpdateRunner           )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.LateUpdateCallbacks); };
			if (typeof(T) == typeof(PostLateUpdateRunner       )) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PostLateUpdateCallbacks); };
			if (typeof(T) == typeof(CameraPlacementUpdateRunner)) return () => {                                 InvokeSafeIfEnabled(Instance.CameraPlacementUpdateCallbacks); };
			if (typeof(T) == typeof(PreRenderRunner            )) return () => {                                 InvokeSafeIfEnabled(Instance.PreRenderCallbacks); };
			if (typeof(T) == typeof(PreUIRunner                )) return () => {                                 InvokeSafeIfEnabled(Instance.PreUICallbacks); };

			// @formatter:on

			throw new NotImplementedException($"No update delegate defined for type {typeof(T)}");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void InvokeSafeIfEnabled(ExtenityEvent extenityEvent)
		{
			if (EnableCatchingExceptionsInUpdateCallbacks)
			{
				extenityEvent.InvokeSafe();
			}
			else
			{
				extenityEvent.InvokeUnsafe();
			}
		}

		// Marker types for PlayerLoop injection
		private struct TimeRunner { }
		private struct NetworkingRunner { }
		private struct InputUpdateRunner { }

		private struct PreFixedUpdateRunner { }
		private struct FixedUpdateRunner { }
		private struct PostFixedUpdateRunner { }

		private struct PreUpdateRunner { }
		private struct UpdateRunner { }
		private struct PostUpdateRunner { }
		private struct InfrequentUpdatesRunner { }

		private struct PreLateUpdateRunner { }
		private struct LateUpdateRunner { }
		private struct PostLateUpdateRunner { }

		private struct CameraPlacementUpdateRunner { }

		private struct PreRenderRunner { }
		private struct PreUIRunner { }

		#endregion

		#region Manually Run The Loop

		public static void ManuallyRunLoopOnce(int fixedUpdateIterations)
		{
			if (EnableCatchingExceptionsInUpdateCallbacks)
			{
				Instance.TimeCallbacks.InvokeSafe();
				Instance.NetworkingCallbacks.InvokeSafe();
				Instance.InputUpdateCallbacks.InvokeSafe();

				for (int i = 0; i < fixedUpdateIterations; i++)
				{
					Instance.PreFixedUpdateCallbacks.InvokeSafe();
					Instance.FixedUpdateCallbacks.InvokeSafe();
					Instance.PostFixedUpdateCallbacks.InvokeSafe();
				}

				Instance.PreUpdateCallbacks.InvokeSafe();
				Instance.UpdateCallbacks.InvokeSafe();
				Instance.PostUpdateCallbacks.InvokeSafe();

				// In ManuallyRunLoopOnce we fire all infrequent-update callbacks unconditionally
				// (without checking frame count or elapsed time) so a single manual loop step
				// is enough to exercise them in tests.
				Instance.UpdateEvery10FramesCallbacks.InvokeSafe();
				Instance.UpdateEvery100MillisecondsCallbacks.InvokeSafe();
				Instance.UpdateEvery250MillisecondsCallbacks.InvokeSafe();
				Instance.UpdateEvery500MillisecondsCallbacks.InvokeSafe();
				Instance.UpdateEvery1000MillisecondsCallbacks.InvokeSafe();
				Instance.UpdateEvery100MillisecondsUnscaledCallbacks.InvokeSafe();
				Instance.UpdateEvery250MillisecondsUnscaledCallbacks.InvokeSafe();
				Instance.UpdateEvery500MillisecondsUnscaledCallbacks.InvokeSafe();
				Instance.UpdateEvery1000MillisecondsUnscaledCallbacks.InvokeSafe();

				Instance.PreLateUpdateCallbacks.InvokeSafe();
				Instance.LateUpdateCallbacks.InvokeSafe();
				Instance.PostLateUpdateCallbacks.InvokeSafe();

				Instance.CameraPlacementUpdateCallbacks.InvokeSafe();

				Instance.PreRenderCallbacks.InvokeSafe();
				Instance.PreUICallbacks.InvokeSafe();
			}
			else
			{
				Instance.TimeCallbacks.InvokeUnsafe();
				Instance.NetworkingCallbacks.InvokeUnsafe();
				Instance.InputUpdateCallbacks.InvokeUnsafe();

				for (int i = 0; i < fixedUpdateIterations; i++)
				{
					Instance.PreFixedUpdateCallbacks.InvokeUnsafe();
					Instance.FixedUpdateCallbacks.InvokeUnsafe();
					Instance.PostFixedUpdateCallbacks.InvokeUnsafe();
				}

				Instance.PreUpdateCallbacks.InvokeUnsafe();
				Instance.UpdateCallbacks.InvokeUnsafe();
				Instance.PostUpdateCallbacks.InvokeUnsafe();

				// In ManuallyRunLoopOnce we fire all infrequent-update callbacks unconditionally
				// (without checking frame count or elapsed time) so a single manual loop step
				// is enough to exercise them in tests.
				Instance.UpdateEvery10FramesCallbacks.InvokeUnsafe();
				Instance.UpdateEvery100MillisecondsCallbacks.InvokeUnsafe();
				Instance.UpdateEvery250MillisecondsCallbacks.InvokeUnsafe();
				Instance.UpdateEvery500MillisecondsCallbacks.InvokeUnsafe();
				Instance.UpdateEvery1000MillisecondsCallbacks.InvokeUnsafe();
				Instance.UpdateEvery100MillisecondsUnscaledCallbacks.InvokeUnsafe();
				Instance.UpdateEvery250MillisecondsUnscaledCallbacks.InvokeUnsafe();
				Instance.UpdateEvery500MillisecondsUnscaledCallbacks.InvokeUnsafe();
				Instance.UpdateEvery1000MillisecondsUnscaledCallbacks.InvokeUnsafe();

				Instance.PreLateUpdateCallbacks.InvokeUnsafe();
				Instance.LateUpdateCallbacks.InvokeUnsafe();
				Instance.PostLateUpdateCallbacks.InvokeUnsafe();

				Instance.CameraPlacementUpdateCallbacks.InvokeUnsafe();

				Instance.PreRenderCallbacks.InvokeUnsafe();
				Instance.PreUICallbacks.InvokeUnsafe();
			}
		}

		#endregion

		#region Timings

#if DisableExtenityTimeCaching

		public static float Time         { [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)] get => UnityEngine.Time.time;         }
		public static float DeltaTime    { [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)] get => UnityEngine.Time.deltaTime;    }
		public static float UnscaledTime { [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)] get => UnityEngine.Time.unscaledTime; }
		public static int   FrameCount   { [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)] get => UnityEngine.Time.frameCount;   }

#elif !UNITY_EDITOR && !DEBUG

		public static float Time;
		public static float DeltaTime;
		public static float UnscaledTime;
		public static int FrameCount;

#else

		private static float _Time;
		public static float Time
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying) // Use Unity times when working in editor.
					return UnityEngine.Time.time;
#endif
				CheckCachedFloatValue(UnityEngine.Time.time, _Time, "time");
				return _Time;
			}
			private set => _Time = value;
		}

		private static float _DeltaTime;
		public static float DeltaTime
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying) // Use Unity times when working in editor.
					return UnityEngine.Time.deltaTime;
#endif
				CheckCachedFloatValue(UnityEngine.Time.deltaTime, _DeltaTime, "deltaTime");
				return _DeltaTime;
			}
			private set => _DeltaTime = value;
		}

		private static float _UnscaledTime;
		public static float UnscaledTime
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying) // Use Unity times when working in editor.
					return UnityEngine.Time.unscaledTime;
#endif
				CheckCachedFloatValue(UnityEngine.Time.unscaledTime, _UnscaledTime, "unscaledTime");
				return _UnscaledTime;
			}
			private set => _UnscaledTime = value;
		}

		private static int _FrameCount;
		public static int FrameCount
		{
			get
			{
#if UNITY_EDITOR
				if (!Application.isPlaying) // Use Unity times when working in editor.
					return UnityEngine.Time.frameCount;
#endif
				CheckCachedIntValue(UnityEngine.Time.frameCount, _FrameCount, "frameCount");
				return _FrameCount;
			}
			private set => _FrameCount = value;
		}

		/// <summary>
		/// Makes sure cached value is exactly the same with Unity's value.
		/// Value is cached at the start of Update calls. This method ensures each time
		/// the code gets that cached value, if asked Unity instead, Unity too would tell
		/// the same value that is exactly equal to the cached value. If not, that means
		/// a serious internal error.
		///
		/// If that error happens, error contains which parameter is problematic
		/// (time, deltaTime, etc.). Also look into the callstack to see which Update method
		/// it is (FixedUpdate, LateUpdate, etc.).
		/// </summary>
		private static void CheckCachedFloatValue(float originalValue, float cachedValue, string memberName)
		{
			const float TimeEqualityCheckTolerance = 1.0f / 1000.0f / 1000.0f; // 1 microsecond, if the value is in seconds.

			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (!originalValue.IsAlmostEqual(cachedValue, TimeEqualityCheckTolerance))
			{
				Log.Fatal($"Loop System detected that the cached 'Time.{memberName}' became obsolete. This system allows optimization by caching Unity's Time API results. It can be disabled via 'DisableExtenityTimeCaching' compiler directive.\nUnity reported value: {originalValue}\nLoop cached value: {cachedValue}\nDifference: {(originalValue - cachedValue)}");
			}
		}

		/// <summary>
		/// Makes sure cached value is exactly the same with Unity's value.
		/// Value is cached at the start of Update calls. This method ensures each time
		/// the code gets that cached value, if asked Unity instead, Unity too would tell
		/// the same value that is exactly equal to the cached value. If not, that means
		/// a serious internal error.
		///
		/// If that error happens, error contains which parameter is problematic
		/// (time, deltaTime, etc.). Also look into the callstack to see which Update method
		/// it is (FixedUpdate, LateUpdate, etc.).
		/// </summary>
		private static void CheckCachedIntValue(int originalValue, int cachedValue, string memberName)
		{
			if (originalValue != cachedValue)
			{
				Log.Fatal($"Loop System detected that the cached 'Time.{memberName}' became obsolete. This system allows optimization by caching Unity's Time API results. It can be disabled via 'DisableExtenityTimeCaching' compiler directive.\nUnity reported value: {originalValue}\nLoop cached value: {cachedValue}\nDifference: {(originalValue - cachedValue)}");
			}
		}

#endif

		internal static void SetCachedTimesFromUnityTimes()
		{
#if !DisableExtenityTimeCaching
			Time = UnityEngine.Time.time;
			DeltaTime = UnityEngine.Time.deltaTime;
			UnscaledTime = UnityEngine.Time.unscaledTime;
			FrameCount = UnityEngine.Time.frameCount;
#endif
		}

		private static void ResetCachedTimes()
		{
#if !DisableExtenityTimeCaching
			Time = float.NaN;
			DeltaTime = float.NaN;
			UnscaledTime = float.NaN;
			FrameCount = int.MinValue;
#endif
		}

		public static double RefreshCacheAndGetTime(bool isUnscaledTime)
		{
#if !DisableExtenityTimeCaching
			if (isUnscaledTime)
			{
				var value = UnityEngine.Time.unscaledTime;
				UnscaledTime = value;
				return value;
			}
			else
			{
				var value = UnityEngine.Time.time;
				Time = value;
				return value;
			}
#else
			return GetTime(isUnscaledTime);
#endif
		}

		public static double GetTime(bool isUnscaledTime)
		{
			return isUnscaledTime ? UnscaledTime : Time;
		}

		#endregion

		#region Safe or Unsafe Mode

		public static bool EnableCatchingExceptionsInUpdateCallbacks = true;

		#endregion

		#region Callback Cleanup Verification

		public static void EnsureAllCallbacksDeregistered()
		{
			if (Instance == null)
				return;

			var totalCallbacks = 0;
			List<string> callbackDetails = null;

			void CheckCallbacks(ExtenityEvent extenityEvent, string callbackName)
			{
				if (extenityEvent.IsAnyListenerRegistered)
				{
					if (callbackDetails == null)
					{
						callbackDetails = new List<string>(); // It's okay to do allocation if an error happens.
					}
					var count = extenityEvent.ListenersCount;
					totalCallbacks += count;
					callbackDetails.Add($"{callbackName}: {count}");
				}
			}

			CheckCallbacks(Instance.TimeCallbacks, "Time");
			CheckCallbacks(Instance.NetworkingCallbacks, "Networking");
			CheckCallbacks(Instance.InputUpdateCallbacks, "InputUpdate");

			CheckCallbacks(Instance.PreFixedUpdateCallbacks, "PreFixedUpdate");
			CheckCallbacks(Instance.PreUpdateCallbacks, "PreUpdate");
			CheckCallbacks(Instance.PreLateUpdateCallbacks, "PreLateUpdate");

			CheckCallbacks(Instance.FixedUpdateCallbacks, "FixedUpdate");
			CheckCallbacks(Instance.UpdateCallbacks, "Update");
			CheckCallbacks(Instance.LateUpdateCallbacks, "LateUpdate");

			CheckCallbacks(Instance.PostFixedUpdateCallbacks, "PostFixedUpdate");
			CheckCallbacks(Instance.PostUpdateCallbacks, "PostUpdate");
			CheckCallbacks(Instance.PostLateUpdateCallbacks, "PostLateUpdate");

			CheckCallbacks(Instance.UpdateEvery10FramesCallbacks, "UpdateEvery10Frames");
			CheckCallbacks(Instance.UpdateEvery100MillisecondsCallbacks,          "UpdateEvery100Milliseconds");
			CheckCallbacks(Instance.UpdateEvery100MillisecondsUnscaledCallbacks,  "UpdateEvery100MillisecondsUnscaled");
			CheckCallbacks(Instance.UpdateEvery250MillisecondsCallbacks,          "UpdateEvery250Milliseconds");
			CheckCallbacks(Instance.UpdateEvery250MillisecondsUnscaledCallbacks,  "UpdateEvery250MillisecondsUnscaled");
			CheckCallbacks(Instance.UpdateEvery500MillisecondsCallbacks,          "UpdateEvery500Milliseconds");
			CheckCallbacks(Instance.UpdateEvery500MillisecondsUnscaledCallbacks,  "UpdateEvery500MillisecondsUnscaled");
			CheckCallbacks(Instance.UpdateEvery1000MillisecondsCallbacks,         "UpdateEvery1000Milliseconds");
			CheckCallbacks(Instance.UpdateEvery1000MillisecondsUnscaledCallbacks, "UpdateEvery1000MillisecondsUnscaled");

			CheckCallbacks(Instance.CameraPlacementUpdateCallbacks, "CameraPlacementUpdate");

			CheckCallbacks(Instance.PreRenderCallbacks, "PreRender");
			CheckCallbacks(Instance.PreUICallbacks, "PreUI");

			if (totalCallbacks > 0)
			{
				throw new Exception($"Loop has {totalCallbacks} callback(s) still registered:\n" + string.Join("\n", callbackDetails));
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(Loop));

		#endregion
	}

}

#endif
