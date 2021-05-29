using System;
using Extenity.ApplicationToolbox;
using Extenity.DebugToolbox.GraphPlotting;
using Extenity.DesignPatternsToolbox;
using Extenity.FlowToolbox;
using Extenity.GameObjectToolbox;
using Extenity.MessagingToolbox;
using Extenity.ProfilingToolbox;
using Sirenix.OdinInspector;

namespace Extenity
{

	public class Loop : SingletonUnity<Loop>
	{
		#region Initialization

		private void Awake()
		{
			InitializeSingleton(true);
			Invoker.ResetSystem();

			// Automatically add Execution Order Helpers if required.
			this.GetFirstOrAddComponent<LoopPreExecutionOrderHelper>();
			this.GetFirstOrAddComponent<LoopDefaultExecutionOrderHelper>();
			this.GetFirstOrAddComponent<LoopPostExecutionOrderHelper>();
		}

		#endregion

		#region Deinitialization

		private void OnApplicationQuit()
		{
			ApplicationTools.IsShuttingDown = true;
		}

		#endregion

		#region Update

		private void FixedUpdate()
		{
			FixedUpdateCount++;
			Time = UnityEngine.Time.time;
			DeltaTime = UnityEngine.Time.deltaTime;
			UnscaledTime = UnityEngine.Time.unscaledTime;

			// FastInvokes are called before any other callbacks. Note that Loop.FixedUpdate is executed before
			// LoopPreExecutionOrderHelper.FixedUpdate as defined in Script Execution Order Project Settings.
			FastInvokeHandler.Instance.CustomFixedUpdate();

			// Instance.FixedUpdateCallbacks.ClearIfRequired();
		}

		private void Update()
		{
			UpdateCount++;
			Time = UnityEngine.Time.time;
			DeltaTime = UnityEngine.Time.deltaTime;
			UnscaledTime = UnityEngine.Time.unscaledTime;

			if (FPSAnalyzer != null)
			{
				FPSAnalyzer.Tick(Time);
			}

			// FastInvokes are called before any other callbacks. Note that Loop.Update is executed before
			// LoopPreExecutionOrderHelper.Update as defined in Script Execution Order Project Settings.
			FastInvokeHandler.Instance.CustomUpdate();

			// Instance.UpdateCallbacks.ClearIfRequired();
		}

		private void LateUpdate()
		{
			LateUpdateCount++;
			Time = UnityEngine.Time.time;
			DeltaTime = UnityEngine.Time.deltaTime;
			UnscaledTime = UnityEngine.Time.unscaledTime;

			// Instance.LateUpdateCallbacks.ClearIfRequired();
		}

		#endregion

		#region Callbacks

		internal readonly ExtenityEvent PreFixedUpdateCallbacks = new ExtenityEvent();
		internal readonly ExtenityEvent PreUpdateCallbacks = new ExtenityEvent();
		internal readonly ExtenityEvent PreLateUpdateCallbacks = new ExtenityEvent();

		internal readonly ExtenityEvent FixedUpdateCallbacks = new ExtenityEvent();
		internal readonly ExtenityEvent UpdateCallbacks = new ExtenityEvent();
		internal readonly ExtenityEvent LateUpdateCallbacks = new ExtenityEvent();

		internal readonly ExtenityEvent PostFixedUpdateCallbacks = new ExtenityEvent();
		internal readonly ExtenityEvent PostUpdateCallbacks = new ExtenityEvent();
		internal readonly ExtenityEvent PostLateUpdateCallbacks = new ExtenityEvent();

		public static void RegisterPreFixedUpdate(Action callback, int order = 0) { Instance.PreFixedUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterPreUpdate(Action callback, int order = 0) { Instance.PreUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterPreLateUpdate(Action callback, int order = 0) { Instance.PreLateUpdateCallbacks.AddListener(callback, order); }
		public static void DeregisterPreFixedUpdate(Action callback) { if (Instance) Instance.PreFixedUpdateCallbacks.RemoveListener(callback); }
		public static void DeregisterPreUpdate(Action callback) { if (Instance) Instance.PreUpdateCallbacks.RemoveListener(callback); }
		public static void DeregisterPreLateUpdate(Action callback) { if (Instance) Instance.PreLateUpdateCallbacks.RemoveListener(callback); }

		public static void RegisterFixedUpdate(Action callback, int order = 0) { Instance.FixedUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterUpdate(Action callback, int order = 0) { Instance.UpdateCallbacks.AddListener(callback, order); }
		public static void RegisterLateUpdate(Action callback, int order = 0) { Instance.LateUpdateCallbacks.AddListener(callback, order); }
		public static void DeregisterFixedUpdate(Action callback) { if (Instance) Instance.FixedUpdateCallbacks.RemoveListener(callback); }
		public static void DeregisterUpdate(Action callback) { if (Instance) Instance.UpdateCallbacks.RemoveListener(callback); }
		public static void DeregisterLateUpdate(Action callback) { if (Instance) Instance.LateUpdateCallbacks.RemoveListener(callback); }

		public static void RegisterPostFixedUpdate(Action callback, int order = 0) { Instance.PostFixedUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterPostUpdate(Action callback, int order = 0) { Instance.PostUpdateCallbacks.AddListener(callback, order); }
		public static void RegisterPostLateUpdate(Action callback, int order = 0) { Instance.PostLateUpdateCallbacks.AddListener(callback, order); }
		public static void DeregisterPostFixedUpdate(Action callback) { if (Instance) Instance.PostFixedUpdateCallbacks.RemoveListener(callback); }
		public static void DeregisterPostUpdate(Action callback) { if (Instance) Instance.PostUpdateCallbacks.RemoveListener(callback); }
		public static void DeregisterPostLateUpdate(Action callback) { if (Instance) Instance.PostLateUpdateCallbacks.RemoveListener(callback); }

		#endregion

		#region Counters

		[Title("Stats")]
		[NonSerialized, ShowInInspector]
		public int UpdateCount;
		[NonSerialized, ShowInInspector]
		public int FixedUpdateCount;
		[NonSerialized, ShowInInspector]
		public int LateUpdateCount;

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

		#region FPS Analyzer

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

			_IsFPSAnalyzerEnabled = enabled;
			if (enabled)
			{
				FPSAnalyzer = new TickAnalyzer(
					new TickPlotter("FPS", VerticalRangeConfiguration.ZeroBasedAdaptive(), gameObject),
					Time,
					TickAnalyzer.HistorySizeFor(60, 5));
			}
			else
			{
				// Deinitialize existing one.
				FPSAnalyzer = null;
			}
		}

		#endregion
	}

}
