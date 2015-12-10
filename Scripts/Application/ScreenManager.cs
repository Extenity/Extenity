using UnityEngine;
using System.Collections;

public class ScreenManager : SingletonUnity<ScreenManager>
{
	#region Configuration

	public float heightBaseInches = 2.31f; // iPhone 5 size

	#endregion

	#region Initialization

	private void Awake()
	{
		InitializeSingleton(this);

		Calculate();
	}

	#endregion

	#region Update

	private void Update()
	{
		if (currentScreenWidth == Screen.width && currentScreenHeight == Screen.height)
			return;

		Calculate();
		Broadcast.ToScene("OnScreenSizeChange", SendMessageOptions.DontRequireReceiver);
	}

	#endregion

	#region Calculations

	private void Calculate()
	{
		previousScreenWidth = currentScreenWidth;
		previousScreenHeight = currentScreenHeight;
		currentScreenWidth = Screen.width;
		currentScreenHeight = Screen.height;

		screenSizeInchesAvailable = !Screen.dpi.IsZero();

		if (screenSizeInchesAvailable)
		{
			screenWidthInches = (float) Screen.width/Screen.dpi;
			screenHeightInches = (float) Screen.height/Screen.dpi;
			screenDiagonalInches = Mathf.Sqrt(
				screenWidthInches*screenWidthInches +
				screenHeightInches*screenHeightInches);

			heightBasedSizeFactor = screenHeightInches/heightBaseInches;
			heightBasedSizeFactorInverted = 1f/heightBasedSizeFactor;
		}
		else
		{
			screenWidthInches = float.NaN;
			screenHeightInches = float.NaN;
			screenDiagonalInches = float.NaN;

			heightBasedSizeFactor = 1f;
			heightBasedSizeFactorInverted = 1f;
		}
	}

	#endregion

	#region Resolution Tools

	public void SwapResolution()
	{
		Screen.SetResolution(Screen.height, Screen.width, false);
	}

	#endregion

	#region Orientation Tools

	private ScreenOrientation previousOrientation;

	public void ChangeOrientation(ScreenOrientation orientation)
	{
		previousOrientation = Screen.orientation;
		Screen.orientation = orientation;
	}

	public void ChangeOrientationToPrevious()
	{
		ChangeOrientation(previousOrientation);
	}

	public ScreenOrientation PreviousOrientation
	{
		get { return previousOrientation; }
	}

	#endregion

	#region Data

	private int previousScreenWidth = int.MinValue;
	private int previousScreenHeight = int.MinValue;
	private int currentScreenWidth = int.MinValue;
	private int currentScreenHeight = int.MinValue;

	private float screenWidthInches = float.NaN;
	private float screenHeightInches = float.NaN;
	private float screenDiagonalInches = float.NaN;
	private bool screenSizeInchesAvailable = false;

	private float heightBasedSizeFactor = float.NaN;
	private float heightBasedSizeFactorInverted = float.NaN;

	public float CurrentAspectRatio
	{
		get { return (float)currentScreenHeight / currentScreenWidth; }
	}

	public int CurrentScreenWidth
	{
		get { return currentScreenWidth; }
	}

	public int CurrentScreenHeight
	{
		get { return currentScreenHeight; }
	}

	public int PreviousScreenWidth
	{
		get { return previousScreenWidth; }
	}

	public int PreviousScreenHeight
	{
		get { return previousScreenHeight; }
	}

	public float ScreenWidthInches
	{
		get { return screenWidthInches; }
	}

	public float ScreenHeightInches
	{
		get { return screenHeightInches; }
	}

	public float ScreenDiagonalInches
	{
		get { return screenDiagonalInches; }
	}

	public bool ScreenSizeInchesAvailable
	{
		get { return screenSizeInchesAvailable; }
	}

	public float HeightBasedSizeFactor
	{
		get { return heightBasedSizeFactor; }
	}

	public float HeightBasedSizeFactorInverted
	{
		get { return heightBasedSizeFactorInverted; }
	}

	#endregion
}
