#if UNITY

using UnityEngine;
using UnityEngine.Events;

// TODO: Keeping a reference to PreviouslyInvokedRenderTexture is bad for garbage collection. Change the way it works. Use OnValidate to detect changed values as needed.

namespace Extenity.RenderingToolbox
{

	public class AutoSizedRenderTexture : MonoBehaviour
	{
		#region Configuration

		public bool AutoResizeEnabled = true;
		public Vector2 ScreenSizeFactor = Vector2.one;
		public int Depth = 24;
		public RenderTextureFormat Format = RenderTextureFormat.Default;
		public RenderTextureReadWrite ReadWrite = RenderTextureReadWrite.Default;
		public RenderTexture RenderTexture;
		private RenderTexture PreviouslyInvokedRenderTexture;

		public int Width
		{
			get { return (int)(Screen.width * ScreenSizeFactor.x); }
		}

		public int Height
		{
			get { return (int)(Screen.height * ScreenSizeFactor.y); }
		}

		#endregion

		#region Events

		public class RenderTextureChangedEvent : UnityEvent<RenderTexture> { }
		public RenderTextureChangedEvent OnRenderTextureChanged = new RenderTextureChangedEvent();

		private void InvokeChangedEvent()
		{
			OnRenderTextureChanged.Invoke(RenderTexture);
			PreviouslyInvokedRenderTexture = RenderTexture;
		}

		private void InvokeChangedEventIfRenderTextureChangedFromField()
		{
			if (PreviouslyInvokedRenderTexture != RenderTexture)
			{
				OnRenderTextureChanged.Invoke(RenderTexture);
				InvokeChangedEvent();
			}
		}

		#endregion

		#region Create Auto Sized Render Texture

		public static AutoSizedRenderTexture Create(
			int depth = 24,
			RenderTextureFormat format = RenderTextureFormat.Default,
			RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default,
			bool hideInHierarchy = true)
		{
			return Create(null, Vector2.one, depth, format, readWrite, hideInHierarchy);
		}

		public static AutoSizedRenderTexture Create(
			UnityAction<RenderTexture> onRenderTextureChanged,
			int depth = 24,
			RenderTextureFormat format = RenderTextureFormat.Default,
			RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default,
			bool hideInHierarchy = true)
		{
			return Create(onRenderTextureChanged, Vector2.one, depth, format, readWrite, hideInHierarchy);
		}

		public static AutoSizedRenderTexture Create(UnityAction<RenderTexture> onRenderTextureChanged, Vector2 screenSizeFactor, int depth = 24, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, bool hideInHierarchy = true)
		{
			var me = CreateGameObject(hideInHierarchy);
			me.ScreenSizeFactor = screenSizeFactor;
			me.Depth = depth;
			me.Format = format;
			me.ReadWrite = readWrite;

			if (onRenderTextureChanged != null)
			{
				me.OnRenderTextureChanged.AddListener(onRenderTextureChanged);
			}

			me.CreateRenderTexture();
			return me;
		}

		private static AutoSizedRenderTexture CreateGameObject(bool hideInHierarchy)
		{
			var go = new GameObject();
			go.name = "_AutoSizedRenderTexture-" + go.GetInstanceID();

			if (hideInHierarchy)
			{
				go.hideFlags = HideFlags.HideInHierarchy;
			}

			return go.AddComponent<AutoSizedRenderTexture>();
		}

		#endregion

		#region Create Render Texture

		private void CreateRenderTexture()
		{
			RenderTexture = CreateRenderTexture(Width, Height, Depth, Format, ReadWrite);
			InvokeChangedEvent();
		}

		private static RenderTexture CreateRenderTexture(int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite)
		{
			Log.Verbose($"Creating render texture. Resolution: {width}x{height} Depth: {depth} Format: {format} ReadWrite: {readWrite}");
			return new RenderTexture(width, height, depth, format, readWrite);
		}

		#endregion

		#region Update

		private void Update()
		{
			if (AutoResizeEnabled)
			{
				CalculateResize();
				InvokeChangedEventIfRenderTextureChangedFromField();
			}
			else
			{
				InvokeChangedEventIfRenderTextureChangedFromField();
			}
		}

		#endregion

		#region Resize

		private void CalculateResize()
		{
			if (RenderTexture == null ||
				RenderTexture.width != Width ||
				RenderTexture.height != Height ||
				RenderTexture.depth != Depth ||
				(Format != RenderTextureFormat.Default && RenderTexture.format != Format))
			{
				CreateRenderTexture();
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(AutoSizedRenderTexture));

		#endregion
	}

}

#endif
