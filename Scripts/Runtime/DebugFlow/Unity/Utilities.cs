
namespace Extenity.DebugFlowTool.Unity
{

	public static class Utilities
	{
		public static UnityEngine.Color ToUnityColor(this Generic.Color color)
		{
			return new UnityEngine.Color(
				color.r / 255f,
				color.g / 255f,
				color.b / 255f,
				1f);
		}

		public static Generic.Color ToDebugFlowColor(this UnityEngine.Color color)
		{
			return new Generic.Color(
				(byte)(color.r * 255f),
				(byte)(color.g * 255f),
				(byte)(color.b * 255f));
		}
	}

}
