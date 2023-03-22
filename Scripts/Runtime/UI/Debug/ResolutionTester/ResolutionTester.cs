using TMPro;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class ResolutionTester : MonoBehaviour
	{
		private bool IsInitialized = false;
		private Vector2Int DefaultResolution;

		public TextMeshProUGUI ResolutionText;

		private void Start()
		{
			RefreshResolutionText();
		}

		private void Initialize()
		{
			if (IsInitialized)
				return;
			IsInitialized = true;

			DefaultResolution = new Vector2Int(Screen.width, Screen.height);
		}

		public void Downsample()
		{
			Initialize();
			var resolution = new Vector2Int(Screen.width / 2, Screen.height / 2);
			ChangeResolution(resolution);
		}

		public void Upsample()
		{
			Initialize();
			var resolution = new Vector2Int(Screen.width * 2, Screen.height * 2);
			if (resolution.x > DefaultResolution.x || resolution.y > DefaultResolution.y)
			{
				resolution.x = DefaultResolution.x;
				resolution.y = DefaultResolution.y;
				Log.Info($"Resolution clamped to '{resolution.x}x{resolution.y}'");
			}

			ChangeResolution(resolution);
		}

		private void ChangeResolution(Vector2Int resolution)
		{
			Screen.SetResolution(resolution.x, resolution.y, true);

			var resultingResolution = new Vector2Int(Screen.width, Screen.height);
			if (resultingResolution.x != resolution.x || resultingResolution.y != resolution.y)
			{
				Log.Warning($"Tried to set resolution to '{resolution.x}x{resolution.y}' but the resulting resolution is '{resultingResolution.x}x{resultingResolution.y}'.");
			}
			else
			{
				Log.Warning($"Resolution set to '{resolution.x}x{resolution.y}'");
			}

			RefreshResolutionText();
		}

		private void RefreshResolutionText()
		{
			if (ResolutionText)
				ResolutionText.text = $"{Screen.width}x{Screen.height}";
		}

		#region Log

		private static readonly Logger Log = new(nameof(ResolutionTester));

		#endregion
	}

}
