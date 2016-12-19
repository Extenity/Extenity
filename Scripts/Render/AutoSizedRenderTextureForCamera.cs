using UnityEngine;

namespace Extenity.Rendering
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

		public void RefreshRenderTexture()
		{
			if (Camera == null)
				return;

			var currentTexture = Camera.targetTexture;
			if (currentTexture == null ||
				currentTexture.width != (int)(Camera.pixelWidth * SizeFactor.x) ||
				currentTexture.height != (int)(Camera.pixelHeight * SizeFactor.y) ||
				currentTexture.depth != Depth ||
				currentTexture.format != Format
			)
			{
				var newTexture = new RenderTexture((int)(Camera.pixelWidth * SizeFactor.x), (int)(Camera.pixelHeight * SizeFactor.y), Depth, Format, ReadWrite);
				Camera.targetTexture = newTexture;
			}
		}

		#endregion

		#region Editor

		protected void OnValidate()
		{
			// This will make sure the RenderTexture always created in editor mode. Ideally it's not necessary and not a good practice to create RenderTexture in Awake because camera settings could have been changed before we start rendering.
			RefreshRenderTexture();
		}

		#endregion
	}

}
