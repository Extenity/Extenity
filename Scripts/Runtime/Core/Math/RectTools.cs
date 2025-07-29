#if UNITY_5_3_OR_NEWER

using Unity.Mathematics;
using UnityEngine;
using static Unity.Mathematics.math;

namespace Extenity.MathToolbox
{

	public static class RectTools
	{
		public static Rect FromMinMax(float minX, float minY, float maxX, float maxY)
		{
			return new Rect(minX,
			                minY,
			                maxX - minX,
			                maxY - minY);
		}

		public static Rect FromMinMax(Vector2 min, Vector2 max)
		{
			return new Rect(min.x,
			                min.y,
			                max.x - min.x,
			                max.y - min.y);
		}

		public static bool IsZero(this Rect me)
		{
			return me.x == 0 && me.y == 0 && me.width == 0 && me.height == 0;
		}

		public static Vector2 ClipPointInsideArea(this Rect area, Vector2 point)
		{
			if (point.x < area.xMin) point.x = area.xMin;
			if (point.y < area.yMin) point.y = area.yMin;
			if (point.x > area.xMax) point.x = area.xMax;
			if (point.y > area.yMax) point.y = area.yMax;
			return point;
		}

		public static Vector2 Center(this Rect rect)
		{
			return new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
		}

		public static Vector2 MinPoint(this Rect rect)
		{
			return new Vector2(rect.xMin, rect.yMin);
		}

		public static Vector2 MaxPoint(this Rect rect)
		{
			return new Vector2(rect.xMax, rect.yMax);
		}

		public static Rect Multiplied(this Rect rect, Vector2 xy)
		{
			return new Rect(rect.xMin * xy.x,
			                rect.yMin * xy.y,
			                rect.width * xy.x,
			                rect.height * xy.y);
		}

		public static Rect MultipliedAndRounded(this Rect rect, int2 xy)
		{
			return new Rect(round(rect.xMin * xy.x),
			                round(rect.yMin * xy.y),
			                round(rect.width * xy.x),
			                round(rect.height * xy.y));
		}

		public static Rect Combined(this Rect rect1, Rect rect2)
		{
			var xMin = min(rect1.xMin, rect2.xMin);
			var yMin = min(rect1.yMin, rect2.yMin);
			return new Rect(
				xMin,
				yMin,
				max(rect1.xMax, rect2.xMax) - xMin,
				max(rect1.yMax, rect2.yMax) - yMin);
		}

		public static Rect Expanded(this Rect rect, float expand)
		{
			return new Rect(
				rect.xMin - expand,
				rect.yMin - expand,
				rect.width + expand * 2f,
				rect.height + expand * 2f);
		}

		public static Rect Expanded(this Rect rect, float expandX, float expandY)
		{
			return new Rect(
				rect.xMin - expandX,
				rect.yMin - expandY,
				rect.width + expandX * 2f,
				rect.height + expandY * 2f);
		}

		public static Rect Expanded(this Rect rect, float expandTop, float expandLeft, float expandBottom, float expandRight)
		{
			return new Rect(
				rect.xMin - expandLeft,
				rect.yMin - expandTop,
				rect.width + (expandLeft + expandRight),
				rect.height + (expandTop + expandBottom));
		}

		public static Rect Expanded(this Rect rect, RectOffset expand)
		{
			return new Rect(
				rect.xMin - expand.left,
				rect.yMin - expand.top,
				rect.width + expand.horizontal,
				rect.height + expand.vertical);
		}

		public static void Move(ref Rect rect, Vector2 translation)
		{
			rect.x += translation.x;
			rect.y += translation.y;
		}

		public static void Move(ref Rect rect, float translationX, float translationY)
		{
			rect.x += translationX;
			rect.y += translationY;
		}

		public static void MoveX(ref Rect rect, float translationX)
		{
			rect.x += translationX;
		}

		public static void MoveY(ref Rect rect, float translationY)
		{
			rect.y += translationY;
		}
	}

}

#endif
