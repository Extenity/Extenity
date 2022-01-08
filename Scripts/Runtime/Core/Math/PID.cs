#if UNITY // TODO-UniversalExtenity: Convert these to Mathematics after importing it into Universal project.

// Reference: http://brettbeauregard.com/blog/2011/04/improving-the-beginners-pid-introduction/

using System;

namespace Extenity.MathToolbox
{

	[Serializable]
	public struct PIDConfiguration
	{
		public double Kp;
		public double Ki;
		public double Kd;

		public bool IsValid { get { return Kp >= 0.0 && Ki >= 0.0 && Kd >= 0.0; } }

		//public PIDConfiguration()
		//{
		//	Kp = 0.0;
		//	Ki = 0.0;
		//	Kd = 0.0;
		//}

		public PIDConfiguration(double Kp, double Ki, double Kd)
		{
			this.Kp = Kp;
			this.Ki = Ki;
			this.Kd = Kd;
		}
	}

	//[Serializable]
	public class PID
	{
		public double Output;
		public double Input;
		public double Target;
		public float OutputFloat => (float)Output;
		public float InputFloat => (float)Input;
		public float TargetFloat => (float)Target;

		private double SampleTime = 0.01f; // Seconds
		public double LastComputationTime;
		public double ITerm;
		public float ITermFloat => (float)ITerm;
		public double LastInput;
		public double Kp;
		public double Ki;
		public double Kd;
		public double OutMin;
		public double OutMax;
		public bool IsActive;
		private DirectionTypes Direction = DirectionTypes.Forward;
		public readonly InputInitializationTypes InputInitializationType;
		public readonly IntegralInitializationTypes IntegralInitializationType;

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
			InstanceID = ++LastGeneratedInstanceID;

			InputInitializationType = inputInitializationType;
			IntegralInitializationType = integralInitializationType;
		}

		public void Reset(double lastComputationTime = 0)
		{
			Input = 0f;
			Output = 0f;
			Target = 0f;
			LastComputationTime = lastComputationTime;
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

		#region Metadata - InstanceID

		[NonSerialized]
		public readonly int InstanceID;

		private static int LastGeneratedInstanceID = 100;

		#endregion

		#region Metadata - Name

		public string Name;

		#endregion

		public int Compute(double now)
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
				Kp = 0 - Kp;
				Ki = 0 - Ki;
				Kd = 0 - Kd;
			}
		}

		public bool SetSampleTime(double newSampleTime)
		{
			if (newSampleTime > 0 && !newSampleTime.IsAlmostEqual(SampleTime, 0.00001f))
			{
				double ratio = (double)newSampleTime / (double)SampleTime;
				Ki *= ratio;
				Kd /= ratio;
				SampleTime = newSampleTime;
				return true;
			}
			return false;
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

		public void SetActive(bool active, double currentTime)
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

#endif
