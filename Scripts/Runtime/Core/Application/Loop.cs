#if UNITY

using System;
using Extenity.FlowToolbox;
using UnityEngine;

namespace Extenity
{

	public static class Loop
	{
		#region Singleton

		internal static LoopHelper Instance;

		#endregion

		#region Initialization

		// Instantiating game objects in SubsystemRegistration and AfterAssembliesLoaded is a bad idea.
		// It works in Editor but observed not working in Windows and Android builds and probably other
		// platforms too. Game objects are destroyed just before BeforeSceneLoad for some reason.
		// So decided to initialize our subsystems at BeforeSceneLoad stage. See 119392241.
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Instantiate()
		{
			Debug.Assert(Instance == null);
			InitializeSystem();
		}

		public static void InitializeSystem()
		{
			DeinitializeSystem();

			Invoker.InitializeSystem();

			var go = new GameObject("_Loop");
			GameObject.DontDestroyOnLoad(go);
			Instance = go.AddComponent<LoopHelper>();
			go.AddComponent<LoopPreExecutionOrderHelper>().LoopHelper = Instance;
			go.AddComponent<LoopDefaultExecutionOrderHelper>().LoopHelper = Instance;
			go.AddComponent<LoopPostExecutionOrderHelper>().LoopHelper = Instance;
		}

		public static void DeinitializeSystem()
		{
			if (Instance)
			{
				GameObject.DestroyImmediate(Instance.gameObject);
				Instance = null;
			}
		}

		#endregion

		#region Callbacks

		// @formatter:off
		public static void RegisterPreFixedUpdate  (Action callback, int order = 0) { Instance.PreFixedUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterPreUpdate       (Action callback, int order = 0) { Instance.PreUpdateCallbacks.AddListener(callback, order);      }
		public static void RegisterPreLateUpdate   (Action callback, int order = 0) { Instance.PreLateUpdateCallbacks.AddListener(callback, order);  }
		public static void DeregisterPreFixedUpdate(Action callback) { if (Instance) Instance.PreFixedUpdateCallbacks.RemoveListener(callback);      }
		public static void DeregisterPreUpdate     (Action callback) { if (Instance) Instance.PreUpdateCallbacks.RemoveListener(callback);           }
		public static void DeregisterPreLateUpdate (Action callback) { if (Instance) Instance.PreLateUpdateCallbacks.RemoveListener(callback);       }

		public static void RegisterFixedUpdate  (Action callback, int order = 0) { Instance.FixedUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterUpdate       (Action callback, int order = 0) { Instance.UpdateCallbacks.AddListener(callback, order);      }
		public static void RegisterLateUpdate   (Action callback, int order = 0) { Instance.LateUpdateCallbacks.AddListener(callback, order);  }
		public static void DeregisterFixedUpdate(Action callback) { if (Instance) Instance.FixedUpdateCallbacks.RemoveListener(callback);      }
		public static void DeregisterUpdate     (Action callback) { if (Instance) Instance.UpdateCallbacks.RemoveListener(callback);           }
		public static void DeregisterLateUpdate (Action callback) { if (Instance) Instance.LateUpdateCallbacks.RemoveListener(callback);       }

		public static void RegisterPostFixedUpdate  (Action callback, int order = 0) { Instance.PostFixedUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterPostUpdate       (Action callback, int order = 0) { Instance.PostUpdateCallbacks.AddListener(callback, order);      }
		public static void RegisterPostLateUpdate   (Action callback, int order = 0) { Instance.PostLateUpdateCallbacks.AddListener(callback, order);  }
		public static void DeregisterPostFixedUpdate(Action callback) { if (Instance) Instance.PostFixedUpdateCallbacks.RemoveListener(callback);      }
		public static void DeregisterPostUpdate     (Action callback) { if (Instance) Instance.PostUpdateCallbacks.RemoveListener(callback);           }
		public static void DeregisterPostLateUpdate (Action callback) { if (Instance) Instance.PostLateUpdateCallbacks.RemoveListener(callback);       }
		// @formatter:on

		#endregion

		#region Timings

#if !UNITY_EDITOR && !DEBUG
		public static float Time;
		public static float DeltaTime;
		public static float UnscaledTime;

#else

		private static float _Time;
		public static float Time
		{
			get
			{
				CheckExpectedTime(UnityEngine.Time.time, _Time, nameof(Time));
				return _Time;
			}
			set => _Time = value;
		}

		private static float _DeltaTime;
		public static float DeltaTime
		{
			get
			{
				CheckExpectedTime(UnityEngine.Time.deltaTime, _DeltaTime, nameof(DeltaTime));
				return _DeltaTime;
			}
			set => _DeltaTime = value;
		}

		private static float _UnscaledTime;
		public static float UnscaledTime
		{
			get
			{
				CheckExpectedTime(UnityEngine.Time.unscaledTime, _UnscaledTime, nameof(UnscaledTime));
				return _UnscaledTime;
			}
			set => _UnscaledTime = value;
		}

		/// <summary>
		/// Makes sure cached time value is exactly the same with Unity's time value.
		/// Time value is cached at the start of Update calls. This method ensures each time
		/// the code gets that cached value, if asked Unity instead, Unity too would tell
		/// the same value that is exactly equal to the cached value. If not, that means
		/// a serious internal error.
		///
		/// If that error happens, error contains which parameter is problematic
		/// (time, deltaTime, etc.). Also look into the callstack to see which Update method
		/// it is (FixedUpdate, LateUpdate, etc.).
		/// </summary>
		private static void CheckExpectedTime(float expectedTime, float actualTime, string parameterName)
		{
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			if (expectedTime != actualTime)
			{
				Log.CriticalError($"{nameof(Loop)} system timing is off for parameter '{parameterName}'. Expected is '{expectedTime}' while actual is '{actualTime}'.");
			}
		}

#endif

		#endregion
	}

}

#endif
