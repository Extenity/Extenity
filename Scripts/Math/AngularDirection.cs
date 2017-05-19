using System;

namespace Extenity.MathToolbox
{

	public enum AngularDirection : byte
	{
		None = 0,
		CW = 1,
		CCW = 2,
	}

	public static class AngularDirectionExt
	{
		public static float ToFloat(this AngularDirection me)
		{
			switch (me)
			{
				case AngularDirection.None: return 0f;
				case AngularDirection.CW: return 1f;
				case AngularDirection.CCW: return -1f;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

}
