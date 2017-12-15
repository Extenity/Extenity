//#if BeyondAudioUsesUnityAudio

using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using UnityEngine;
using UnityEngine.Serialization;

namespace Extenity.BeyondAudio.Effects
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
			public AudioSource Source;
			public AudioSource SourceLow;
			public AnimationCurve VolumeCurve;
			public AnimationCurve FrequencyCurve;
		}

		public float LoadSmoothness = 0.08f;
		public AudioSource PushGasSource;
		public AudioSource ReleaseGasSource;
		public float MinRateOfChangeToPush = 1f;
		public float MinRateOfChangeToRelease = 1f;
		public float MinIntervalBetweenPushEffects = 0.3f;
		public float MinIntervalBetweenReleaseEffects = 0.3f;
		public List<ClipConfiguration> ClipConfigurations;

		[Header("Turbo")]
		public AudioSource TurboAudioSource;
		public AnimationCurve TurboVolumeCurve;
		public AnimationCurve TurboFrequencyCurve;

		[NonSerialized]
		public float Load;

		private float LastPushEffectTime;
		private float LastReleaseEffectTime;

		private void Start()
		{
			ScaleConfigurations();
		}

		private void OnValidate()
		{
			ScaleConfigurations();
		}

		private void FixedUpdate()
		{
			GasInput = GasInput.Clamp01();

			var targetLoad = GasInput;
			Load += (targetLoad - Load) * LoadSmoothness;

			var diff = (GasInput - PreviousGasInput) / Time.deltaTime;
			if (diff > MinRateOfChangeToPush && PushGasSource)
			{
				var now = Time.time;
				if (now > LastPushEffectTime + MinIntervalBetweenPushEffects)
				{
					LastPushEffectTime = now;
					PushGasSource.Play();
				}
			}
			if (-diff > MinRateOfChangeToRelease && ReleaseGasSource)
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
			for (int i = 0; i < ClipConfigurations.Count; i++)
			{
				var clipConfiguration = ClipConfigurations[i];
				var frequency = clipConfiguration.FrequencyCurve.Evaluate(RPM.Value);
				var volume = clipConfiguration.VolumeCurve.Evaluate(RPM.Value);
				clipConfiguration.Source.pitch = frequency;
				clipConfiguration.Source.volume = volume * Load;
				clipConfiguration.SourceLow.pitch = frequency;
				clipConfiguration.SourceLow.volume = volume * (1f - Load);
			}

			if (TurboAudioSource)
			{
				TurboAudioSource.pitch = TurboFrequencyCurve.Evaluate(Turbo);
				TurboAudioSource.volume = TurboVolumeCurve.Evaluate(Turbo);
			}
		}

		public void ScaleConfigurations()
		{
			RPM.Min = 0f;
			RPM.Max = MaxRPM;

			for (int i = 0; i < ClipConfigurations.Count; i++)
			{
				var clipConfiguration = ClipConfigurations[i];
				clipConfiguration.VolumeCurve.AbsoluteScaleHorizontal(MaxRPM);
				clipConfiguration.VolumeCurve.Clamp01Vertical();
				clipConfiguration.FrequencyCurve.AbsoluteScaleHorizontal(MaxRPM);
				clipConfiguration.FrequencyCurve.ClampVertical(0f, 3f);
			}
		}
	}

}

//#endif
