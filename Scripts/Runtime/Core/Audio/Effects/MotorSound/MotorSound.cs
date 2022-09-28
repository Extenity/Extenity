#if ExtenityAudio

using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.Audio.Effects
{

	public class MotorSound : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float GasInput;
		private float PreviousGasInput;
		public ClampedFloat RPM;
		[Range(0f, 1f)]
		public float Turbo;

		[SerializeField, FormerlySerializedAs("ParameterEndValue")]
		private float _MaxRPM = 100f;
		public float MaxRPM
		{
			get { return _MaxRPM; }
			set
			{
				_MaxRPM = value;
				ScaleConfigurations();
			}
		}

		[Serializable]
		public class ClipConfiguration
		{
			public bool UseOnlyHighLoad = false;

			[FormerlySerializedAs("Source")]
			public AudioSource HighLoadSource;
			[FormerlySerializedAs("SourceLow")]
			public AudioSource LowLoadSource;

			public AnimationCurve VolumeCurve;
			public AnimationCurve FrequencyCurve;

			//[NonSerialized]
			//public bool IsHighLoadAvailable;
			//[NonSerialized]
			//public bool IsLowLoadAvailable;
		}


		[Header("RPM")]
		public float LoadSmoothness = 0.08f;
		public List<ClipConfiguration> ClipConfigurations;

		[Header("Gas Leaks")]
		public AudioSource PushGasSource;
		public AudioSource ReleaseGasSource;
		public float MinRateOfChangeToPush = 1f;
		public float MinRateOfChangeToRelease = 1f;
		public float MinIntervalBetweenPushEffects = 0.3f;
		public float MinIntervalBetweenReleaseEffects = 0.3f;

		[Header("Turbo")]
		public AudioSource TurboAudioSource;
		public AnimationCurve TurboVolumeCurve;
		public AnimationCurve TurboFrequencyCurve;

		[NonSerialized]
		public float Load;

		private float LastPushEffectTime;
		private float LastReleaseEffectTime;

		private bool IsTurboAvailable;
		private bool IsPushGasAvailable;
		private bool IsReleaseGasAvailable;


		#region Initialization

		private void Start()
		{
			RefreshStates();
			//ScaleConfigurations(); Better do this only in editor.
		}

		#endregion

		#region Deinitialization

		//private void OnDestroy()
		//{
		//}

		#endregion

		private void FixedUpdate()
		{
			GasInput = GasInput.Clamp01();

			var targetLoad = GasInput;
			Load += (targetLoad - Load) * LoadSmoothness;

			var diff = (GasInput - PreviousGasInput) / Time.deltaTime;
			if (diff > MinRateOfChangeToPush && IsPushGasAvailable)
			{
				var now = Time.time;
				if (now > LastPushEffectTime + MinIntervalBetweenPushEffects)
				{
					LastPushEffectTime = now;
					PushGasSource.Play();
				}
			}
			if (-diff > MinRateOfChangeToRelease && IsReleaseGasAvailable)
			{
				var now = Time.time;
				if (now > LastReleaseEffectTime + MinIntervalBetweenReleaseEffects)
				{
					LastReleaseEffectTime = now;
					ReleaseGasSource.Play();
				}
			}

			PreviousGasInput = GasInput;
		}

		private void Update()
		{
			var rpm = RPM.Value;
			var load = Load;
			var inverseLoad = 1f - Load;

			for (int i = 0; i < ClipConfigurations.Count; i++)
			{
				var clipConfiguration = ClipConfigurations[i];
				var frequency = clipConfiguration.FrequencyCurve.Evaluate(rpm);
				var volume = clipConfiguration.VolumeCurve.Evaluate(rpm);
				if (clipConfiguration.UseOnlyHighLoad)
				{
					clipConfiguration.HighLoadSource.pitch = frequency;
					clipConfiguration.HighLoadSource.volume = volume;
				}
				else
				{
					clipConfiguration.HighLoadSource.pitch = frequency;
					clipConfiguration.HighLoadSource.volume = volume * load;
					clipConfiguration.LowLoadSource.pitch = frequency;
					clipConfiguration.LowLoadSource.volume = volume * inverseLoad;
				}
			}

			if (IsTurboAvailable)
			{
				TurboAudioSource.pitch = TurboFrequencyCurve.Evaluate(Turbo);
				TurboAudioSource.volume = TurboVolumeCurve.Evaluate(Turbo);
			}
		}

		public void RefreshStates()
		{
			RPM.Min = 0f;
			RPM.Max = MaxRPM;

			IsTurboAvailable = TurboAudioSource.IsNotNullAndHasClip();
			IsPushGasAvailable = PushGasSource.IsNotNullAndHasClip();
			IsReleaseGasAvailable = ReleaseGasSource.IsNotNullAndHasClip();

			//for (var i = 0; i < ClipConfigurations.Count; i++)
			//{
			//	var configuration = ClipConfigurations[i];
			//	configuration.IsHighLoadAvailable = configuration.HighLoadSource.IsNotNullAndHasClip();
			//	configuration.IsLowLoadAvailable = configuration.LowLoadSource.IsNotNullAndHasClip();
			//}
		}

		public void ScaleConfigurations()
		{
			for (int i = 0; i < ClipConfigurations.Count; i++)
			{
				var clipConfiguration = ClipConfigurations[i];
				clipConfiguration.VolumeCurve.AbsoluteScaleHorizontal(MaxRPM);
				clipConfiguration.VolumeCurve.ClampVertical(0f, 3f);
				clipConfiguration.FrequencyCurve.AbsoluteScaleHorizontal(MaxRPM);
				clipConfiguration.FrequencyCurve.ClampVertical(0f, 3f);
			}
		}

		#region Editor

#if UNITY_EDITOR

		private void OnValidate()
		{
			RefreshStates();
			ScaleConfigurations();
		}

#endif

		#endregion
	}

}

#endif
