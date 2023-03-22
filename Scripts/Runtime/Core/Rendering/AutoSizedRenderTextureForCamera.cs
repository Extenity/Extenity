#if UNITY

using UnityEngine;

namespace Extenity.RenderingToolbox
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(Camera))]
	public class AutoSizedRenderTextureForCamera : MonoBehaviour
	{
		#region Configuration

		public int Depth = 24;
		public RenderTextureFormat Format = RenderTextureFormat.Default;
		public RenderTextureReadWrite ReadWrite = RenderTextureReadWrite.Default;
		public Vector2 SizeFactor = Vector2.one;

		#endregion

		#region Camera

		private Camera _Camera;
		public Camera Camera
		{
			get
			{
				if (_Camera == null)
					_Camera = GetComponent<Camera>();
				return _Camera;
			}
		}

		#endregion

		#region Render Events

		protected void OnPreRender()
		{
			RefreshRenderTexture();
		}

		#endregion

		#region Render Texture

		private int CurrentTextureWidth = -1;
		private int CurrentTextureHeight = -1;
		private int CurrentTextureDepth = -1;
		private RenderTextureFormat CurrentTextureFormat = (RenderTextureFormat)(-1);
		private RenderTextureReadWrite CurrentTextureReadWrite = (RenderTextureReadWrite)(-1);

		public void RefreshRenderTexture()
		{
			if (Camera == null)
				return;

			var sizeX = (int)(Screen.width * SizeFactor.x);
			var sizeY = (int)(Screen.height * SizeFactor.y);
			var currentTexture = Camera.targetTexture;
			if (currentTexture != null)
			{
				// Old method to check changes
				//if (currentTexture.width == sizeX &&
				//    currentTexture.height == sizeY &&
				//    currentTexture.depth == Depth &&
				//    (
				//		Format == RenderTextureFormat.Default || // Ignore format checking if the format set to Default. We lose format checking when we set Format as Default. But currently there is no easy way to ask Unity what the current platform's default render texture format is.
				//		currentTexture.format == Format
				//	)
				//)

				if (CurrentTextureWidth == sizeX &&
					CurrentTextureHeight == sizeY &&
					CurrentTextureDepth == Depth &&
					CurrentTextureFormat == Format &&
					CurrentTextureReadWrite == ReadWrite
				)
				{
					return; // No changes needed.
				}
				else
				{
					Log.Verbose($"Changing render texture. Old: {CurrentTextureWidth}x{CurrentTextureHeight} {CurrentTextureDepth} {CurrentTextureFormat} {CurrentTextureReadWrite} New: {sizeX}x{sizeY} {Depth} {Format} {ReadWrite}");
				}
			}
			else
			{
				Log.Verbose($"Creating new render texture: {sizeX}x{sizeY} {Depth} {Format} {ReadWrite}");
			}

			CurrentTextureWidth = sizeX;
			CurrentTextureHeight = sizeY;
			CurrentTextureDepth = Depth;
			CurrentTextureFormat = Format;
			CurrentTextureReadWrite = ReadWrite;
			var newTexture = new RenderTexture(sizeX, sizeY, Depth, Format, ReadWrite);
			Camera.targetTexture = newTexture;
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		protected void OnValidate()
		{
			// This will make sure the RenderTexture always created in editor mode. Ideally it's not necessary and not a good practice to create RenderTexture in Awake because camera settings could have been changed before we start rendering.
			RefreshRenderTexture();
		}

#endif

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(AutoSizedRenderTextureForCamera));

		#endregion
	}

}

#endif
