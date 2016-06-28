using UnityEngine;

public class AssignMainCameraToCanvas : MonoBehaviour
{
	public RenderMode CanvasRenderMode = RenderMode.ScreenSpaceCamera;

	private void Start()
	{
		var canvas = GetComponent<Canvas>();
		canvas.renderMode = CanvasRenderMode;
		canvas.worldCamera = Camera.main;
	}
}
