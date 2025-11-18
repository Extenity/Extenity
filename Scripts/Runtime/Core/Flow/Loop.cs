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

		public static int FrameCount;
		public static int FixedUpdateCount;

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
		public static void DeregisterPreFixedUpdate       (Action callback) { Instance.PreFixedUpdateCallbacks       .RemoveListener(callback); }
		public static void DeregisterPreUpdate            (Action callback) { Instance.PreUpdateCallbacks            .RemoveListener(callback); }
		public static void DeregisterPreLateUpdate        (Action callback) { Instance.PreLateUpdateCallbacks        .RemoveListener(callback); }
		public static void DeregisterFixedUpdate          (Action callback) { Instance.FixedUpdateCallbacks          .RemoveListener(callback); }
		public static void DeregisterUpdate               (Action callback) { Instance.UpdateCallbacks               .RemoveListener(callback); }
		public static void DeregisterLateUpdate           (Action callback) { Instance.LateUpdateCallbacks           .RemoveListener(callback); }
		public static void DeregisterPostFixedUpdate      (Action callback) { Instance.PostFixedUpdateCallbacks      .RemoveListener(callback); }
		public static void DeregisterPostUpdate           (Action callback) { Instance.PostUpdateCallbacks           .RemoveListener(callback); }
		public static void DeregisterPostLateUpdate       (Action callback) { Instance.PostLateUpdateCallbacks       .RemoveListener(callback); }
		public static void DeregisterPreRender            (Action callback) { Instance.PreRenderCallbacks            .RemoveListener(callback); }
		public static void DeregisterPreUI                (Action callback) { Instance.PreUICallbacks                .RemoveListener(callback); }

		// @formatter:on

		#endregion

		#region PlayerLoop Injection

		private static void InjectIntoPlayerLoop(ref PlayerLoopSystem playerLoop)
		{
			InsertLoopSystemAfter<TimeUpdate>(ref playerLoop, typeof(TimeUpdate.WaitForLastPresentationAndUpdateTime), CreateLoopSystem<TimeRunner>());
			InsertLoopSystemAfter<TimeUpdate>(ref playerLoop, typeof(TimeRunner), CreateLoopSystem<NetworkingRunner>());

			InsertLoopSystemBefore<FixedUpdate>(ref playerLoop, typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate), CreateLoopSystem<PreFixedUpdateRunner>());
			InsertLoopSystemAfter<FixedUpdate>(ref playerLoop, typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate), CreateLoopSystem<FixedUpdateRunner>());
			InsertLoopSystemAfter<FixedUpdate>(ref playerLoop, typeof(FixedUpdateRunner), CreateLoopSystem<PostFixedUpdateRunner>());

			InsertLoopSystemBefore<Update>(ref playerLoop, typeof(Update.ScriptRunBehaviourUpdate), CreateLoopSystem<PreUpdateRunner>());
			InsertLoopSystemAfter<Update>(ref playerLoop, typeof(Update.ScriptRunBehaviourUpdate), CreateLoopSystem<UpdateRunner>());
			InsertLoopSystemAfter<Update>(ref playerLoop, typeof(UpdateRunner), CreateLoopSystem<PostUpdateRunner>());

			InsertLoopSystemBefore<PreLateUpdate>(ref playerLoop, typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), CreateLoopSystem<PreLateUpdateRunner>());
			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate), CreateLoopSystem<LateUpdateRunner>());
			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(LateUpdateRunner), CreateLoopSystem<PostLateUpdateRunner>());

			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(PostLateUpdateRunner), CreateLoopSystem<PreRenderRunner>());
			InsertLoopSystemAfter<PreLateUpdate>(ref playerLoop, typeof(PreRenderRunner), CreateLoopSystem<PreUIRunner>());
		}

		private static void RemoveFromPlayerLoop(ref PlayerLoopSystem playerLoop)
		{
			RemoveLoopSystem<TimeUpdate>(ref playerLoop, typeof(TimeRunner));
			RemoveLoopSystem<TimeUpdate>(ref playerLoop, typeof(NetworkingRunner));

			RemoveLoopSystem<FixedUpdate>(ref playerLoop, typeof(PreFixedUpdateRunner));
			RemoveLoopSystem<FixedUpdate>(ref playerLoop, typeof(FixedUpdateRunner));
			RemoveLoopSystem<FixedUpdate>(ref playerLoop, typeof(PostFixedUpdateRunner));

			RemoveLoopSystem<Update>(ref playerLoop, typeof(PreUpdateRunner));
			RemoveLoopSystem<Update>(ref playerLoop, typeof(UpdateRunner));
			RemoveLoopSystem<Update>(ref playerLoop, typeof(PostUpdateRunner));

			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(PreLateUpdateRunner));
			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(LateUpdateRunner));
			RemoveLoopSystem<PreLateUpdate>(ref playerLoop, typeof(PostLateUpdateRunner));
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
			if (typeof(T) == typeof(TimeRunner)) return () => { SetCachedTimesFromUnityTimes(); FrameCount++; InvokeSafeIfEnabled(Instance.TimeCallbacks); };
			if (typeof(T) == typeof(NetworkingRunner)) return () => { InvokeSafeIfEnabled(Instance.NetworkingCallbacks); };

			if (typeof(T) == typeof(PreFixedUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PreFixedUpdateCallbacks); };
			if (typeof(T) == typeof(FixedUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); FixedUpdateCount++; Invoker.Handler.CustomFixedUpdate(Time); InvokeSafeIfEnabled(Instance.FixedUpdateCallbacks); };
			if (typeof(T) == typeof(PostFixedUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PostFixedUpdateCallbacks); };

			if (typeof(T) == typeof(PreUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PreUpdateCallbacks); };
			if (typeof(T) == typeof(UpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); Invoker.Handler.CustomUpdate(UnscaledTime); InvokeSafeIfEnabled(Instance.UpdateCallbacks); };
			if (typeof(T) == typeof(PostUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PostUpdateCallbacks); };

			if (typeof(T) == typeof(PreLateUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PreLateUpdateCallbacks); };
			if (typeof(T) == typeof(LateUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.LateUpdateCallbacks); };
			if (typeof(T) == typeof(PostLateUpdateRunner)) return () => { SetCachedTimesFromUnityTimes(); InvokeSafeIfEnabled(Instance.PostLateUpdateCallbacks); };

			if (typeof(T) == typeof(PreRenderRunner)) return () => { InvokeSafeIfEnabled(Instance.PreRenderCallbacks); };
			if (typeof(T) == typeof(PreUIRunner)) return () => { InvokeSafeIfEnabled(Instance.PreUICallbacks); };

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

		private struct PreFixedUpdateRunner { }
		private struct FixedUpdateRunner { }
		private struct PostFixedUpdateRunner { }

		private struct PreUpdateRunner { }
		private struct UpdateRunner { }
		private struct PostUpdateRunner { }

		private struct PreLateUpdateRunner { }
		private struct LateUpdateRunner { }
		private struct PostLateUpdateRunner { }

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

				for (int i = 0; i < fixedUpdateIterations; i++)
				{
					Instance.PreFixedUpdateCallbacks.InvokeSafe();
					Instance.FixedUpdateCallbacks.InvokeSafe();
					Instance.PostFixedUpdateCallbacks.InvokeSafe();
				}

				Instance.PreUpdateCallbacks.InvokeSafe();
				Instance.UpdateCallbacks.InvokeSafe();
				Instance.PostUpdateCallbacks.InvokeSafe();

				Instance.PreLateUpdateCallbacks.InvokeSafe();
				Instance.LateUpdateCallbacks.InvokeSafe();
				Instance.PostLateUpdateCallbacks.InvokeSafe();

				Instance.PreRenderCallbacks.InvokeSafe();
				Instance.PreUICallbacks.InvokeSafe();
			}
			else
			{
				Instance.TimeCallbacks.InvokeUnsafe();
				Instance.NetworkingCallbacks.InvokeUnsafe();

				for (int i = 0; i < fixedUpdateIterations; i++)
				{
					Instance.PreFixedUpdateCallbacks.InvokeUnsafe();
					Instance.FixedUpdateCallbacks.InvokeUnsafe();
					Instance.PostFixedUpdateCallbacks.InvokeUnsafe();
				}

				Instance.PreUpdateCallbacks.InvokeUnsafe();
				Instance.UpdateCallbacks.InvokeUnsafe();
				Instance.PostUpdateCallbacks.InvokeUnsafe();

				Instance.PreLateUpdateCallbacks.InvokeUnsafe();
				Instance.LateUpdateCallbacks.InvokeUnsafe();
				Instance.PostLateUpdateCallbacks.InvokeUnsafe();

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

#elif !UNITY_EDITOR && !DEBUG

		public static float Time;
		public static float DeltaTime;
		public static float UnscaledTime;

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

#endif

		internal static void SetCachedTimesFromUnityTimes()
		{
#if !DisableExtenityTimeCaching
			Time = UnityEngine.Time.time;
			DeltaTime = UnityEngine.Time.deltaTime;
			UnscaledTime = UnityEngine.Time.unscaledTime;
#endif
		}

		private static void ResetCachedTimes()
		{
#if !DisableExtenityTimeCaching
			Time = float.NaN;
			DeltaTime = float.NaN;
			UnscaledTime = float.NaN;
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

			CheckCallbacks(Instance.PreFixedUpdateCallbacks, "PreFixedUpdate");
			CheckCallbacks(Instance.PreUpdateCallbacks, "PreUpdate");
			CheckCallbacks(Instance.PreLateUpdateCallbacks, "PreLateUpdate");

			CheckCallbacks(Instance.FixedUpdateCallbacks, "FixedUpdate");
			CheckCallbacks(Instance.UpdateCallbacks, "Update");
			CheckCallbacks(Instance.LateUpdateCallbacks, "LateUpdate");

			CheckCallbacks(Instance.PostFixedUpdateCallbacks, "PostFixedUpdate");
			CheckCallbacks(Instance.PostUpdateCallbacks, "PostUpdate");
			CheckCallbacks(Instance.PostLateUpdateCallbacks, "PostLateUpdate");

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
