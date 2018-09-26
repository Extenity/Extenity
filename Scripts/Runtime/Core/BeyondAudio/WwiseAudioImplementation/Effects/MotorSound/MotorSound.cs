#if BeyondAudioUsesWwiseAudio

using System;
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

		[SerializeField]
		[FormerlySerializedAs("_MaxRPM")]
		public float MaxRPM;

		[Header("RPM")]
		public float LoadSmoothness = 0.08f;

		[Header("Gas Leaks")]
		//public AudioSource PushGasSource;
		//public AudioSource ReleaseGasSource;
		public float MinRateOfChangeToPush = 1f;
		public float MinRateOfChangeToRelease = 1f;
		public float MinIntervalBetweenPushEffects = 0.3f;
		public float MinIntervalBetweenReleaseEffects = 0.3f;

		[NonSerialized]
		public float Load;

		private float LastPushEffectTime;
		private float LastReleaseEffectTime;

		private bool IsPushGasAvailable;
		private bool IsReleaseGasAvailable;


		#region Initialization

		private void Start()
		{
			RefreshStates();
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
					// TODO: AUDIO: Inform Wwise
					//PushGasSource.Play();
				}
			}
			if (-diff > MinRateOfChangeToRelease && IsReleaseGasAvailable)
			{
				var now = Time.time;
				if (now > LastReleaseEffectTime + MinIntervalBetweenReleaseEffects)
				{
					LastReleaseEffectTime = now;
					// TODO: AUDIO: Inform Wwise
					//ReleaseGasSource.Play();
				}
			}

			PreviousGasInput = GasInput;
		}

		private void Update()
		{
			//var rpm = RPM.Value;
			//var load = Load;
			//var inverseLoad = 1f - Load;

			// TODO: AUDIO: Update RTPCs here
			//UpdateRTPC(rpm);
			//UpdateRTPC(load);
			//UpdateRTPC(inverseLoad);
		}

		public void RefreshStates()
		{
			RPM.Min = 0f;
			RPM.Max = MaxRPM;

			// TODO: AUDIO: Is there a way to know if we have this events defined in Wwise?
			//IsPushGasAvailable = PushGasSource.IsNotNullAndHasClip();
			//IsReleaseGasAvailable = ReleaseGasSource.IsNotNullAndHasClip();
		}

		#region Editor

		private void OnValidate()
		{
			RefreshStates();
		}

		#endregion
	}

}

#endif
