#if UNITY

#if UNITY_EDITOR
#define UNITY_DRAWER
#define DebugDrawAvailable
#endif

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Extenity.DataToolbox;
using Extenity.DesignPatternsToolbox;
using Extenity.MathToolbox;
using Extenity.TextureToolbox;
using Debug = UnityEngine.Debug;

namespace Extenity.DebugToolbox
{

	public class DebugDraw : AutoSingletonUnity<DebugDraw>
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

		protected override void AwakeDerived()
		{
			InitializeTextures();

#if !UNITY_DRAWER
			CreateLineMaterial();
#endif
		}

		#endregion

		#region Update

#if UNITY_DRAWER
		private void Update()
		{
			if (DebugDrawingDisabled)
				return;

			OnUpdateBar();
			OnUpdateWriteScreen();
			OnUpdateWriteScene();
		}
#endif

		#endregion

		#region GUI

#if UNITY_DRAWER
		private void OnGUI() // Ignored by Code Correct
		{
			if (DebugDrawingDisabled)
				return;

			DrawBar();
			DrawWriteScreen();
			DrawWriteScene();
		}
#endif

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
			TextureRed = TextureTools.CreateSimpleTexture(Color.red);
			TextureOrange = TextureTools.CreateSimpleTexture(new Color(1f, 0.4f, 0f, 1f));
			TextureYellow = TextureTools.CreateSimpleTexture(Color.yellow);
			TextureGreen = TextureTools.CreateSimpleTexture(Color.green);
			TextureCyan = TextureTools.CreateSimpleTexture(Color.cyan);
			TextureBlue = TextureTools.CreateSimpleTexture(Color.blue);
			TexturePurple = TextureTools.CreateSimpleTexture(new Color(0.3f, 0f, 0.8f, 1f));

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

		#endregion

		#region Shapes

		#region Line

		[Conditional("DebugDrawAvailable")]
		public static void Line(Vector3 start, Vector3 end, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(start, end, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void Line(Vector3 start, Vector3 end, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

#if !UNITY_DRAWER
			Instance.lineData.Add(new DebugLineData(start, end, color));
#else
			Debug.DrawLine(start, end, color, duration, depthTest);
#endif
		}

		[Conditional("DebugDrawAvailable")]
		public static void Line(Vector3 start, Vector3 end, Vector3 offset, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(start, end, offset, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void Line(Vector3 start, Vector3 end, Vector3 offset, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(start + offset, end + offset, color, duration, depthTest);
		}

		#endregion

		#region Ray

		[Conditional("DebugDrawAvailable")]
		public static void Ray(Ray ray, float length, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Ray(ray, length, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void Ray(Ray ray, float length, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Line(ray.origin, ray.origin + ray.direction * length, color, duration, depthTest);
		}

		#endregion

		#region Plus and Cross

		[Conditional("DebugDrawAvailable")]
		public static void Plus(Vector3 worldPosition, float size, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Plus(worldPosition, size, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
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

		[Conditional("DebugDrawAvailable")]
		public static void Cross(Vector3 worldPosition, float size, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Cross(worldPosition, size, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
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

		[Conditional("DebugDrawAvailable")]
		public static void CircleXZ(Circle circle, Transform transform, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			CircleXZ(circle.center, circle.radius, transform, color, angleStep, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void CircleXZ(Vector3 worldPosition, float radius, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 start = worldPosition + new Vector3(radius, 0f, 0f);

			for (float angle = angleStep; angle < Mathf.PI * 2f; angle += angleStep)
			{
				var end = worldPosition + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle));
				Line(start, end, color, duration, depthTest);
				start = end;
			}

			Line(start, worldPosition + new Vector3(radius, 0f, 0f), color, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void CircleXZ(Vector3 localPosition, float radius, Transform transform, Color color, float angleStep = 0.4f, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			Vector3 start = transform.TransformPoint(localPosition + new Vector3(radius, 0f, 0f));

			for (float angle = angleStep; angle < Mathf.PI * 2f; angle += angleStep)
			{
				var end = transform.TransformPoint(localPosition + new Vector3(radius * Mathf.Cos(angle), 0f, radius * Mathf.Sin(angle)));
				Line(start, end, color, duration, depthTest);
				start = end;
			}

			Line(start, transform.TransformPoint(localPosition + new Vector3(radius, 0f, 0f)), color, duration, depthTest);
		}

		#endregion

		#region Rectangle

		[Conditional("DebugDrawAvailable")]
		public static void RectangleXZ(Rectangle rect, float height, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;

			var points = rect.GetPointsXZ(height);

			Line(points[0], points[1], color, duration, depthTest);
			Line(points[1], points[2], color, duration, depthTest);
			Line(points[2], points[3], color, duration, depthTest);
			Line(points[3], points[0], color, duration, depthTest);
		}

		#endregion

		#region Plane

		[Conditional("DebugDrawAvailable")]
		public static void Plane(Vector3 center, Vector3 normal, float size, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			Plane(center, normal, size, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
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

		[Conditional("DebugDrawAvailable")]
		public static void AAB(Bounds bounds, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void AAB(Bounds bounds, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, color, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void AAB(Bounds bounds, Transform transform, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, transform, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void AAB(Bounds bounds, Transform transform, Color color, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(bounds.min, bounds.max, transform, color, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
		public static void AAB(Vector3 min, Vector3 max, float duration = 0f, bool depthTest = true)
		{
			if (DebugDrawingDisabled) return;
			AAB(min, max, DefaultColor, duration, depthTest);
		}

		[Conditional("DebugDrawAvailable")]
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

		[Conditional("DebugDrawAvailable")]
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

		#region Bar

		public class BarData
		{
			public int lineIndex;
			public NineSliceAnchor anchor;
			public float value;
			public float shadowedValue;
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

			public BarData(int lineIndex, NineSliceAnchor anchor)
			{
				this.shadowedValue = float.NaN;
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
			NineSliceAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth)
		{
			return UpdateOrCreateBarData(text, value ? 1f : 0f, 0f, 1f, 0f, anchor, lineIndex, duration, width, false);
		}

		public static BarData Bar(string text, bool value,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth)
		{
			return UpdateOrCreateBarData(text, value ? 1f : 0f, 0f, 1f, 0f, anchor, lineIndex, barColor, duration, width, false);
		}

		#endregion
		#region User - Bar 01

		public static BarData Bar01(string text, float value,
			NineSliceAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData Bar01(string text, float value,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar Centered 01

		public static BarData BarCentered01(string text, float value,
			NineSliceAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0.5f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData BarCentered01(string text, float value,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, 0f, 1f, 0.5f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar Symmetric 01

		public static BarData BarSymmetric01(string text, float value,
			NineSliceAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -1f, 1f, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData BarSymmetric01(string text, float value,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -1f, 1f, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar Symmetric

		public static BarData BarSymmetric(string text, float value, float maxValue,
			NineSliceAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -maxValue, maxValue, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData BarSymmetric(string text, float value, float maxValue,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, -maxValue, maxValue, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar with Min Max

		public static BarData Bar(string text, float value,
			float minValue, float maxValue,
			NineSliceAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, 0f, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData Bar(string text, float value,
			float minValue, float maxValue,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, 0f, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar with Min Max Center

		public static BarData Bar(string text, float value,
			float minValue, float maxValue, float centerValue,
			NineSliceAnchor anchor, int lineIndex,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, duration, width, clamp);
		}

		public static BarData Bar(string text, float value,
			float minValue, float maxValue, float centerValue,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration = DefaultBarDuration, int width = DefaultBarWidth, bool clamp = false)
		{
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, barColor, duration, width, clamp);
		}

		#endregion
		#region User - Bar Shadowed Value

		public static BarData BarShadowedValue(float shadowedValue, NineSliceAnchor anchor, int lineIndex)
		{
			return UpdateOrCreateBarDataForOnlyShadowedValue(shadowedValue, anchor, lineIndex);
		}

		public static BarData BarShadowedValueReset(NineSliceAnchor anchor, int lineIndex)
		{
			return UpdateOrCreateBarDataForOnlyShadowedValue(float.NaN, anchor, lineIndex);
		}

		#endregion

		#region UpdateOrCreateBarData

		private static BarData UpdateOrCreateBarData(string text, float value,
			float minValue, float maxValue, float centerValue,
			NineSliceAnchor anchor, int lineIndex,
			float duration, int width, bool clamp)
		{
			if (!IsInstanceAvailable)
				return null;
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, Instance.DefaultBarColor, Instance.DefaultBarBackgroundColor, duration, width, clamp);
		}

		private static BarData UpdateOrCreateBarData(string text, float value,
			float minValue, float maxValue, float centerValue,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor,
			float duration, int width, bool clamp)
		{
			if (!IsInstanceAvailable)
				return null;
			return UpdateOrCreateBarData(text, value, minValue, maxValue, centerValue, anchor, lineIndex, barColor, Instance.DefaultBarBackgroundColor, duration, width, clamp);
		}

		private static BarData UpdateOrCreateBarData(string text, float value,
			float minValue, float maxValue, float centerValue,
			NineSliceAnchor anchor, int lineIndex,
			Color barColor, Color backgroundColor,
			float duration, int width, bool clamp)
		{
			if (!IsInstanceAvailable)
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

		private static BarData UpdateOrCreateBarDataForOnlyShadowedValue(float shadowedValue, NineSliceAnchor anchor, int lineIndex)
		{
			if (!IsInstanceAvailable)
				return null;

			var data = Instance.GetOrCreateBarData(lineIndex, anchor);

			data.shadowedValue = shadowedValue;
			data.lastUpdateTime = Time.time;
			data.updatedInFrame = Time.frameCount;

			return data;
		}

		private BarData GetOrCreateBarData(int lineIndex, NineSliceAnchor anchor)
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

			if (barBackgroundTexture == null)
			{
				barBackgroundTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
			}
			if (barTexture == null)
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
				case NineSliceAnchor.TopLeft: rectArea.x = 0f; rectArea.y = data.lineIndex * BarAreaHeight; break;
				case NineSliceAnchor.Top: rectArea.x = (Screen.width - data.width) >> 1; rectArea.y = data.lineIndex * BarAreaHeight; break;
				case NineSliceAnchor.TopRight: rectArea.x = Screen.width - data.width; rectArea.y = data.lineIndex * BarAreaHeight; break;
				case NineSliceAnchor.Right: rectArea.x = Screen.width - data.width; rectArea.y = (Screen.height >> 1) + data.lineIndex * BarAreaHeight; break;
				case NineSliceAnchor.BottomRight: rectArea.x = Screen.width - data.width; rectArea.y = Screen.height - (data.lineIndex + 1) * BarAreaHeight; break;
				case NineSliceAnchor.Bottom: rectArea.x = (Screen.width - data.width) >> 1; rectArea.y = Screen.height - (data.lineIndex + 1) * BarAreaHeight; break;
				case NineSliceAnchor.BottomLeft: rectArea.x = 0; rectArea.y = Screen.height - (data.lineIndex + 1) * BarAreaHeight; break;
				case NineSliceAnchor.Left: rectArea.x = 0; rectArea.y = (Screen.height >> 1) + data.lineIndex * BarAreaHeight; break;
				default: throw new ArgumentOutOfRangeException();
			}

			switch (data.anchor)
			{
				case NineSliceAnchor.TopLeft: RectTools.Move(ref rectArea, BarAreaMargin, BarAreaMargin); break;
				case NineSliceAnchor.Top: RectTools.MoveY(ref rectArea, BarAreaMargin); break;
				case NineSliceAnchor.TopRight: RectTools.Move(ref rectArea, -BarAreaMargin, BarAreaMargin); break;
				case NineSliceAnchor.Right: RectTools.MoveX(ref rectArea, -BarAreaMargin); break;
				case NineSliceAnchor.BottomRight: RectTools.Move(ref rectArea, -BarAreaMargin, -BarAreaMargin); break;
				case NineSliceAnchor.Bottom: RectTools.MoveY(ref rectArea, -BarAreaMargin); break;
				case NineSliceAnchor.BottomLeft: RectTools.Move(ref rectArea, BarAreaMargin, -BarAreaMargin); break;
				case NineSliceAnchor.Left: RectTools.MoveX(ref rectArea, BarAreaMargin); break;
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
					if (!float.IsNaN(data.shadowedValue))
					{
						var barShadowedEnd = (int)(rectArea.width * ((data.shadowedValue - data.minValue) / (data.maxValue - data.minValue)));
						rect.width = barShadowedEnd - barStart;
						GUI.DrawTexture(rect, barTexture, ScaleMode.StretchToFill, true, 0f, new Color(0.6f, 0.6f, 0.6f, 0.6f), 0f, 0f);
					}
					rect.width = barEnd - barStart;
					GUI.DrawTexture(rect, barTexture);
					GUI.EndGroup();
				}
				else
				{
					GUI.DrawTexture(rectBackground, barBackgroundTexture);
					rect.x += barStart;
					if (!float.IsNaN(data.shadowedValue))
					{
						var barShadowedEnd = (int)(rectArea.width * ((data.shadowedValue - data.minValue) / (data.maxValue - data.minValue)));
						rect.width = barShadowedEnd - barStart;
						GUI.DrawTexture(rect, barTexture, ScaleMode.StretchToFill, true, 0f, new Color(0.6f, 0.6f, 0.6f, 0.6f), 0f, 0f);
					}
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

		#region Write To Screen

		private class WriteScreenData
		{
#pragma warning disable 649
			public int lineIndex;
			public NineSliceAnchor anchor;
			public string text;
			public Rect rect;
			public Color color;
			public float startTime;
			public float duration;
#pragma warning restore 649

			public WriteScreenData(int lineIndex, NineSliceAnchor anchor)
			{
				this.lineIndex = lineIndex;
				this.anchor = anchor;
			}
		}

		private List<WriteScreenData> writeScreenData = new List<WriteScreenData>(20);
		private const int writeScreenTextAreaHeight = 20;

		[Conditional("DebugDrawAvailable")]
		public static void WriteScreenRectSeparated(string prefix, Rect value, int lineIndexStart, NineSliceAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			WriteScreen(prefix + ".x = " + value.x, lineIndexStart, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".y = " + value.y, lineIndexStart + 1, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".width = " + value.width, lineIndexStart + 2, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".height = " + value.height, lineIndexStart + 3, anchor, textColor, duration, textAreaWidth);
		}

		[Conditional("DebugDrawAvailable")]
		public static void WriteScreenVector2Separated(string prefix, Vector2 value, int lineIndexStart, NineSliceAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			WriteScreen(prefix + ".x = " + value.x, lineIndexStart, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".y = " + value.y, lineIndexStart + 1, anchor, textColor, duration, textAreaWidth);
		}

		[Conditional("DebugDrawAvailable")]
		public static void WriteScreenVector3Separated(string prefix, Vector3 value, int lineIndexStart, NineSliceAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			WriteScreen(prefix + ".x = " + value.x, lineIndexStart, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".y = " + value.y, lineIndexStart + 1, anchor, textColor, duration, textAreaWidth);
			WriteScreen(prefix + ".z = " + value.z, lineIndexStart + 2, anchor, textColor, duration, textAreaWidth);
		}

		[Conditional("DebugDrawAvailable")]
		public static void WriteScreen(string text, int lineIndex, NineSliceAnchor anchor, Color textColor, float duration = 1f, int textAreaWidth = 250)
		{
			if (!IsInstanceAvailable)
				return;

			var data = Instance.GetOrCreateWriteScreenData(lineIndex, anchor);

			switch (anchor)
			{
				case NineSliceAnchor.TopLeft: data.rect.x = 0f; data.rect.y = lineIndex * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.Top: data.rect.x = (Screen.width - textAreaWidth) >> 1; data.rect.y = lineIndex * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.TopRight: data.rect.x = Screen.width - textAreaWidth; data.rect.y = lineIndex * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.Right: data.rect.x = Screen.width - textAreaWidth; data.rect.y = (Screen.height >> 1) + lineIndex * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.BottomRight: data.rect.x = Screen.width - textAreaWidth; data.rect.y = Screen.height - (lineIndex + 1) * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.Bottom: data.rect.x = (Screen.width - textAreaWidth) >> 1; data.rect.y = Screen.height - (lineIndex + 1) * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.BottomLeft: data.rect.x = 0; data.rect.y = Screen.height - (lineIndex + 1) * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.Left: data.rect.x = 0; data.rect.y = (Screen.height >> 1) + lineIndex * writeScreenTextAreaHeight; break;
				case NineSliceAnchor.Center: data.rect.x = (Screen.width - textAreaWidth) >> 1; data.rect.y = (Screen.height >> 1) + lineIndex * writeScreenTextAreaHeight; break;
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

		private WriteScreenData GetOrCreateWriteScreenData(int lineIndex, NineSliceAnchor anchor)
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

		[Conditional("DebugDrawAvailable")]
		public static void WriteScene(string text, int lineIndex, Vector3 position, Color textColor, float duration = 1f)
		{
			if (!IsInstanceAvailable)
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
				if (data.lineIndex == lineIndex && data.position.IsAlmostEqual(position))
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

#endif
