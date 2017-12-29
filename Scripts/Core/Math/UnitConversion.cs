using UnityEngine;

namespace Extenity.MathToolbox
{

	public static class UnitConversion
	{
		public const float RadiansPerSecondToRPM = 9.54929659643f;
		public const float RPMToRadiansPerSecond = 1f / RadiansPerSecondToRPM;
		public const float RPMToDegreesPerSecond = Mathf.Rad2Deg / RadiansPerSecondToRPM;

		public const float MetersPerSecondToKMH = 3.6f;
		public const float RPMToLinearVelocityInMetersPerSecond = 0.10472f;
		public const float RPMToLinearVelocityInKMH = RPMToLinearVelocityInMetersPerSecond * MetersPerSecondToKMH;

		public static float ConvertRPMToLinearSpeedInKMH(float rpm, float radius)
		{
			return rpm * radius * RPMToLinearVelocityInKMH;
		}

		public static float ConvertRPMToLinearSpeedInMetersPerSecond(float rpm, float radius)
		{
			return rpm * radius * RPMToLinearVelocityInMetersPerSecond;
		}

		public static float ConvertLinearSpeedInKMHToRPM(float speed, float radius)
		{
			return speed / radius / RPMToLinearVelocityInKMH;
		}

		public static float ConvertLinearSpeedInMetersPerSecondToRPM(float speed, float radius)
		{
			return speed / radius / RPMToLinearVelocityInMetersPerSecond;
		}
	}

}
