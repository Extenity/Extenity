using UnityEngine;

namespace Extenity.Rendering
{

	[RequireComponent(typeof(Camera))]
	public class CameraConfigurationSynchronizer : MonoBehaviour
	{
		#region Configuration

		public Camera FromCamera;

		public bool SyncClearFlags = false;
		public bool SyncClearStencilAfterLightingPass = false;
		public bool SyncBackgroundColor = false;
		public bool SyncCullingMask = false;
		public bool SyncCullingMatrix = false;
		public bool SyncProjectionMatrix = false;
		public bool SyncNonJitteredProjectionMatrix = false;
		public bool SyncOrthographic = true;
		public bool SyncOrthographicSize = true;
		public bool SyncFieldOfView = true;
		public bool SyncAspect = false;
		public bool SyncNearClipPlane = true;
		public bool SyncFarClipPlane = true;
		public bool SyncViewportRect = true;
		public bool SyncDepth = false;
		public bool SyncRenderingPath = false;
		public bool SyncTargetTexture = false;
		public bool SyncUseOcclusionCulling = true;
		public bool SyncHDR = true;
		public bool SyncTargetDisplay = false;

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

		protected void OnPreCull()
		{
			if (Camera == null)
				return;

			SyncConfiguration();
		}

		#endregion

		#region Sync

		public void SyncConfiguration()
		{
			if (FromCamera == null)
				return;

			if (SyncClearFlags)
				Camera.clearFlags = FromCamera.clearFlags;
			if (SyncClearStencilAfterLightingPass)
				Camera.clearStencilAfterLightingPass = FromCamera.clearStencilAfterLightingPass;
			if (SyncBackgroundColor)
				Camera.backgroundColor = FromCamera.backgroundColor;
			if (SyncCullingMask)
				Camera.cullingMask = FromCamera.cullingMask;
			if (SyncCullingMatrix)
				Camera.cullingMatrix = FromCamera.cullingMatrix;
			if (SyncProjectionMatrix)
				Camera.projectionMatrix = FromCamera.projectionMatrix;
			if (SyncNonJitteredProjectionMatrix)
				Camera.nonJitteredProjectionMatrix = FromCamera.nonJitteredProjectionMatrix;
			if (SyncOrthographic)
				Camera.orthographic = FromCamera.orthographic;
			if (SyncOrthographicSize)
				Camera.orthographicSize = FromCamera.orthographicSize;
			if (SyncFieldOfView)
				Camera.fieldOfView = FromCamera.fieldOfView;
			if (SyncAspect)
				Camera.aspect = FromCamera.aspect;
			if (SyncNearClipPlane)
				Camera.nearClipPlane = FromCamera.nearClipPlane;
			if (SyncFarClipPlane)
				Camera.farClipPlane = FromCamera.farClipPlane;
			if (SyncViewportRect)
				Camera.pixelRect = FromCamera.pixelRect;
			if (SyncDepth)
				Camera.depth = FromCamera.depth;
			if (SyncRenderingPath)
				Camera.renderingPath = FromCamera.renderingPath;
			if (SyncTargetTexture)
				Camera.targetTexture = FromCamera.targetTexture;
			if (SyncUseOcclusionCulling)
				Camera.useOcclusionCulling = FromCamera.useOcclusionCulling;
			if (SyncHDR)
				Camera.hdr = FromCamera.hdr;
			if (SyncTargetDisplay)
				Camera.targetDisplay = FromCamera.targetDisplay;
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		protected void Awake()
		{
			if (!Application.isPlaying)
			{
				UnityEditor.EditorApplication.update += Update;

				SyncConfiguration();
			}
		}

		protected void OnDestroy()
		{
			UnityEditor.EditorApplication.update -= Update;
		}

		protected void OnEnable()
		{
			if (!Application.isPlaying)
			{
				SyncConfiguration();
			}
		}

		protected void OnDisable()
		{
			if (!Application.isPlaying)
			{
				SyncConfiguration();
			}
		}

		protected void Update()
		{
			if (!Application.isPlaying)
			{
				SyncConfiguration();
			}
		}

		protected void OnValidate()
		{
			SyncConfiguration();
		}

#endif

		#endregion
	}

}
