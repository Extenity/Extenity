using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.UIToolbox
{

	[RequireComponent(typeof(Canvas))]
	public class CanvasRegistrar : MonoBehaviour
	{
		#region Canvas Link

		[SerializeField]
		private Canvas Canvas;

		private void GetCanvasLink()
		{
			Canvas = GetComponent<Canvas>();
		}

		#endregion

		#region Initialization / Deinitialization

		protected void OnEnable()
		{
			CanvasManager.Register(Canvas);
			InitializeUICamera();
		}

		protected void OnDisable()
		{
			CanvasManager.Deregister(Canvas);
		}

		#endregion

		#region Auto Assign UICamera

		[Header("UI Camera")]
		public bool SetUICameraAutomatically = true;

		private void InitializeUICamera()
		{
			if (SetUICameraAutomatically)
			{
				var uiCamera = UICamera.Instance;
				if (uiCamera)
				{
					Canvas.renderMode = RenderMode.ScreenSpaceCamera;
					Canvas.worldCamera = uiCamera.Camera;
				}
				else
				{
					Debug.LogError($"{nameof(UICamera)} was not initialized yet while initializing camera '{gameObject.FullName()}'.");
				}
			}
		}

		#endregion

		#region Editor

		protected void OnValidate()
		{
			GetCanvasLink();
		}

		#endregion
	}

}
