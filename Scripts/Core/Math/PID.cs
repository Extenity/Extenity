// Reference: http://brettbeauregard.com/blog/2011/04/improving-the-beginners-pid-introduction/

using System;

namespace Extenity.MathToolbox
{

	[Serializable]
	public struct PIDConfiguration
	{
		public float Kp;
		public float Ki;
		public float Kd;

		public bool IsValid { get { return Kp >= 0f && Ki >= 0f && Kd >= 0f; } }

		//public PIDConfiguration()
		//{
		//	Kp = 0f;
		//	Ki = 0f;
		//	Kd = 0f;
		//}

		public PIDConfiguration(float Kp, float Ki, float Kd)
		{
			this.Kp = Kp;
			this.Ki = Ki;
			this.Kd = Kd;
		}
	}

	[Serializable]
	public class PID
	{
		public double Input { get; set; }
		public double Output { get; private set; }
		public double Target { get; set; }

		private float SampleTime = 0.01f; // Seconds
		public float LastComputationTime { get; private set; }
		public double ITerm { get; private set; }
		public double LastInput { get; private set; }
		public double Kp { get; private set; }
		public double Ki { get; private set; }
		public double Kd { get; private set; }
		public double OutMin { get; private set; }
		public double OutMax { get; private set; }
		public float OutMinF { get { return (float)OutMin; } }
		public float OutMaxF { get { return (float)OutMax; } }
		public bool IsActive { get; private set; }
		private DirectionTypes Direction = DirectionTypes.Forward;
		public InputInitializationTypes InputInitializationType { get; set; }
		public IntegralInitializationTypes IntegralInitializationType { get; set; }

		public enum InputInitializationTypes : byte
		{
			Zero,
			Input,
		}

		public enum IntegralInitializationTypes : byte
		{
			Zero,
			Output,
		}

		public enum DirectionTypes : byte
		{
			Forward,
			Reverse,
		}

		#region Initialization

		public PID(InputInitializationTypes inputInitializationType, IntegralInitializationTypes integralInitializationType)
		{
			InputInitializationType = inputInitializationType;
			IntegralInitializationType = integralInitializationType;
		}

		public void Reset()
		{
			Input = 0f;
			Output = 0f;
			Target = 0f;
			LastComputationTime = 0f;
			ITerm = 0f;

			Initialize();
		}

		private void Initialize()
		{
			switch (InputInitializationType)
			{
				case InputInitializationTypes.Zero:
					LastInput = 0f;
					break;
				case InputInitializationTypes.Input:
					LastInput = Input;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			switch (IntegralInitializationType)
			{
				case IntegralInitializationTypes.Zero:
					ITerm = 0f;
					break;
				case IntegralInitializationTypes.Output:
					ITerm = Output;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// Clamp ITerm
			if (ITerm > OutMax)
				ITerm = OutMax;
			else if (ITerm < OutMin)
				ITerm = OutMin;
		}

		#endregion

		#region ID

		public string ID;

		#endregion

		public int Compute(float now)
		{
			if (!IsActive)
				return 0;

			var calculatedStepCount = 0;
			while (now - LastComputationTime >= SampleTime)
			{
				double difference = Target - Input;
				ITerm += (Ki * difference);
				if (ITerm > OutMax) ITerm = OutMax;
				else if (ITerm < OutMin) ITerm = OutMin;
				double deltaInput = (Input - LastInput);

				// Compute PID Output.
				Output = Kp * difference + ITerm - Kd * deltaInput;
				if (Output > OutMax) Output = OutMax;
				else if (Output < OutMin) Output = OutMin;

				// Remember some variables for next time.
				LastInput = Input;
				LastComputationTime += SampleTime;
				calculatedStepCount++;
			}
			return calculatedStepCount;
		}

		public void SetTunings(PIDConfiguration configuration)
		{
			if (!configuration.IsValid)
				throw new Exception("PID configuration is not valid.");

			Kp = configuration.Kp;
			Ki = configuration.Ki * SampleTime;
			Kd = configuration.Kd / SampleTime;

			if (Direction == DirectionTypes.Reverse)
			{
				Kp = (0 - Kp);
				Ki = (0 - Ki);
				Kd = (0 - Kd);
			}
		}

		public void SetSampleTime(float newSampleTime)
		{
			if (newSampleTime > 0 && !newSampleTime.IsAlmostEqual(SampleTime, 0.00001f))
			{
				double ratio = (double)newSampleTime / (double)SampleTime;
				Ki *= ratio;
				Kd /= ratio;
				SampleTime = newSampleTime;
			}
		}

		public void SetOutputLimits(double min, double max)
		{
			if (min > max)
				throw new ArgumentException("Min is greater than max.");

			OutMin = min;
			OutMax = max;

			// Clamp Output
			if (Output > OutMax)
				Output = OutMax;
			else if (Output < OutMin)
				Output = OutMin;

			// Clamp ITerm
			if (ITerm > OutMax)
				ITerm = OutMax;
			else if (ITerm < OutMin)
				ITerm = OutMin;
		}

		public void SetActive(bool active, float currentTime)
		{
			if (active == IsActive)
				return;

			if (active)
			{
				// We just went online.
				Initialize();

				LastComputationTime = currentTime;
			}
			IsActive = active;
		}

		public void SetDirection(DirectionTypes direction)
		{
			if (Direction == direction)
				return;

			Direction = direction;

			// Switch sign
			Kp = (0 - Kp);
			Ki = (0 - Ki);
			Kd = (0 - Kd);
		}

		public void SetInputAndTarget(double current, double target)
		{
			Input = current;
			Target = target;
		}
	}

}
