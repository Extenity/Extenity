#if UNITY

using System;
using Extenity.ApplicationToolbox;
using Extenity.DebugToolbox.GraphPlotting;
using Extenity.FlowToolbox;
using Extenity.MessagingToolbox;
using Extenity.ProfilingToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity
{

	public class LoopHelper : MonoBehaviour
	{
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
			Loop.Time = UnityEngine.Time.time;
			Loop.DeltaTime = UnityEngine.Time.deltaTime;
			Loop.UnscaledTime = UnityEngine.Time.unscaledTime;

			// FastInvokes are called before any other callbacks. Note that Loop.FixedUpdate is executed before
			// LoopPreExecutionOrderHelper.FixedUpdate as defined in Script Execution Order Project Settings.
			FastInvokeHandler.Instance.CustomFixedUpdate(Loop.Time);

			// Instance.FixedUpdateCallbacks.ClearIfRequired();
		}

		private void Update()
		{
			UpdateCount++;
			Loop.Time = UnityEngine.Time.time;
			Loop.DeltaTime = UnityEngine.Time.deltaTime;
			Loop.UnscaledTime = UnityEngine.Time.unscaledTime;

			if (FPSAnalyzer != null)
			{
				FPSAnalyzer.Tick(Loop.Time);
			}

			// FastInvokes are called before any other callbacks. Note that Loop.Update is executed before
			// LoopPreExecutionOrderHelper.Update as defined in Script Execution Order Project Settings.
			FastInvokeHandler.Instance.CustomUpdate(Loop.UnscaledTime);

			// Instance.UpdateCallbacks.ClearIfRequired();
		}

		private void LateUpdate()
		{
			LateUpdateCount++;
			Loop.Time = UnityEngine.Time.time;
			Loop.DeltaTime = UnityEngine.Time.deltaTime;
			Loop.UnscaledTime = UnityEngine.Time.unscaledTime;

			// Instance.LateUpdateCallbacks.ClearIfRequired();
		}

		#endregion

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

		[Title("Stats")]
		[NonSerialized, ShowInInspector]
		public int UpdateCount;
		[NonSerialized, ShowInInspector]
		public int FixedUpdateCount;
		[NonSerialized, ShowInInspector]
		public int LateUpdateCount;

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

		#endregion
	}

}

#endif
