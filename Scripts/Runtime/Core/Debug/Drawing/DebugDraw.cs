#if UNITY_5_3_OR_NEWER

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
#if UNITY_DRAWER
using UnityEngine.UIElements;
#endif
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

#if UNITY_DRAWER
			// Nothing to draw yet. The component is enabled only while there are bars or texts on screen,
			// so LateUpdate does not run at all when there is nothing to draw.
			enabled = false;
#else
			CreateLineMaterial();
#endif
		}

		protected override void OnDestroyDerived()
		{
#if UNITY_DRAWER
			DestroyPanel();
#endif
		}

		#endregion

		#region Update

#if UNITY_DRAWER
		private void LateUpdate()
		{
			// LateUpdate, so that the values set in this frame's Update calls are shown in this frame.
			var currentTime = Time.time;
			RemoveExpiredBars(currentTime);
			RemoveExpiredWriteScreen(currentTime);
			RemoveExpiredWriteScene(currentTime);

			if (barData.Count == 0 && writeScreenData.Count == 0 && writeSceneData.Count == 0)
			{
				StopDrawing();
				return;
			}

			SetOverlayVisible(!DebugDrawingDisabled);
			if (DebugDrawingDisabled)
				return;

			RefreshBars();
			RefreshWriteScreen();
			RefreshWriteScene();
		}
#endif

		#endregion

		#region UI Toolkit Panel

#if UNITY_DRAWER

		/// <summary>
		/// The UI Toolkit theme of the panel that draws bars and texts. When not set, a theme that is already loaded
		/// is used, preferring Unity's default runtime theme. Set it before the first bar or text is drawn when the
		/// application does not load any UI Toolkit theme by itself.
		/// </summary>
		public static ThemeStyleSheet Theme;

		/// <summary>
		/// Sorting order of the panel that draws bars and texts. High by default, so that debug drawings stay above
		/// the application's own UI. Applied when the panel is created.
		/// </summary>
		public static float PanelSortingOrder = 32000f;

		private const string UnityDefaultRuntimeThemeName = "UnityDefaultRuntimeTheme";

		private PanelSettings panelSettings;
		private GameObject documentObject;
		private UIDocument document;
		private VisualElement overlayRoot;
		private bool isOverlayVisible = true;

		private void StartDrawing()
		{
			if (documentObject == null)
			{
				BuildPanel();
			}

			if (!documentObject.activeSelf)
			{
				documentObject.SetActive(true);
				// UIDocument creates a new root visual element each time it gets enabled, so the overlay is
				// attached again every time the document comes back.
				var documentRoot = document.rootVisualElement;
				documentRoot.pickingMode = PickingMode.Ignore;
				overlayRoot.RemoveFromHierarchy();
				documentRoot.Add(overlayRoot);
			}

			enabled = true;
		}

		private void StopDrawing()
		{
			enabled = false;
			if (documentObject != null)
			{
				// No panel to update or render while there is nothing to draw.
				documentObject.SetActive(false);
			}
		}

		private void SetOverlayVisible(bool visible)
		{
			if (isOverlayVisible == visible)
				return;
			isOverlayVisible = visible;
			overlayRoot.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
		}

		private void BuildPanel()
		{
			var theme = FindTheme();

			panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
			panelSettings.name = "DebugDrawPanelSettings";
			panelSettings.hideFlags = HideFlags.DontSave;
			panelSettings.sortingOrder = PanelSortingOrder;
			// One panel unit per screen pixel, so that the Screen based positions below map directly to the panel.
			panelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
			panelSettings.scale = 1f;
			panelSettings.clearColor = false;
			panelSettings.themeStyleSheet = theme;

			documentObject = new GameObject("DebugDrawDocument");
			documentObject.transform.SetParent(transform, false);
			// Inactive until StartDrawing, which attaches the overlay when it activates the document.
			documentObject.SetActive(false);
			document = documentObject.AddComponent<UIDocument>();
			document.panelSettings = panelSettings;

			overlayRoot = new VisualElement { name = "DebugDraw_Overlay", pickingMode = PickingMode.Ignore };
			overlayRoot.style.position = Position.Absolute;
			overlayRoot.style.left = 0f;
			overlayRoot.style.top = 0f;
			overlayRoot.style.right = 0f;
			overlayRoot.style.bottom = 0f;
			isOverlayVisible = true;
		}

		private static ThemeStyleSheet FindTheme()
		{
			if (Theme)
				return Theme;

			var loadedThemes = Resources.FindObjectsOfTypeAll<ThemeStyleSheet>();
			if (loadedThemes.Length == 0)
			{
				throw new InvalidOperationException($"{nameof(DebugDraw)} found no loaded UI Toolkit theme to draw its bars and texts with. Assign {nameof(DebugDraw)}.{nameof(Theme)} before drawing.");
			}
			foreach (var loadedTheme in loadedThemes)
			{
				if (loadedTheme.name == UnityDefaultRuntimeThemeName)
					return loadedTheme;
			}
			return loadedThemes[0];
		}

		private void DestroyPanel()
		{
			if (documentObject != null)
			{
				Destroy(documentObject);
				documentObject = null;
			}
			if (panelSettings != null)
			{
				Destroy(panelSettings);
				panelSettings = null;
			}
		}

		/// <summary>
		/// Colours given to DebugDraw are treated as linear colours in linear colour space projects, the same way
		/// Debug.DrawLine and immediate mode GUI treat them, while UI Toolkit takes them as sRGB. Converting keeps the
		/// on-screen colours the same as those of the lines.
		/// </summary>
		private static Color ToPanelColor(Color color)
		{
			return QualitySettings.activeColorSpace == ColorSpace.Linear ? color.gamma : color;
		}

		private static Label CreateLabel(string name, int fontSize)
		{
			var label = new Label { name = name, pickingMode = PickingMode.Ignore };
			label.style.position = Position.Absolute;
			label.style.marginLeft = 0f;
			label.style.marginRight = 0f;
			label.style.marginTop = 0f;
			label.style.marginBottom = 0f;
			label.style.paddingLeft = 0f;
			label.style.paddingRight = 0f;
			label.style.paddingTop = LabelVerticalPadding;
			label.style.paddingBottom = LabelVerticalPadding;
			label.style.fontSize = fontSize;
			return label;
		}

		private static VisualElement CreateBox(string name)
		{
			var box = new VisualElement { name = name, pickingMode = PickingMode.Ignore };
			box.style.position = Position.Absolute;
			return box;
		}

		private static void SetRect(VisualElement element, float x, float y, float width, float height)
		{
			element.style.left = x;
			element.style.top = y;
			element.style.width = width;
			element.style.height = height;
		}

		/// <summary>
		/// Places the element between two horizontal positions given in any order, like a rectangle with a negative
		/// width would be drawn.
		/// </summary>
		private static void SetHorizontalSpan(VisualElement element, int start, int end)
		{
			element.style.left = Mathf.Min(start, end);
			element.style.width = Mathf.Abs(end - start);
		}

		private const int LabelVerticalPadding = 3;
		private const int DefaultTextFontSize = 12;
		// Texts are drawn slightly dimmer than their given colors, like the default label style used to draw them.
		private static readonly Color TextColorTint = new Color(0.9f, 0.9f, 0.9f, 1f);

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
#if UNITY_DRAWER
			internal BarElements elements;
#endif

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

#if UNITY_DRAWER
		private const int BarTextFontSize = 11;
		private const int BarMinMaxTextFontSize = 9;
		// The height of a single line of bar text, including the label's vertical padding.
		private const float BarTextHeight = 18f;
		private const float BarHeight = BarAreaHeight * 0.2f;
		private static readonly Color BarTextColor = new Color(0.9f, 0.9f, 0.9f, 1f);
		private static readonly Color BarMinMaxTextColor = BarTextColor * 0.9f;
		private static readonly Color BarShadowedValueTint = new Color(0.6f, 0.6f, 0.6f, 0.6f);
#endif

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

#if UNITY_DRAWER
			Instance.StartDrawing();
#endif
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

#if UNITY_DRAWER
			Instance.StartDrawing();
#endif
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

#if UNITY_DRAWER

		private void RemoveExpiredBars(float currentTime)
		{
			for (int i = barData.Count - 1; i >= 0; i--)
			{
				var data = barData[i];

				if (data.duration < 0f || data.lastUpdateTime + data.duration < currentTime)
				{
					data.elements?.Root.RemoveFromHierarchy();
					data.elements = null;
					barData.RemoveAt(i);
				}
			}
		}

#endif

		#endregion

		#region Draw Bar

#if UNITY_DRAWER

		internal sealed class BarElements
		{
			public VisualElement Root;
			public Label Text;
			public VisualElement Background;
			public VisualElement BarArea;
			public VisualElement ShadowedBar;
			public VisualElement Bar;
			public Label MinText;
			public Label MaxText;

			// Last shown values, so that texts are only formatted again when they change.
			public string ShownText;
			public float ShownValue = float.NaN;
			public float ShownMinValue = float.NaN;
			public float ShownMaxValue = float.NaN;
			public Color ShownBarColor;
			public Color ShownBackgroundColor;
		}

		private void RefreshBars()
		{
			for (int i = 0; i < barData.Count; i++)
			{
				RefreshBar(barData[i]);
			}
		}

		private BarElements CreateBarElements()
		{
			var elements = new BarElements();

			elements.Root = CreateBox("DebugDraw_Bar");
			overlayRoot.Add(elements.Root);

			elements.Text = CreateLabel("DebugDraw_BarText", BarTextFontSize);
			elements.Text.style.color = ToPanelColor(BarTextColor);
			elements.Text.style.overflow = Overflow.Hidden;
			elements.Text.style.whiteSpace = WhiteSpace.Normal;
			elements.Text.style.unityTextAlign = TextAnchor.UpperLeft;
			elements.Root.Add(elements.Text);

			elements.Background = CreateBox("DebugDraw_BarBackground");
			elements.Root.Add(elements.Background);

			elements.BarArea = CreateBox("DebugDraw_BarArea");
			elements.Root.Add(elements.BarArea);

			elements.ShadowedBar = CreateBox("DebugDraw_ShadowedBar");
			elements.ShadowedBar.style.top = 0f;
			elements.ShadowedBar.style.height = BarHeight;
			elements.BarArea.Add(elements.ShadowedBar);

			elements.Bar = CreateBox("DebugDraw_BarValue");
			elements.Bar.style.top = 0f;
			elements.Bar.style.height = BarHeight;
			elements.BarArea.Add(elements.Bar);

			elements.MinText = CreateLabel("DebugDraw_BarMinText", BarMinMaxTextFontSize);
			elements.MinText.style.color = ToPanelColor(BarMinMaxTextColor);
			elements.MinText.style.unityTextAlign = TextAnchor.LowerLeft;
			elements.Root.Add(elements.MinText);

			elements.MaxText = CreateLabel("DebugDraw_BarMaxText", BarMinMaxTextFontSize);
			elements.MaxText.style.color = ToPanelColor(BarMinMaxTextColor);
			elements.MaxText.style.unityTextAlign = TextAnchor.LowerRight;
			elements.Root.Add(elements.MaxText);

			return elements;
		}

		private void RefreshBar(BarData data)
		{
			var elements = data.elements;
			if (elements == null)
			{
				elements = CreateBarElements();
				data.elements = elements;
			}

			var rectArea = CalculateBarArea(data);
			var width = rectArea.width;

			// The bar area sits right below the bar text. The background frames it with a margin and the min max texts
			// go right below the background.
			var barY = BarTextHeight;
			var backgroundHeight = BarHeight + BarBackgroundMarginDouble;
			SetRect(elements.Root, rectArea.x, rectArea.y, width, rectArea.height);
			SetRect(elements.Text, 0f, 0f, width, rectArea.height);
			SetRect(elements.Background, -BarBackgroundMargin, barY - BarBackgroundMargin, width + BarBackgroundMarginDouble, backgroundHeight);
			SetRect(elements.BarArea, 0f, barY, width, BarHeight);
			SetRect(elements.MinText, 0f, barY - BarBackgroundMargin + backgroundHeight, width, backgroundHeight);
			SetRect(elements.MaxText, 0f, barY - BarBackgroundMargin + backgroundHeight, width, backgroundHeight);

			// A clamped bar is cut at the edges of the bar area.
			elements.BarArea.style.overflow = data.clamp ? Overflow.Hidden : Overflow.Visible;

			// Texts
			if (!ReferenceEquals(elements.ShownText, data.text) || elements.ShownValue != data.value)
			{
				elements.ShownText = data.text;
				elements.ShownValue = data.value;
				elements.Text.text = string.IsNullOrEmpty(data.text)
					? data.value.ToString("0.0000")
					: data.text + ": " + data.value.ToString("0.0000");
			}
			if (elements.ShownMinValue != data.minValue)
			{
				elements.ShownMinValue = data.minValue;
				elements.MinText.text = data.minValue.ToString("0.0000");
			}
			if (elements.ShownMaxValue != data.maxValue)
			{
				elements.ShownMaxValue = data.maxValue;
				elements.MaxText.text = data.maxValue.ToString("0.0000");
			}

			// Colors. The bar is shown with its active colors in the frames it gets updated, and dimmer otherwise.
			var barColor = data.CurrentBarColor;
			var backgroundColor = data.CurrentBackgroundColor;
			if (elements.ShownBarColor != barColor)
			{
				elements.ShownBarColor = barColor;
				elements.Bar.style.backgroundColor = ToPanelColor(barColor);
				elements.ShadowedBar.style.backgroundColor = ToPanelColor(barColor * BarShadowedValueTint);
			}
			if (elements.ShownBackgroundColor != backgroundColor)
			{
				elements.ShownBackgroundColor = backgroundColor;
				elements.Background.style.backgroundColor = ToPanelColor(backgroundColor);
			}

			// Bar
			var barStart = CalculateBarPosition(width, data.centerValue, data);
			var barEnd = CalculateBarPosition(width, data.value, data);
			SetHorizontalSpan(elements.Bar, barStart, barEnd);
			if (float.IsNaN(data.shadowedValue))
			{
				elements.ShadowedBar.style.display = DisplayStyle.None;
			}
			else
			{
				elements.ShadowedBar.style.display = DisplayStyle.Flex;
				SetHorizontalSpan(elements.ShadowedBar, barStart, CalculateBarPosition(width, data.shadowedValue, data));
			}

			if (data.duration == 0f)
			{
				data.duration = -1f; // Mark data to be removed in next update
			}
		}

		private static int CalculateBarPosition(float width, float value, BarData data)
		{
			var position = width * ((value - data.minValue) / (data.maxValue - data.minValue));
			// A bar without a range (e.g. a shadowed value set on a line that has no bar) has nothing to show.
			return float.IsFinite(position) ? (int)position : 0;
		}

		private static Rect CalculateBarArea(BarData data)
		{
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

			return rectArea;
		}

#endif

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
#if UNITY_DRAWER
			public Label label;
#endif

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

#if UNITY_DRAWER
			Instance.StartDrawing();
#endif
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

#if UNITY_DRAWER

		private void RemoveExpiredWriteScreen(float currentTime)
		{
			for (int i = writeScreenData.Count - 1; i >= 0; i--)
			{
				var data = writeScreenData[i];

				if (data.startTime + data.duration < currentTime)
				{
					data.label?.RemoveFromHierarchy();
					data.label = null;
					writeScreenData.RemoveAt(i);
				}
			}
		}

		private void RefreshWriteScreen()
		{
			for (int i = 0; i < writeScreenData.Count; i++)
			{
				var data = writeScreenData[i];
				var label = data.label;
				if (label == null)
				{
					label = CreateLabel("DebugDraw_WriteScreen", DefaultTextFontSize);
					// Long texts wrap and get cut at the bottom of the text area.
					label.style.whiteSpace = WhiteSpace.Normal;
					label.style.overflow = Overflow.Hidden;
					label.style.unityTextAlign = TextAnchor.UpperLeft;
					overlayRoot.Add(label);
					data.label = label;
				}

				SetRect(label, data.rect.x, data.rect.y, data.rect.width, data.rect.height);
				label.text = data.text;
				label.style.color = ToPanelColor(data.color * TextColorTint);
			}
		}

#endif

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
#if UNITY_DRAWER
			public Label label;
#endif

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

#if UNITY_DRAWER
			Instance.StartDrawing();
#endif
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

#if UNITY_DRAWER

		private bool isMissingMainCameraLogged;

		private void RemoveExpiredWriteScene(float currentTime)
		{
			for (int i = writeSceneData.Count - 1; i >= 0; i--)
			{
				var data = writeSceneData[i];

				if (data.startTime + data.duration < currentTime)
				{
					data.label?.RemoveFromHierarchy();
					data.label = null;
					writeSceneData.RemoveAt(i);
				}
			}
		}

		private void RefreshWriteScene()
		{
			if (writeSceneData.Count == 0)
				return;

			var camera = Camera.main;
			if (!camera)
			{
				// Operational: scenes may go without a main camera for a while, e.g. while loading.
				if (!isMissingMainCameraLogged)
				{
					isMissingMainCameraLogged = true;
					Log.With(nameof(DebugDraw)).Warning($"{nameof(DebugDraw)} cannot place the texts written to the scene because there is no main camera. They are hidden until a camera tagged MainCamera is available.");
				}
				for (int i = 0; i < writeSceneData.Count; i++)
				{
					if (writeSceneData[i].label != null)
						writeSceneData[i].label.style.display = DisplayStyle.None;
				}
				return;
			}
			isMissingMainCameraLogged = false;

			for (int i = 0; i < writeSceneData.Count; i++)
			{
				var data = writeSceneData[i];
				var label = data.label;
				if (label == null)
				{
					label = CreateLabel("DebugDraw_WriteScene", DefaultTextFontSize);
					label.style.whiteSpace = WhiteSpace.NoWrap;
					// Centered on the position.
					label.style.translate = new Translate(Length.Percent(-50f), Length.Percent(-50f));
					overlayRoot.Add(label);
					data.label = label;
				}

				var screenPosition = camera.WorldToScreenPoint(data.position);
				if (screenPosition.z < 0f)
				{
					// Behind the camera.
					label.style.display = DisplayStyle.None;
					continue;
				}

				label.style.display = DisplayStyle.Flex;
				label.style.left = screenPosition.x;
				label.style.top = Screen.height - screenPosition.y - data.lineIndex * writeSceneTextAreaHeight;
				label.text = data.text;
				label.style.color = ToPanelColor(data.color * TextColorTint);
			}
		}

#endif

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
