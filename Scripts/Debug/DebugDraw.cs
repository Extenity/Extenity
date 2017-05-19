#if UNITY_EDITOR
#define UNITY_DRAWER
#endif

using System;
using UnityEngine;
using System.Collections.Generic;
using Extenity.DesignPatternsToolbox;
using Extenity.MathToolbox;

namespace Extenity.DebugToolbox
{

	public class DebugDraw : SingletonUnity<DebugDraw>
	{
		public static bool DebugDrawingDisabled = false;

		public static Color[] DebugColors =
			{
			Color.magenta,
			Color.cyan,
			Color.yellow,
			Color.white,
			Color.red,
			Color.green,
			Color.blue,
		};

		public static Color DefaultColor = Color.white;

		#region Initialization

		private void Awake()
		{
			InitializeSingleton(this, false);

			InitializeTextures();

#if !UNITY_DRAWER
		CreateLineMaterial();
#endif
		}

		#endregion

		#region Update

		private void Update()
		{
			if (DebugDrawingDisabled)
				return;

			OnUpdateBar();
			OnUpdateWriteScreen();
			OnUpdateWriteScene();
		}

		#endregion

		#region GUI

		private void OnGUI()
		{
			if (DebugDrawingDisabled)
				return;

			DrawBar();
			DrawWriteScreen();
			DrawWriteScene();
		}

		#endregion

		#region Draw debug lines outside of editor

#if !UNITY_DRAWER

	private Material lineMaterial;
	private List<DebugLineData> lineData;

	private struct DebugLineData
	{
		public Vector3 start;
		public Vector3 end;
		public Color color;

		public DebugLineData(Vector3 start, Vector3 end, Color color)
		{
			this.start = start;
			this.end = end;
			this.color = color;
		}
	}

	private void CreateLineMaterial()
	{
		lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
		lineMaterial.hideFlags = HideFlags.HideAndDontSave;
		lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;

		lineData = new List<DebugLineData>(1000);
	}

	void OnPostRender()
	{
		if (DebugDrawingDisabled) return;

		//GL.PushMatrix();
		//GL.LoadIdentity();
		lineMaterial.SetPass(0);
		GL.Begin(GL.LINES);

		for (int i = 0; i < lineData.Count; i++)
		{
			var debugLineData = lineData[i];
			GL.Color(debugLineData.color);
			GL.Vertex(debugLineData.start);
			GL.Vertex(debugLineData.end);
		}

		GL.End();
		//GL.PopMatrix();

		lineData.Clear();
	}

#endif

		#endregion

		#region Texture

		public static Texture2D TextureRed;
		public static Texture2D TextureOrange;
		public static Texture2D TextureYellow;
		public static Texture2D TextureGreen;
		public static Texture2D TextureCyan;
		public static Texture2D TextureBlue;
		public static Texture2D TexturePurple;
		public static Texture2D[] ColoredTextures;

		private static void InitializeTextures()
		{
			TextureRed = CreateColoredTexture(Color.red);
			TextureOrange = CreateColoredTexture(new Color(1f, 0.4f, 0f, 1f));
			TextureYellow = CreateColoredTexture(Color.yellow);
			TextureGreen = CreateColoredTexture(Color.green);
			TextureCyan = CreateColoredTexture(Color.cyan);
			TextureBlue = CreateColoredTexture(Color.blue);
			TexturePurple = CreateColoredTexture(new Color(0.3f, 0f, 0.8f, 1f));

			ColoredTextures = new[]
			{
			TextureRed,
			TextureOrange,
			TextureYellow,
			TextureGreen,
			TextureCyan,
			TextureBlue,
			TexturePurple,
		};
		}

		private static Texture2D CreateColoredTexture(Color color)
		{
			var texture = new Texture2D(4, 4, TextureFormat.ARGB32, false);
			texture.SetPixels(0, 0, 4, 4, new[]
			{
			color, color, color, color,
			color, color, color, color,
			color, color, color, color,
			color, color, color, color,
		});

			texture.Apply(false, true);
			return texture;
		}

		#endregion

		#region Shapes

		#region Line

		public static void Line(Vector3 start, Vector3 end, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(start, end, DefaultColor, duration, depthTest);
		}

		public static void Line(Vector3 start, Vector3 end, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

#if !UNITY_DRAWER
		Instance.lineData.Add(new DebugLineData(start, end, color));
#else
			Debug.DrawLine(start, end, color, duration, depthTest);
#endif
		}

		public static void Line(Vector3 start, Vector3 end, Vector3 offset, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(start, end, offset, DefaultColor, duration, depthTest);
		}

		public static void Line(Vector3 start, Vector3 end, Vector3 offset, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(start + offset, end + offset, color, duration, depthTest);
		}

		#endregion

		#region Ray

		public static void Ray(Ray ray, float length, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Ray(ray, length, DefaultColor, duration, depthTest);
		}

		public static void Ray(Ray ray, float length, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(ray.origin, ray.origin + ray.direction * length, color, duration, depthTest);
		}

		#endregion

		#region Plus and Cross

		public static void Plus(Vector3 worldPosition, float size, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Plus(worldPosition, size, DefaultColor, duration, depthTest);
		}

		public static void Plus(Vector3 worldPosition, float size, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 diffX = new Vector3(size, 0f, 0f);
			Vector3 diffY = new Vector3(0f, size, 0f);
			Vector3 diffZ = new Vector3(0f, 0f, size);
			Line(worldPosition - diffX, worldPosition + diffX, color, duration, depthTest);
			Line(worldPosition - diffY, worldPosition + diffY, color, duration, depthTest);
			Line(worldPosition - diffZ, worldPosition + diffZ, color, duration, depthTest);
		}

		public static void Cross(Vector3 worldPosition, float size, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Cross(worldPosition, size, DefaultColor, duration, depthTest);
		}

		public static void Cross(Vector3 worldPosition, float size, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 diff1 = new Vector3(size, size, 0f);
			Vector3 diff2 = new Vector3(-size, size, 0f);
			Vector3 diff3 = new Vector3(0f, size, size);
			Vector3 diff4 = new Vector3(0f, size, -size);
			Vector3 diff5 = new Vector3(size, 0f, size);
			Vector3 diff6 = new Vector3(size, 0f, -size);
			Line(worldPosition - diff1, worldPosition + diff1, color, duration, depthTest);
			Line(worldPosition - diff2, worldPosition + diff2, color, duration, depthTest);
			Line(worldPosition - diff3, worldPosition + diff3, color, duration, depthTest);
			Line(worldPosition - diff4, worldPosition + diff4, color, duration, depthTest);
			Line(worldPosition - diff5, worldPosition + diff5, color, duration, depthTest);
			Line(worldPosition - diff6, worldPosition + diff6, color, duration, depthTest);
		}

		#endregion

		#region Circle

		public static void CircleXZ(Circle circle, Transform transform, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			CircleXZ(circle.center, circle.radius, transform, color, angleStep, duration, depthTest);
		}

		public static void CircleXZ(Vector3 worldPosition, float radius, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 start = worldPosition + new Vector3(radius, 0f, 0f);
			Vector3 end;

			for (float angle = angleStep; angle < Mathf.PI * 2f; angle += angleStep)
			{
				end = worldPosition + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
				Line(start, end, color, duration, depthTest);
				start = end;
			}

			Line(start, worldPosition + new Vector3(radius, 0f, 0f), color, duration, depthTest);
		}

		public static void CircleXZ(Vector3 localPosition, float radius, Transform transform, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 start = transform.TransformPoint(localPosition + new Vector3(radius, 0f, 0f));
			Vector3 end;

			for (float angle = angleStep; angle < Mathf.PI * 2f; angle += angleStep)
			{
				end = transform.TransformPoint(localPosition + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle)));
				Line(start, end, color, duration, depthTest);
				start = end;
			}

			Line(start, transform.TransformPoint(localPosition + new Vector3(radius, 0f, 0f)), color, duration, depthTest);
		}

		#endregion

		#region Rectangle

		public static void RectangleXZ(Rectangle rect, float height, Color color, float duration = 0f, bool depthTest = true)
		{
			var points = rect.GetPointsXZ(height);

			Line(points[0], points[1], color, duration, depthTest);
			Line(points[1], points[2], color, duration, depthTest);
			Line(points[2], points[3], color, duration, depthTest);
			Line(points[3], points[0], color, duration, depthTest);
		}

		#endregion

		#region Plane

		public static void Plane(Vector3 center, Vector3 normal, float size, float duration = 0f, bool depthTest = true)
		{
			Plane(center, normal, size, DefaultColor, duration, depthTest);
		}

		public static void Plane(Vector3 center, Vector3 normal, float size, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 p0 = new Vector3(-size, 0, -size); // left forward
			Vector3 p1 = new Vector3(size, 0, -size); // right forward
			Vector3 p2 = new Vector3(size, 0, size); // right back
			Vector3 p3 = new Vector3(-size, 0, size); // left back

			Quaternion rot = Quaternion.LookRotation(normal, Vector3.up) * Quaternion.Euler(90f, 0.0f, 0.0f);

			p0 = rot * p0 + center;
			p1 = rot * p1 + center;
			p2 = rot * p2 + center;
			p3 = rot * p3 + center;

			Vector3 difP3P0 = (p3 - p0);
			Vector3 difP2P1 = (p2 - p1);
			Vector3 difP1P0 = (p1 - p0);
			Vector3 difP2P3 = (p2 - p3);

			float step = 0.1f;

			for (float xR = step; xR <= 1; xR += step)
			{
				Line(p0 + difP3P0 * xR, p1 + difP2P1 * xR, color * 0.5f, duration, depthTest);
			}

			for (float yR = step; yR <= 1; yR += step)
			{
				Line(p0 + difP1P0 * yR, p3 + difP2P3 * yR, color * 0.5f, duration, depthTest);
			}

			Line(p0, p1, color, duration, depthTest);
			Line(p1, p2, color, duration, depthTest);
			Line(p2, p3, color, duration, depthTest);
			Line(p3, p0, color, duration, depthTest);

			Line(center, center + normal, new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b, 1.0f), duration, depthTest);
		}

		#endregion

		#region AAB

		public static void AAB(Bounds bounds, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, DefaultColor, duration, depthTest);
		}

		public static void AAB(Bounds bounds, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, color, duration, depthTest);
		}

		public static void AAB(Bounds bounds, Transform transform, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, transform, DefaultColor, duration, depthTest);
		}

		public static void AAB(Bounds bounds, Transform transform, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, transform, color, duration, depthTest);
		}

		public static void AAB(Vector3 min, Vector3 max, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(min, max, DefaultColor, duration, depthTest);
		}

		public static void AAB(Vector3 min, Vector3 max, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 a = min;
			Vector3 b = min;

			b.x = max.x;
			Line(a, b, color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(a, b, color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(a, b, color, duration, depthTest);

			a.x = min.x;
			a.z = max.z;
			b.y = min.y;
			b.z = max.z;
			Line(a, b, color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(a, b, color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(a, b, color, duration, depthTest);

			a.x = min.x;
			a.z = min.z;
			b.x = min.x;
			b.y = min.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			b.x = max.x;
			b.y = min.y;
			Line(a, b, color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(a, b, color, duration, depthTest);

			//Line(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), color, duration, depthTest);
			//Line(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z), color, duration, depthTest);
			//Line(new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), color, duration, depthTest);
			//Line(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), color, duration, depthTest);

			//Line(new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), color, duration, depthTest);
			//Line(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color, duration, depthTest);
			//Line(new Vector3(min.x, max.y, max.z), new Vector3(max.x, max.y, max.z), color, duration, depthTest);
			//Line(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), color, duration, depthTest);

			//Line(new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z), color, duration, depthTest);
			//Line(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), color, duration, depthTest);
			//Line(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color, duration, depthTest);
			//Line(new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), color, duration, depthTest);
		}

		public static void AAB(Vector3 min, Vector3 max, Transform transform, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			if (!transform)
			{
				AAB(min, max, color);
				return;
			}

			Vector3 a = min;
			Vector3 b = min;

			b.x = max.x;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);

			a.x = min.x;
			a.z = max.z;
			b.y = min.y;
			b.z = max.z;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			b.x = min.x;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.x = max.x;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);

			a.x = min.x;
			a.z = min.z;
			b.x = min.x;
			b.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.x = max.x;
			a.y = min.y;
			b.x = max.x;
			b.y = min.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
			a.y = max.y;
			b.y = max.y;
			Line(transform.TransformPoint(a), transform.TransformPoint(b), color, duration, depthTest);
		}

		#endregion

		#endregion

		#region Mesh Helpers

		public static void MeshNormals(Mesh mesh, Transform transform, float lineLenght, Color color, float duration = 0f, bool depthTest = true)
		{
			if (mesh.normals == null)
				return;

			Vector3[] vertices = mesh.vertices;
			Vector3[] normals = mesh.normals;

			DebugAssert.IsEqual(normals.Length, vertices.Length);

			for (int i = 0; i < normals.Length; i++)
			{
				Line(vertices[i], vertices[i] + (normals[i] * lineLenght), color, duration, depthTest);
			}
		}

		#endregion

		#region Bar

		public class BarData
		{
			public int lineIndex;
			public GUIAnchor anchor;
			public float value;
			public float minValue;
			public float maxValue;
			public float centerValue;
			public bool clamp;
			public string text;
			public Color activeBarColor;
			public Color inactiveBarColor;
			public Color activeBackgroundColor;
			public Color inactiveBackgroundColor;
			public int width;
			public float duration;
			internal float lastUpdateTime;
			internal int updatedInFrame;

			public BarData(int lineIndex, GUIAnchor anchor)
			{
				this.lineIndex = lineIndex;
				this.anchor = anchor;
			}

			public Color CurrentBarColor
			{
				get { return updatedInFrame == Time.frameCount ? activeBarColor : inactiveBarColor; }
			}
			public Color CurrentBackgroundColor
			{
				get { return updatedInFrame == Time.frameCount ? activeBackgroundColor : inactiveBackgroundColor; }
			}

			public void SetBarColor(Color activeColor, float inactiveColorFactor = 0.8f)
			{
				activeBarColor = activeColor;
				inactiveBarColor = activeColor * inactiveColorFactor;
			}
			public void SetBackgroundColor(Color activeColor, float inactiveColorFactor = 0.8f)
			{
				activeBackgroundColor = activeColor;
				inactiveBackgroundColor = activeColor * inactiveColorFactor;
			}
		}

		public Color DefaultBarColor = Color.green;
		public Color DefaultBarBackgroundColor = Color.grey;
		private const float DefaultBarDuration = 1f;
		private const int DefaultBarWidth = 150;

		private const int BarAreaHeight = 40;
		private const int BarAreaMargin = 20;
		private const int BarBackgroundMargin = 3;
		private const int BarBackgroundMarginDouble = BarBackgroundMargin * 2;

		private List<BarData> barData = new List<BarData>(20);
		private GUIStyle barGuiStyleText;
		private GUIStyle barGuiStyleMinText;
		private GUIStyle barGuiStyleMaxText;
		private float barTextHeight;

		#region Initialization

		private void InitializeBars()
		{
			barGuiStyleText = new GUIStyle(GUI.skin.GetStyle("Label"));
			barGuiStyleText.fontSize = 11;
			barGuiStyleMinText = new GUIStyle(GUI.skin.GetStyle("Label"));
			barGuiStyleMaxText = new GUIStyle(GUI.skin.GetStyle("Label"));
			barGuiStyleMinText.normal.textColor *= 0.9f;
			barGuiStyleMaxText.normal.textColor *= 0.9f;
			barGuiStyleMinText.fontSize = 9;
			barGuiStyleMaxText.fontSize = 9;
			barGuiStyleMinText.alignment = TextAnchor.LowerLeft;
			barGuiStyleMaxText.alignment = TextAnchor.LowerRight;
			barTextHeight = barGuiStyleText.CalcSize(new GUIContent("A")).y;
		}

		private bool IsBarInitialized
		{
			get { return barGuiStyleMinText != null; }
		}

		#endregion

		#region User - Bar Bool

		public static BarData Bar(string text, bool value,
			GUIAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth)
		{
			return UpdateOrCreateBarData(text, value ? 1f : 0f, 0f, 1f, 0f, anchor, lineIndex, duration, width, false);
		}

		public static BarData Bar(string text, bool value,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth)
		{
			return UpdateOrCreateBarData(text, value ? 1f : 0f, 0f, 1f, 0f, anchor, lineIndex, barColor, duration, width, false);
		}

		#endregion
		#region User - Bar 01

		public static BarData Bar01(string text, float value,
			GUIAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData Bar01(string text, float value,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar Centered 01

		public static BarData BarCentered01(string text, float value,
			GUIAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0.5f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData BarCentered01(string text, float value,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0.5f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar Symmetric 01

		public static BarData BarSymmetric01(string text, float value,
			GUIAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -1f, 1f, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData BarSymmetric01(string text, float value,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -1f, 1f, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar Symmetric

		public static BarData BarSymmetric(string text, float value, float maxValue,
			GUIAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -maxValue, maxValue, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData BarSymmetric(string text, float value, float maxValue,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -maxValue, maxValue, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar with Min Max

		public static BarData Bar(string text, float value,
			float minValue, float maxValue,
			GUIAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData Bar(string text, float value,
			float minValue, float maxValue,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar with Min Max Center

		public static BarData Bar(string text, float value,
			float minValue, float maxValue, float centerValue,
			GUIAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData Bar(string text, float value,
			float minValue, float maxValue, float centerValue,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion

		#region UpdateOrCreateBarData

		private static BarData UpdateOrCreateBarData(string text, float value,
			float minValue, float maxValue, float centerValue,
			GUIAnchor anchor, int lineIndex,
			float duration, int width, bool clamp)
		{
			if (!Instance)
				return null;
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, Instance.DefaultBarColor, Instance.DefaultBarBackgroundColor, duration, width, clamp);
		}

		private static BarData UpdateOrCreateBarData(string text, float value,
			float minValue, float maxValue, float centerValue,
			GUIAnchor anchor, int lineIndex,
			Color barColor,
			float duration, int width, bool clamp)
		{
			if (!Instance)
				return null;
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, barColor, Instance.DefaultBarBackgroundColor, duration, width, clamp);
		}

		private static BarData UpdateOrCreateBarData(string text, float value,
			float minValue, float maxValue, float centerValue,
			GUIAnchor anchor, int lineIndex,
			Color barColor, Color backgroundColor,
			float duration, int width, bool clamp)
		{
			if (!Instance)
				return null;

			var data = Instance.GetOrCreateBarData(lineIndex, anchor);

			if (duration <= 0f)
				duration = 0.01f;

			data.value = value;
			data.minValue = minValue;
			data.maxValue = maxValue;
			data.centerValue = centerValue;
			data.clamp = clamp;
			data.text = text;
			data.SetBarColor(barColor);
			data.SetBackgroundColor(backgroundColor);
			data.width = width;
			data.duration = duration;
			data.lastUpdateTime = Time.time;
			data.updatedInFrame = Time.frameCount;

			return data;
		}

		private BarData GetOrCreateBarData(int lineIndex, GUIAnchor anchor)
		{
			for (int i = 0; i < barData.Count; i++)
			{
				var data = barData[i];
				if (data.lineIndex == lineIndex && data.anchor == anchor)
				{
					return data;
				}
			}

			var newData = new BarData(lineIndex, anchor);
			barData.Add(newData);
			return newData;
		}

		#endregion

		#region Update Bar

		private void OnUpdateBar()
		{
			float currentTime = Time.time;

			for (int i = barData.Count - 1; i >= 0; i--)
			{
				var data = barData[i];

				if (data.duration < 0f || data.lastUpdateTime + data.duration < currentTime)
				{
					barData.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Draw Bar

		private static Texture2D barBackgroundTexture;
		private static Texture2D barTexture;
		private static Color lastBarBackgroundColor;
		private static Color lastBarColor;

		private void DrawBar()
		{
			for (int i = 0; i < barData.Count; i++)
			{
				DrawBarInternal(barData[i]);
			}
		}

		private void DrawBarInternal(BarData data)
		{
			if (!IsBarInitialized)
			{
				InitializeBars();
			}

			if (!barBackgroundTexture)
			{
				barBackgroundTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			}
			if (!barTexture)
			{
				barTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			}

			if (lastBarBackgroundColor != data.CurrentBackgroundColor)
			{
				barBackgroundTexture.SetPixel(0, 0, data.CurrentBackgroundColor);
				barBackgroundTexture.Apply();
				lastBarBackgroundColor = data.CurrentBackgroundColor;
			}
			if (lastBarColor != data.CurrentBarColor)
			{
				barTexture.SetPixel(0, 0, data.CurrentBarColor);
				barTexture.Apply();
				lastBarColor = data.CurrentBarColor;
			}

			var rectArea = new Rect(0f, 0f, data.width, BarAreaHeight);

			switch (data.anchor)
			{
				case GUIAnchor.LeftTop: rectArea.x = 0f; rectArea.y = data.lineIndex * BarAreaHeight; break;
				case GUIAnchor.Top: rectArea.x = (Screen.width - data.width) >> 1; rectArea.y = data.lineIndex * BarAreaHeight; break;
				case GUIAnchor.RightTop: rectArea.x = Screen.width - data.width; rectArea.y = data.lineIndex * BarAreaHeight; break;
				case GUIAnchor.Right: rectArea.x = Screen.width - data.width; rectArea.y = (Screen.height >> 1) + data.lineIndex * BarAreaHeight; break;
				case GUIAnchor.RightBottom: rectArea.x = Screen.width - data.width; rectArea.y = Screen.height - (data.lineIndex + 1) * BarAreaHeight; break;
				case GUIAnchor.Bottom: rectArea.x = (Screen.width - data.width) >> 1; rectArea.y = Screen.height - (data.lineIndex + 1) * BarAreaHeight; break;
				case GUIAnchor.LeftBottom: rectArea.x = 0; rectArea.y = Screen.height - (data.lineIndex + 1) * BarAreaHeight; break;
				case GUIAnchor.Left: rectArea.x = 0; rectArea.y = (Screen.height >> 1) + data.lineIndex * BarAreaHeight; break;
				default: throw new ArgumentOutOfRangeException();
			}

			switch (data.anchor)
			{
				case GUIAnchor.LeftTop: MathTools.Move(ref rectArea, BarAreaMargin, BarAreaMargin); break;
				case GUIAnchor.Top: MathTools.MoveY(ref rectArea, BarAreaMargin); break;
				case GUIAnchor.RightTop: MathTools.Move(ref rectArea, -BarAreaMargin, BarAreaMargin); break;
				case GUIAnchor.Right: MathTools.MoveX(ref rectArea, -BarAreaMargin); break;
				case GUIAnchor.RightBottom: MathTools.Move(ref rectArea, -BarAreaMargin, -BarAreaMargin); break;
				case GUIAnchor.Bottom: MathTools.MoveY(ref rectArea, -BarAreaMargin); break;
				case GUIAnchor.LeftBottom: MathTools.Move(ref rectArea, BarAreaMargin, -BarAreaMargin); break;
				case GUIAnchor.Left: MathTools.MoveX(ref rectArea, BarAreaMargin); break;
				default: throw new ArgumentOutOfRangeException();
			}

			var rectBar = rectArea;
			rectBar.y += barTextHeight;
			rectBar.height *= 0.2f;

			var rectBackground = new Rect(
				rectBar.x - BarBackgroundMargin,
				rectBar.y - BarBackgroundMargin,
				rectBar.width + BarBackgroundMarginDouble,
				rectBar.height + BarBackgroundMarginDouble);

			// Draw text and value
			{
				if (string.IsNullOrEmpty(data.text))
				{
					GUI.Label(rectArea, data.value.ToString("0.0000"), barGuiStyleText);
				}
				else
				{
					GUI.Label(rectArea, data.text + ": " + data.value.ToString("0.0000"), barGuiStyleText);
				}
			}

			// Draw background and bar
			{
				var barStart = (int)(rectArea.width * ((data.centerValue - data.minValue) / (data.maxValue - data.minValue)));
				var barEnd = (int)(rectArea.width * ((data.value - data.minValue) / (data.maxValue - data.minValue)));
				var rect = rectBar;

				if (data.clamp)
				{
					GUI.DrawTexture(rectBackground, barBackgroundTexture);
					GUI.BeginGroup(rectBar);
					rect.x = barStart;
					rect.y = 0;
					rect.width = barEnd - barStart;
					GUI.DrawTexture(rect, barTexture);
					GUI.EndGroup();
				}
				else
				{
					GUI.DrawTexture(rectBackground, barBackgroundTexture);
					rect.x += barStart;
					rect.width = barEnd - barStart;
					GUI.DrawTexture(rect, barTexture);
				}
			}

			// Draw min max texts
			{
				var rect = rectBackground;
				rect.x += BarBackgroundMargin;
				rect.y += rectBackground.height;
				rect.width = rectBar.width;

				GUI.Label(rect, data.minValue.ToString("0.0000"), barGuiStyleMinText);
				GUI.Label(rect, data.maxValue.ToString("0.0000"), barGuiStyleMaxText);
			}

			if (data.duration == 0f)
			{
				data.duration = -1f; // Mark data to be removed in next update
			}
		}

		#endregion

		#endregion

		#region Frustum

		public static void DrawFrustum(Transform transform, float fovY, float maxRange, float minRange, float aspect, Color color, Vector3 center)
		{
			var previousMatrix = Gizmos.matrix;
			var previousColor = Gizmos.color;

			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawFrustum(center, fovY, maxRange, minRange, aspect);

			Gizmos.matrix = previousMatrix;
			Gizmos.color = previousColor;
		}

		#endregion

		#region Write To Screen

		private class WriteScreenData
		{
#pragma warning disable 649
			public int lineIndex;
			public GUIAnchor anchor;
			public string text;
			public Rect rect;
			public Color color;
			public float startTime;
			public float duration;
#pragma warning restore 649

			public WriteScreenData(int lineIndex, GUIAnchor anchor)
			{
				this.lineIndex = lineIndex;
				this.anchor = anchor;
			}
		}

		private List<WriteScreenData> writeScreenData = new List<WriteScreenData>(20);
		private const int writeScreenTextAreaHeight = 20;

		public static void WriteScreenRectSeparated(string prefix, Rect value, int lineIndexStart, GUIAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			WriteScreen(prefix + ".x = " + value.x, lineIndexStart, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".y = " + value.y, lineIndexStart + 1, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".width = " + value.width, lineIndexStart + 2, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".height = " + value.height, lineIndexStart + 3, anchor, textColor, duration, textAreaWidth);
		}

		public static void WriteScreenVector2Separated(string prefix, Vector2 value, int lineIndexStart, GUIAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			WriteScreen(prefix + ".x = " + value.x, lineIndexStart, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".y = " + value.y, lineIndexStart + 1, anchor, textColor, duration, textAreaWidth);
		}

		public static void WriteScreenVector3Separated(string prefix, Vector3 value, int lineIndexStart, GUIAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			WriteScreen(prefix + ".x = " + value.x, lineIndexStart, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".y = " + value.y, lineIndexStart + 1, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".z = " + value.z, lineIndexStart + 2, anchor, textColor, duration, textAreaWidth);
		}

		public static void WriteScreen(string text, int lineIndex, GUIAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			if (!Instance)
				return;

			var data = Instance.GetOrCreateWriteScreenData(lineIndex, anchor);

			switch (anchor)
			{
				case GUIAnchor.LeftTop: data.rect.x = 0f; data.rect.y = lineIndex * writeScreenTextAreaHeight; break;
				case GUIAnchor.Top: data.rect.x = (Screen.width - textAreaWidth) >> 1; data.rect.y = lineIndex * writeScreenTextAreaHeight; break;
				case GUIAnchor.RightTop: data.rect.x = Screen.width - textAreaWidth; data.rect.y = lineIndex * writeScreenTextAreaHeight; break;
				case GUIAnchor.Right: data.rect.x = Screen.width - textAreaWidth; data.rect.y = (Screen.height >> 1) + lineIndex * writeScreenTextAreaHeight; break;
				case GUIAnchor.RightBottom: data.rect.x = Screen.width - textAreaWidth; data.rect.y = Screen.height - (lineIndex + 1) * writeScreenTextAreaHeight; break;
				case GUIAnchor.Bottom: data.rect.x = (Screen.width - textAreaWidth) >> 1; data.rect.y = Screen.height - (lineIndex + 1) * writeScreenTextAreaHeight; break;
				case GUIAnchor.LeftBottom: data.rect.x = 0; data.rect.y = Screen.height - (lineIndex + 1) * writeScreenTextAreaHeight; break;
				case GUIAnchor.Left: data.rect.x = 0; data.rect.y = (Screen.height >> 1) + lineIndex * writeScreenTextAreaHeight; break;
				case GUIAnchor.Center: data.rect.x = (Screen.width - textAreaWidth) >> 1; data.rect.y = (Screen.height >> 1) + lineIndex * writeScreenTextAreaHeight; break;
				default: throw new ArgumentOutOfRangeException();
			}

			data.rect.width = textAreaWidth;
			data.rect.height = writeScreenTextAreaHeight;

			if (duration <= 0f)
				duration = 0.01f;

			data.text = text;
			data.color = textColor;
			data.startTime = Time.time;
			data.duration = duration;
		}

		private WriteScreenData GetOrCreateWriteScreenData(int lineIndex, GUIAnchor anchor)
		{
			for (int i = 0; i < writeScreenData.Count; i++)
			{
				var data = writeScreenData[i];
				if (data.lineIndex == lineIndex && data.anchor == anchor)
				{
					return data;
				}
			}

			var newData = new WriteScreenData(lineIndex, anchor);
			writeScreenData.Add(newData);
			return newData;
		}

		private void OnUpdateWriteScreen()
		{
			float currentTime = Time.time;

			for (int i = writeScreenData.Count - 1; i >= 0; i--)
			{
				var data = writeScreenData[i];

				if (data.startTime + data.duration < currentTime)
				{
					writeScreenData.RemoveAt(i);
				}
			}
		}

		private void DrawWriteScreen()
		{
			for (int i = 0; i < writeScreenData.Count; i++)
			{
				var data = writeScreenData[i];
				var oldColor = GUI.contentColor;
				GUI.contentColor = data.color;
				GUI.Label(data.rect, data.text);
				GUI.contentColor = oldColor;
			}
		}

		#endregion

		#region Write To Scene

		private class WriteSceneData
		{
			public int lineIndex;
			public Vector3 position;
			public string text;
			public Color color;
			public float startTime;
			public float duration;

			public WriteSceneData(int lineIndex, Vector3 position)
			{
				this.lineIndex = lineIndex;
				this.position = position;
			}
		}

		private List<WriteSceneData> writeSceneData = new List<WriteSceneData>(20);
		private const int writeSceneTextAreaHeight = 20;

		public static void WriteScene(string text, int lineIndex, Vector3 position, Color textColor, float duration = 1f)
		{
			if (!Instance)
				return;

			var data = Instance.GetOrCreateWriteSceneData(lineIndex, position);

			if (duration <= 0f)
				duration = 0.01f;

			data.text = text;
			data.color = textColor;
			data.startTime = Time.time;
			data.duration = duration;
		}

		private WriteSceneData GetOrCreateWriteSceneData(int lineIndex, Vector3 position)
		{
			for (int i = 0; i < writeSceneData.Count; i++)
			{
				var data = writeSceneData[i];
				if (data.lineIndex == lineIndex && data.position.IsAlmostEqualVector3(position))
				{
					return data;
				}
			}

			var newData = new WriteSceneData(lineIndex, position);
			writeSceneData.Add(newData);
			return newData;
		}

		private void OnUpdateWriteScene()
		{
			float currentTime = Time.time;

			for (int i = writeSceneData.Count - 1; i >= 0; i--)
			{
				var data = writeSceneData[i];

				if (data.startTime + data.duration < currentTime)
				{
					writeSceneData.RemoveAt(i);
				}
			}
		}

		private void DrawWriteScene()
		{
			for (int i = 0; i < writeSceneData.Count; i++)
			{
				var data = writeSceneData[i];
				var screenPosition = Camera.main.WorldToScreenPoint(data.position);

				GUIStyle style = "Label";
				var size = style.CalcSize(new GUIContent(data.text));
				var halfSize = size * 0.5f;
				var rect = new Rect(
					screenPosition.x - halfSize.x,
					Screen.height - (screenPosition.y + halfSize.y) - (data.lineIndex * writeSceneTextAreaHeight),
					size.x, size.y);

				var oldColor = GUI.contentColor;
				GUI.contentColor = data.color;
				GUI.Label(rect, data.text);
				GUI.contentColor = oldColor;
			}
		}

		#endregion

		#region Tools

		public static Color GetDebugColor(int i)
		{
			return DebugColors[i % DebugColors.Length];
		}

		#endregion
	}

}
