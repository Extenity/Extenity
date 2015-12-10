using Extenity.Logging;
using UnityEngine;

public class VideoInputDevice : MonoBehaviour
{
	public string preferredDeviceName;
	public bool selectFirstDeviceIfPreferredNameNotFound = true;
	public Material updateMaterial;
	public bool displayOnScreen = true;
	public GUIAnchor displayOnScreenGUIAnchor = GUIAnchor.RightTop;
	public int displayOnScreenMarginX = 4;
	public int displayOnScreenMarginY = 4;
	public float displayOnScreenWholeScreenHeightRatio = 0.35f;
	public bool autoSelectOnStart = true;
	public float autoSelectOnStartDelay = 3f;
	public bool autoStartCaptureOnSelect = true;
	public float autoStartCaptureOnSelectDelay = 3f;

	private WebCamDevice selectedDevice;
	private WebCamTexture videoInputTexture;

	void Start()
	{
		if (autoSelectOnStart)
		{
			Invoke("SelectDevice", autoSelectOnStartDelay);
		}
	}

	void OnDestroy()
	{
		DeselectDevice();
	}

	void OnGUI()
	{
		if (displayOnScreen && IsCapturing && IsTextureAvailable)
		{
			float ratio = (Screen.height * displayOnScreenWholeScreenHeightRatio) / (float)videoInputTexture.height;

			Rect rect = GUITools.Rect(
				(int)(videoInputTexture.width * ratio),
				(int)(videoInputTexture.height * ratio),
				displayOnScreenGUIAnchor,
				displayOnScreenMarginX,
				displayOnScreenMarginY);
			GUI.DrawTexture(rect, videoInputTexture, ScaleMode.ScaleToFit, false);
		}
	}

	public bool SelectDevice()
	{
		if (IsCapturing)
		{
			StopCapture();
		}

		selectedDevice = new WebCamDevice();
		videoInputTexture = null;

		var devices = WebCamTexture.devices;

		if (devices == null || devices.Length == 0)
		{
			Logger.Log("No video device found");
			return false;
		}

		Logger.Log("Listing video input devices:");
		foreach (var device in devices)
		{
			Logger.Log("   Name: " + device.name);

			if (device.name.ToLower().Contains(preferredDeviceName))
			{
				selectedDevice = device;
			}
		}

		if (string.IsNullOrEmpty(selectedDevice.name))
		{
			if (selectFirstDeviceIfPreferredNameNotFound)
			{
				selectedDevice = devices[0];
			}
			else
			{
				Logger.Log("No video device found");
				return false;
			}
		}

		videoInputTexture = new WebCamTexture(selectedDevice.name);
		if (updateMaterial != null)
		{
			updateMaterial.mainTexture = videoInputTexture;
		}

		if (autoStartCaptureOnSelect && IsDeviceSelected)
		{
			Invoke("StartCapture", autoStartCaptureOnSelectDelay);
		}

		return true;
	}

	public void DeselectDevice()
	{
		if (IsInvoking("SelectDevice"))
			CancelInvoke("SelectDevice");

		if (!IsDeviceSelected)
			return;

		StopCapture();

		if (updateMaterial == videoInputTexture)
		{
			updateMaterial.mainTexture = null;
		}
		videoInputTexture = null;
		selectedDevice = new WebCamDevice();
	}

	public void StartCapture()
	{
		if (IsCapturing)
			return;

		if (string.IsNullOrEmpty(selectedDevice.name))
		{
			Logger.Log("No video input device selected for capture");
			return;
		}

		videoInputTexture.Play();
	}

	public void StopCapture()
	{
		if (IsInvoking("StartCapture"))
			CancelInvoke("StartCapture");

		if (!IsCapturing)
			return;

		videoInputTexture.Stop();
	}

	private void OnAllVideoInputDevicesSelectDeselect(bool select)
	{
		if (select)
		{
			SelectDevice();
		}
		else
		{
			DeselectDevice();
		}
	}

	private void OnAllVideoInputDevicesStartStopCapture(bool start)
	{
		if (start)
		{
			StartCapture();
		}
		else
		{
			StopCapture();
		}
	}

	public bool IsDeviceSelected
	{
		get { return !string.IsNullOrEmpty(selectedDevice.name); }
	}

	public bool IsCapturing
	{
		get { return videoInputTexture != null && videoInputTexture.isPlaying; }
	}

	public Texture Texture
	{
		get { return videoInputTexture; }
	}

	public bool DidUpdateThisFrame
	{
		get
		{
			if (videoInputTexture == null || !videoInputTexture.isPlaying)
				return false;
			return videoInputTexture.didUpdateThisFrame;
		}
	}

	public bool IsTextureAvailable
	{
		get { return videoInputTexture != null && videoInputTexture.width > 0 && videoInputTexture.height > 0; }
	}
}
