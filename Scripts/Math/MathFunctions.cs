using Extenity.DebugToolbox;

public static class MathFunctions
{
	public static float TanHLike(float x, float centerX, float endX)
	{
		DebugAssert.IsTrue(centerX < endX);
		return TanHLike((x - centerX) / (endX - centerX));
	}

	public static double TanHLike(double x, double centerX, double endX)
	{
		DebugAssert.IsTrue(centerX < endX);
		return TanHLike((x - centerX) / (endX - centerX));
	}

	public static float TanHLike(float x, float endX)
	{
		DebugAssert.IsPositive(endX);
		return TanHLike(x / endX);
	}

	public static double TanHLike(double x, double endX)
	{
		DebugAssert.IsPositive(endX);
		return TanHLike(x / endX);
	}

	public static float TanHLike(float x)
	{
		if (x >= 1) return 1;
		if (x <= -1) return -1;
		if (x < 0)
		{
			x = x + 1;
			return x * x - 1;
		}
		x = 1 - x;
		return 1 - x * x;
	}

	public static double TanHLike(double x)
	{
		if (x >= 1) return 1;
		if (x <= -1) return -1;
		if (x < 0)
		{
			x = x + 1;
			return x * x - 1;
		}
		x = 1 - x;
		return 1 - x * x;
	}

	// TODO: Rename
	public static float Decreasing(float x)
	{
		// Function: 1 - ((1 - x) * (1 - x))
		if (x <= 0f)
			return 0f;
		if (x >= 1f)
			return 1f;
		var a = 1f - x;
		return 1 - a * a;
	}

	// TODO: Rename
	public static float Decreasing(float x, float endX)
	{
		// Function: 1 - ((1 - x * 5) * (1 - x * 5))
		if (x <= 0f)
			return 0f;
		if (x >= endX)
			return 1f;
		var a = 1f - (x / endX);
		return 1 - a * a;
	}

	// TODO: Rename
	public static float Decreasing(float x, float startX, float endX)
	{
		// Function: 1 - ((1 - (x - startX) / (endX - startX)) * (1 - (x - startX) / (endX - startX)))
		if (x <= startX)
			return 0f;
		if (x >= endX)
			return 1f;
		var a = 1f - ((x - startX) / (endX - startX));
		return 1 - a * a;
	}
}
