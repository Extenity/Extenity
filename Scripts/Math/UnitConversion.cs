using UnityEngine;

public static class UnitConversion
{
	public static readonly float RadOverSecToRPM = 9.54929659643f;
	public static readonly float RPMToRadOverSec = 1f / RadOverSecToRPM;
	public static readonly float RPMToDegreesOverSec = Mathf.Rad2Deg / RadOverSecToRPM;
}
