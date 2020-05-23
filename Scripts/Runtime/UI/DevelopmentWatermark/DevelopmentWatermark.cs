using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class DevelopmentWatermark : MonoBehaviour
	{
#if UNITY_EDITOR
		private static GameObject Instance;

		private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
		{
			if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
			{
				UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
				if (Instance)
				{
					Destroy(Instance);
					Instance = null;
				}
			}
		}
#endif

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			var go = new GameObject("DevelopmentWatermarkCanvas", typeof(Canvas), typeof(CanvasScaler));
			go.hideFlags = HideFlags.HideAndDontSave;
			var canvas = go.GetComponent<Canvas>();
			var canvasScaler = go.GetComponent<CanvasScaler>();

			canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingOrder = 32767;
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			canvasScaler.matchWidthOrHeight = 0.5f;
			canvasScaler.referenceResolution = new Vector2(800, 600);

			var imageGO = new GameObject("DevelopmentWatermarkImage", typeof(CanvasRenderer), typeof(Image));
			imageGO.hideFlags = HideFlags.HideAndDontSave;
			var imageTransform = (RectTransform)imageGO.transform;
			imageTransform.SetParent(go.transform, false);
			var image = imageGO.GetComponent<Image>();

			imageTransform.localScale = Vector3.one;
			imageTransform.pivot = new Vector2(0f, 0f);
			imageTransform.anchorMin = new Vector2(0f, 0f);
			imageTransform.anchorMax = new Vector2(0f, 0f);
			imageTransform.anchoredPosition = new Vector2(0f, 0f);
			imageTransform.sizeDelta = new Vector2(5f, 5f);
			image.color = new Color(1f, 1f, 1f, 0.2f);
			image.raycastTarget = false;

#if UNITY_EDITOR
			Instance = go;
			UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
#endif
		}
	}

}
