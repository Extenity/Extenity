using System;
using UnityEngine;
#if UNITY_2019_3_OR_NEWER
using UnityEngine.Rendering;
#else
using UnityEngine.Experimental.Rendering;
#endif

namespace Extenity.ProfilingToolbox
{

	/// <summary>
	/// Requires Scriptable Rendering Pipeline (SRP) to get frame events. See: https://forum.unity.com/threads/onprerender-onpostrender-counterpart-in-srp.535875/#post-3530582
	/// </summary>
	public sealed class FPSCounter : IDisposable
	{
		#region Configuration

		/// <summary>
		/// Note that this will both affect FPS display and FPS stabilization detection method.
		/// Increasing this will smooth out FPS display, but FPS stabilization will require more
		/// frames to calculate.
		/// </summary>
		private const int MeanEntryCount = 6;

		#endregion

		#region Initialization

		public FPSCounter(int poorFPSBarrier = 25, int fineFPSBarrier = 45)
		{
			Debug.Assert(fineFPSBarrier > poorFPSBarrier);
			PoorFPSBarrier = poorFPSBarrier;
			FineFPSBarrier = fineFPSBarrier;
			FPSQualityInvDiff = 1f / (FineFPSBarrier - PoorFPSBarrier);

			TickAnalyzer = new TickAnalyzer(0f, MeanEntryCount);
		}

		#endregion

		#region Deinitialization

		public void Dispose()
		{
			EndCapturing();

			TickAnalyzer.ClearAllEvents();
		}

		#endregion

		#region Capture

		public bool IsCapturing { get; private set; }

		public void StartCapturing()
		{
			if (IsCapturing)
				return;
			IsCapturing = true;

			TickAnalyzer.Reset(Time.realtimeSinceStartup, MeanEntryCount);

#if UNITY_2019_3_OR_NEWER
			RenderPipelineManager.beginFrameRendering += OnBeginFrameRendering;
#else
			RenderPipeline.beginFrameRendering += OnBeginFrameRendering;
#endif
		}

		public void EndCapturing()
		{
			if (!IsCapturing)
				return;
			IsCapturing = false;

#if UNITY_2019_3_OR_NEWER
			RenderPipelineManager.beginFrameRendering -= OnBeginFrameRendering;
#else
			RenderPipeline.beginFrameRendering -= OnBeginFrameRendering;
#endif

			TickAnalyzer.Reset(0f, MeanEntryCount);
		}

#if UNITY_2019_3_OR_NEWER
		private void OnBeginFrameRendering(ScriptableRenderContext context, Camera[] cameras)
#else
		private void OnBeginFrameRendering(Camera[] cameras)
#endif
		{
			TickAnalyzer.Tick(Time.realtimeSinceStartup);
		}

		#endregion

		#region FPS

		public readonly TickAnalyzer TickAnalyzer;

		// Use TickAnalyzer events instead.
		//public readonly ExtenityEvent<float> OnFPSUpdate = new ExtenityEvent<float>();

		public float FPS => (float)TickAnalyzer.MeanElapsedTime;

		#endregion

		#region FPS Quality

		public readonly int PoorFPSBarrier;
		public readonly int FineFPSBarrier;
		public readonly float FPSQualityInvDiff;

		public float QualityRatio
		{
			get { return Mathf.Clamp01((FPS - PoorFPSBarrier) * FPSQualityInvDiff); }
		}

		#endregion
	}

}
