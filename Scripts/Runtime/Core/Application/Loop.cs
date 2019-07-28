using System;
using Extenity.ApplicationToolbox;
using Extenity.DebugToolbox.GraphPlotting;
using Extenity.DesignPatternsToolbox;
using Extenity.FlowToolbox;
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

			// FastInvokes are called before any other callbacks.
			FastInvokeHandler.Instance.CustomFixedUpdate();

			FixedUpdateCallbacks.InvokeSafe();
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

			// FastInvokes are called before any other callbacks.
			FastInvokeHandler.Instance.CustomUpdate();

			UpdateCallbacks.InvokeSafe();
		}

		private void LateUpdate()
		{
			LateUpdateCount++;
			Time = UnityEngine.Time.time;
			DeltaTime = UnityEngine.Time.deltaTime;
			UnscaledTime = UnityEngine.Time.unscaledTime;
			LateUpdateCallbacks.InvokeSafe();
		}

		#endregion

		#region Callbacks

		public static readonly ExtenityEvent FixedUpdateCallbacks = new ExtenityEvent();
		public static readonly ExtenityEvent UpdateCallbacks = new ExtenityEvent();
		public static readonly ExtenityEvent LateUpdateCallbacks = new ExtenityEvent();

		#endregion

		#region Counters

		public static int UpdateCount;
		public static int FixedUpdateCount;
		public static int LateUpdateCount;

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

		[NonSerialized]
		public TickAnalyzer FPSAnalyzer;
		private bool _IsFPSAnalyzerEnabled;

		[Button, ButtonGroup("ToggleFPSAnalyzer"), DisableIf(nameof(_IsFPSAnalyzerEnabled))]
		public void EnableFPSAnalyzer()
		{
			EnableFPSAnalyzer(true);
		}

		[Button, ButtonGroup("ToggleFPSAnalyzer"), EnableIf(nameof(_IsFPSAnalyzerEnabled))]
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
					new TickPlotter("FPS", ValueAxisRangeConfiguration.CreateZeroBasedAdaptive(), gameObject),
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
