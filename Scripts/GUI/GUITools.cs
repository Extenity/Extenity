using System;
using System.Reflection;
using Extenity.Applicational;
using UnityEngine;
using UnityEngine.EventSystems;

public enum GUIAnchor // TODO: rename to ScreenAnchor
{
	LeftTop,
	Top,
	RightTop,
	Right,
	RightBottom,
	Bottom,
	LeftBottom,
	Left,
	Center,
}

public static class GUILayoutTools
{
	public static bool Button(string text, bool buttonEnabled = true, params GUILayoutOption[] options)
	{
		var enabledWas = GUI.enabled;
		if (!buttonEnabled)
		{
			GUI.enabled = false;
		}

		var result = GUILayout.Button(text, options);

		GUI.enabled = enabledWas;
		return result;
	}
}

public static class GUITools
{
	#region Initialization

	// This static initializer works for runtime, but apparently isn't called when
	// Editor play mode stops, so DrawLine will re-initialize if needed.
	static GUITools()
	{
		InitializeLineDrawer();
	}

	#endregion

	public static bool IsGUIActiveInCurrentEventSystem
	{
		get
		{
			var eventSystem = EventSystem.current;
			if (eventSystem)
			{
				if (eventSystem.currentSelectedGameObject != null)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool IsEnterHit
	{
		get { return Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return); }
	}

	public static void DisableTabTravel()
	{
		if (Event.current.keyCode == KeyCode.Tab || Event.current.character == '\t')
			Event.current.Use();
	}

	public static bool Button(Rect rect, string text, bool buttonEnabled = true)
	{
		var enabledWas = GUI.enabled;
		if (!buttonEnabled)
		{
			GUI.enabled = false;
		}

		var result = GUI.Button(rect, text);

		GUI.enabled = enabledWas;
		return result;
	}

	public static Rect Rect(int width, int height, GUIAnchor anchor, int marginX = 0, int marginY = 0)
	{
		switch (anchor)
		{
			case GUIAnchor.LeftTop: return new Rect(marginX, marginY, width, height);
			case GUIAnchor.Top: return new Rect((Screen.width - width) >> 1, marginY, width, height);
			case GUIAnchor.RightTop: return new Rect(Screen.width - width - marginX, marginY, width, height);
			case GUIAnchor.Right: return new Rect(Screen.width - width - marginX, (Screen.height - height) >> 1, width, height);
			case GUIAnchor.RightBottom: return new Rect(Screen.width - width - marginX, Screen.height - height - marginX, width, height);
			case GUIAnchor.Bottom: return new Rect((Screen.width - width) >> 1, Screen.height - height - marginY, width, height);
			case GUIAnchor.LeftBottom: return new Rect(marginX, Screen.height - height - marginY, width, height);
			case GUIAnchor.Left: return new Rect(marginX, (Screen.height - height) >> 1, width, height);
			case GUIAnchor.Center: return new Rect((Screen.width - width) >> 1, (Screen.height - height) >> 1, width, height);
			default: throw new ArgumentOutOfRangeException();
		}
	}

	public static Vector2 Point(GUIAnchor anchor, int marginX = 0, int marginY = 0)
	{
		switch (anchor)
		{
			case GUIAnchor.LeftTop: return new Vector2(marginX, marginY);
			case GUIAnchor.Top: return new Vector2(Screen.width >> 1, marginY);
			case GUIAnchor.RightTop: return new Vector2(Screen.width - marginX, marginY);
			case GUIAnchor.Right: return new Vector2(Screen.width - marginX, Screen.height >> 1);
			case GUIAnchor.RightBottom: return new Vector2(Screen.width - marginX, Screen.height - marginX);
			case GUIAnchor.Bottom: return new Vector2(Screen.width >> 1, Screen.height - marginY);
			case GUIAnchor.LeftBottom: return new Vector2(marginX, Screen.height - marginY);
			case GUIAnchor.Left: return new Vector2(marginX, Screen.height >> 1);
			case GUIAnchor.Center: return new Vector2(Screen.width >> 1, Screen.height >> 1);
			default: throw new ArgumentOutOfRangeException();
		}
	}

	public static Vector2 OrthographicPoint(Camera camera, GUIAnchor anchor, float offsetX = 0, float offsetY = 0)
	{
		var orthographicSizeY = camera.orthographicSize;
		var orthographicSizeX = orthographicSizeY / ScreenManager.Instance.CurrentAspectRatio;

		switch (anchor)
		{
			case GUIAnchor.LeftTop: return new Vector2(-orthographicSizeX + offsetX, orthographicSizeY - offsetY);
			case GUIAnchor.Top: return new Vector2(offsetX, orthographicSizeY - offsetY);
			case GUIAnchor.RightTop: return new Vector2(orthographicSizeX - offsetX, orthographicSizeY - offsetY);
			case GUIAnchor.Right: return new Vector2(orthographicSizeX - offsetX, offsetY);
			case GUIAnchor.RightBottom: return new Vector2(orthographicSizeX - offsetX, -orthographicSizeY + offsetY);
			case GUIAnchor.Bottom: return new Vector2(offsetX, -orthographicSizeY + offsetY);
			case GUIAnchor.LeftBottom: return new Vector2(-orthographicSizeX + offsetX, -orthographicSizeY + offsetY);
			case GUIAnchor.Left: return new Vector2(-orthographicSizeX + offsetX, offsetY);
			case GUIAnchor.Center: return new Vector2(offsetX, offsetY);
			default: throw new ArgumentOutOfRangeException();
		}
	}

	public static int CenterOrientedRectPositionX(int rectWidth, int windowWidth)
	{
		return (windowWidth - rectWidth) >> 1;
	}

	public static int CenterOrientedRectPositionY(int rectHeight, int windowHeight)
	{
		return (windowHeight - rectHeight) >> 1;
	}

	public static int CenterOrientedRectPositionX(int rectWidth)
	{
		return (Screen.width - rectWidth) >> 1;
	}

	public static int CenterOrientedRectPositionY(int rectHeight)
	{
		return (Screen.height - rectHeight) >> 1;
	}

	#region Conversions

	public static Vector2 ScreenToScreenRatioCoordinates(this Vector2 valueInPixels)
	{
		// TODO: OPTIMIZATION
		return new Vector2(valueInPixels.x / Screen.width, valueInPixels.y / Screen.height);
	}

	public static float ScreenToScreenRatioCoordinates(this float valueInPixels)
	{
		// TODO: OPTIMIZATION
		return valueInPixels / Screen.height;
	}

	public static Vector2 ScreenRatioToScreenCoordinates(this Vector2 valueInScreenRatio)
	{
		// TODO: OPTIMIZATION
		return new Vector2(valueInScreenRatio.x * Screen.width, valueInScreenRatio.y * Screen.height);
	}

	public static float ScreenRatioToScreenCoordinates(this float valueInScreenRatio)
	{
		// TODO: OPTIMIZATION
		return valueInScreenRatio * Screen.height;
	}

	public static Rect ScreenRatioToScreenCoordinates(this Rect valueInPixels)
	{
		return new Rect(
			valueInPixels.x * Screen.width,
			valueInPixels.y * Screen.height,
			valueInPixels.width * Screen.width,
			valueInPixels.height * Screen.height);
	}

	#endregion

	#region Drawing Tools

	public static void DrawRect(Rect rect) { DrawRect(rect, GUI.contentColor, 1.0f); }
	public static void DrawRect(Rect rect, Color color) { DrawRect(rect, color, 1.0f); }
	public static void DrawRect(Rect rect, float width) { DrawRect(rect, GUI.contentColor, width); }
	public static void DrawRect(Rect rect, Color color, float width)
	{
		DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y + rect.height), color, width);
	}

	// Line drawing routine originally courtesy of Linusmartensson:
	// http://forum.unity3d.com/threads/71979-Drawing-lines-in-the-editor
	//
	// Rewritten to improve performance by Yossarian King / August 2013.
	//
	// This version produces virtually identical results to the original (tested by drawing
	// one over the other and observing errors of one pixel or less), but for large numbers
	// of lines this version is more than four times faster than the original, and comes
	// within about 70% of the raw performance of Graphics.DrawTexture.
	//
	// Peak performance on my laptop is around 200,000 lines per second. The laptop is
	// Windows 7 64-bit, Intel Core2 Duo CPU 2.53GHz, 4G RAM, NVIDIA GeForce GT 220M.
	// Line width and anti-aliasing had negligible impact on performance.
	//
	// For a graph of benchmark results in a standalone Windows build, see this image:
	// https://app.box.com/s/hyuhi565dtolqdm97e00
	//
	// For a Google spreadsheet with full benchmark results, see:
	// https://docs.google.com/spreadsheet/ccc?key=0AvJlJlbRO26VdHhzeHNRMVF2UHZHMXFCTVFZN011V1E&usp=sharing

	// Draw a line in screen space, suitable for use from OnGUI calls from either
	// MonoBehaviour or EditorWindow. Note that this should only be called during repaint
	// events, when (Event.current.type == EventType.Repaint).
	//
	// Works by computing a matrix that transforms a unit square -- Rect(0,0,1,1) -- into
	// a scaled, rotated, and offset rectangle that corresponds to the line and its width.
	// A DrawTexture call used to draw a line texture into the transformed rectangle.
	//
	// More specifically:
	//      scale x by line length, y by line width
	//      rotate around z by the angle of the line
	//      offset by the position of the upper left corner of the target rectangle
	//
	// By working out the matrices and applying some trigonometry, the matrix calculation comes
	// out pretty simple. See https://app.box.com/s/xi08ow8o8ujymazg100j for a picture of my
	// notebook with the calculations.

	private static Texture2D aaLineTex = null;
	private static Texture2D lineTex = null;
	private static Material blitMaterial = null;
	private static Material blendMaterial = null;
	private static Rect lineRect = new Rect(0, 0, 1, 1);

	public static void DrawLine(Vector2 pointA, Vector2 pointB, bool antiAlias = true) { DrawLine(pointA, pointB, GUI.contentColor, 1.0f, antiAlias); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, bool antiAlias = true) { DrawLine(pointA, pointB, color, 1.0f, antiAlias); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB, float width, bool antiAlias = true) { DrawLine(pointA, pointB, GUI.contentColor, width, antiAlias); }
	public static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width, bool antiAlias = true)
	{
		// Normally the static initializer does this, but to handle texture reinitialization
		// after editor play mode stops we need this check in the Editor.
#if UNITY_EDITOR
		if (!lineTex)
		{
			InitializeLineDrawer();
		}
#endif

		// Note that theta = atan2(dy, dx) is the angle we want to rotate by, but instead
		// of calculating the angle we just use the sine (dy/len) and cosine (dx/len).
		float dx = pointB.x - pointA.x;
		float dy = pointB.y - pointA.y;
		float len = Mathf.Sqrt(dx * dx + dy * dy);

		// Early out on tiny lines to avoid divide by zero.
		// Plus what's the point of drawing a line 1/1000th of a pixel long??
		if (len < 0.001f)
		{
			return;
		}

		// Pick texture and material (and tweak width) based on anti-alias setting.
		Texture2D tex;
		Material mat;
		if (antiAlias)
		{
			// Multiplying by three is fine for anti-aliasing width-1 lines, but make a wide "fringe"
			// for thicker lines, which may or may not be desirable.
			width = width * 3.0f;
			tex = aaLineTex;
			mat = blendMaterial;
		}
		else
		{
			tex = lineTex;
			mat = blitMaterial;
		}

		float wdx = width * dy / len;
		float wdy = width * dx / len;

		Matrix4x4 matrix = Matrix4x4.identity;
		matrix.m00 = dx;
		matrix.m01 = -wdx;
		matrix.m03 = pointA.x + 0.5f * wdx;
		matrix.m10 = dy;
		matrix.m11 = wdy;
		matrix.m13 = pointA.y - 0.5f * wdy;

		// Use GL matrix and Graphics.DrawTexture rather than GUI.matrix and GUI.DrawTexture,
		// for better performance. (Setting GUI.matrix is slow, and GUI.DrawTexture is just a
		// wrapper on Graphics.DrawTexture.)
		GL.PushMatrix();
		GL.MultMatrix(matrix);
		Graphics.DrawTexture(lineRect, tex, lineRect, 0, 0, 0, 0, color, mat);
		GL.PopMatrix();
	}

	// Other than method name, DrawBezierLine is unchanged from Linusmartensson's original implementation.
	public static void DrawBezierLine(Vector2 start, Vector2 startTangent, Vector2 end, Vector2 endTangent, Color color, float width, bool antiAlias, int segments)
	{
		Vector2 lastV = CubeBezier(start, startTangent, end, endTangent, 0);
		for (int i = 1; i < segments; ++i)
		{
			Vector2 v = CubeBezier(start, startTangent, end, endTangent, i / (float)segments);
			DrawLine(lastV, v, color, width, antiAlias);
			lastV = v;
		}
	}

	private static Vector2 CubeBezier(Vector2 s, Vector2 st, Vector2 e, Vector2 et, float t)
	{
		float rt = 1 - t;
		return rt * rt * rt * s + 3 * rt * rt * t * st + 3 * rt * t * t * et + t * t * t * e;
	}

	private static void InitializeLineDrawer()
	{
		if (lineTex == null)
		{
			lineTex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			lineTex.SetPixel(0, 1, Color.white);
			lineTex.Apply();
		}
		if (aaLineTex == null)
		{
			// TODO: better anti-aliasing of wide lines with a larger texture? or use Graphics.DrawTexture with border settings
			aaLineTex = new Texture2D(1, 3, TextureFormat.ARGB32, false);
			aaLineTex.SetPixel(0, 0, new Color(1, 1, 1, 0));
			aaLineTex.SetPixel(0, 1, Color.white);
			aaLineTex.SetPixel(0, 2, new Color(1, 1, 1, 0));
			aaLineTex.Apply();
		}

		// GUI.blitMaterial and GUI.blendMaterial are used internally by GUI.DrawTexture,
		// depending on the alphaBlend parameter. Use reflection to "borrow" these references.
		blitMaterial = (Material)typeof(GUI).GetMethod("get_blitMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
		blendMaterial = (Material)typeof(GUI).GetMethod("get_blendMaterial", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);
	}

	#endregion
}
