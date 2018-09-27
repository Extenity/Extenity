#if BeyondAudioUsesWwiseAudio

using System;
using Extenity.MathToolbox;
using UnityEngine;

namespace Extenity.BeyondAudio.Effects
{

	public class MotorSound : MonoBehaviour
	{
		[Range(0f, 1f)]
		public float GasInput;
		private float PreviousGasInput;
		public ClampedFloat RPM;

		[SerializeField]
		public float MaxRPM;

		[Header("RPM")]
		public AK.Wwise.RTPC EngineRPMParameter;
		public AK.Wwise.RTPC EngineLoadParameter;
		public AK.Wwise.RTPC EngineInverseLoadParameter;
		private bool IsEngineLoadAvailable;
		private bool IsEngineInverseLoadAvailable;
		public float LoadSmoothness = 0.08f;

		[Header("Gas Leaks")]
		public AK.Wwise.Event PushGasEvent;
		public AK.Wwise.Event ReleaseGasEvent;
		public float MinRateOfChangeToPush = 1f;
		public float MinRateOfChangeToRelease = 1f;
		public float MinIntervalBetweenPushEffects = 0.3f;
		public float MinIntervalBetweenReleaseEffects = 0.3f;

		[NonSerialized]
		public float Load;

		private float LastPushEffectTime;
		private float LastReleaseEffectTime;

		private bool IsPushGasOrReleaseGasAvailable;
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

			if (IsPushGasOrReleaseGasAvailable)
			{
				var diff = (GasInput - PreviousGasInput) / Time.deltaTime;
				if (diff > MinRateOfChangeToPush && IsPushGasAvailable)
				{
					var now = Time.time;
					if (now > LastPushEffectTime + MinIntervalBetweenPushEffects)
					{
						LastPushEffectTime = now;
						PushGasEvent.Post(gameObject);
					}
				}
				if (-diff > MinRateOfChangeToRelease && IsReleaseGasAvailable)
				{
					var now = Time.time;
					if (now > LastReleaseEffectTime + MinIntervalBetweenReleaseEffects)
					{
						LastReleaseEffectTime = now;
						ReleaseGasEvent.Post(gameObject);
					}
				}

				PreviousGasInput = GasInput;
			}
		}

		private void Update()
		{
			var rpm = RPM.Value;
			var load = Load;
			var inverseLoad = 1f - Load;

			EngineRPMParameter.SetGlobalValue(rpm);
			if (IsEngineLoadAvailable)
				EngineLoadParameter.SetGlobalValue(load);
			if (IsEngineInverseLoadAvailable)
				EngineInverseLoadParameter.SetGlobalValue(inverseLoad);
		}

		public void RefreshStates()
		{
			RPM.Min = 0f;
			RPM.Max = MaxRPM;

			IsEngineLoadAvailable = EngineLoadParameter.IsValid();
			IsEngineInverseLoadAvailable = EngineInverseLoadParameter.IsValid();

			IsPushGasAvailable = PushGasEvent.IsValid();
			IsReleaseGasAvailable = ReleaseGasEvent.IsValid();
			IsPushGasOrReleaseGasAvailable = IsPushGasAvailable || IsReleaseGasAvailable;
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
