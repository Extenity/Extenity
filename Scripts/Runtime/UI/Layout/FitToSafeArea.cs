using Extenity.ScreenToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Source: https://forum.unity.com/threads/canvashelper-resizes-a-recttransform-to-iphone-xs-safe-area.521107/
	/// </summary>
	// [ExecuteInEditMode] // See 11653662.
	public class FitToSafeArea : MonoBehaviour
	{
		[Required]
		public Canvas ContainerCanvas;
		[Required]
		public RectTransform PanelThatFitsIntoSafeAreaOfCanvas;

		protected void OnEnable()
		{
			ScreenTracker.OnScreenModified.AddListener(OnScreenModified);
			OnScreenModified(default);
		}

		protected void OnDisable()
		{
			ScreenTracker.OnScreenModified.RemoveListener(OnScreenModified);
		}

		private void OnScreenModified(ScreenInfo info)
		{
			ScreenTools.ApplySafeArea(ContainerCanvas, PanelThatFitsIntoSafeAreaOfCanvas);
		}

		#region Auto Find Components

#if UNITY_EDITOR

		protected void OnValidate()
		{
			if (ContainerCanvas == null)
			{
				ContainerCanvas = GetComponentInParent<Canvas>();
			}
			if (PanelThatFitsIntoSafeAreaOfCanvas == null)
			{
				PanelThatFitsIntoSafeAreaOfCanvas = GetComponent<RectTransform>();
			}
		}

#endif

		#endregion
	}

}
