#if UNITY_5_3_OR_NEWER

//using AdvancedInspector;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Extenity.RenderingToolbox
{

	[RequireComponent(typeof(Camera))]
	public class CameraConfigurationSynchronizer : MonoBehaviour
	{
		#region Configuration

		//[Group("Data Source", Expandable = false, Priority = 10)]
		public Camera FromCamera;

		//[Group("Configuration", Expandable = true, Priority = 20)]
		public bool SyncClearFlags = false;
		//[Group("Configuration")]
		public bool SyncClearStencilAfterLightingPass = false;
		//[Group("Configuration")]
		public bool SyncBackgroundColor = false;
		//[Group("Configuration")]
		public bool SyncCullingMask = false;
		//[Group("Configuration")]
		public bool SyncCullingMatrix = false;
		//[Group("Configuration")]
		public bool SyncProjectionMatrix = false;
		//[Group("Configuration")]
		public bool SyncNonJitteredProjectionMatrix = false;
		//[Group("Configuration")]
		public bool SyncOrthographic = true;
		//[Group("Configuration")]
		public bool SyncOrthographicSize = true;
		//[Group("Configuration")]
		public bool SyncFieldOfView = true;
		//[Group("Configuration")]
		public bool SyncAspect = false;
		//[Group("Configuration")]
		public bool SyncNearClipPlane = true;
		//[Group("Configuration")]
		public bool SyncFarClipPlane = true;
		//[Group("Configuration")]
		public bool SyncViewportRect = true;
		//[Group("Configuration")]
		public bool SyncDepth = false;
		//[Group("Configuration")]
		public bool SyncRenderingPath = false;
		//[Group("Configuration")]
		public bool SyncTargetTexture = false;
		//[Group("Configuration")]
		public bool SyncUseOcclusionCulling = true;
		//[Group("Configuration")]
		public bool SyncHDR = true;
		//[Group("Configuration")]
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
				Camera.allowHDR = FromCamera.allowHDR;
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
				EditorApplication.update += Update;

				SyncConfiguration();
			}
		}

		protected void OnDestroy()
		{
			EditorApplication.update -= Update;
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

#endif
