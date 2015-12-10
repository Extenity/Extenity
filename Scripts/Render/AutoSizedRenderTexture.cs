using System;
using UnityEngine;
using System.Collections;
using Extenity.Logging;
using UnityEngine.Events;

public class AutoSizedRenderTexture : MonoBehaviour
{
	#region Configuration

	public bool AutoResizeEnabled = true;
	public Vector2 ScreenSizeFactor = Vector2.one;
	public int Depth = 24;
	public RenderTextureFormat Format = RenderTextureFormat.Default;
	public RenderTextureReadWrite ReadWrite = RenderTextureReadWrite.Default;
	public RenderTexture RenderTexture;
	private RenderTexture PreviouslyInvokedRenderTexture;

	public int Width
	{
		get { return (int)(Screen.width * ScreenSizeFactor.x); }
	}

	public int Height
	{
		get { return (int)(Screen.height * ScreenSizeFactor.y); }
	}

	#endregion

	#region Events

	public class RenderTextureChangedEvent : UnityEvent<RenderTexture> { }
	public RenderTextureChangedEvent OnRenderTextureChanged = new RenderTextureChangedEvent();

	private void InvokeChangedEvent()
	{
		OnRenderTextureChanged.Invoke(RenderTexture);
		PreviouslyInvokedRenderTexture = RenderTexture;
	}

	private void InvokeChangedEventIfRenderTextureChangedFromField()
	{
		if (PreviouslyInvokedRenderTexture != RenderTexture)
		{
			OnRenderTextureChanged.Invoke(RenderTexture);
			InvokeChangedEvent();
		}
	}

	#endregion

	#region Create Auto Sized Render Texture

	public static AutoSizedRenderTexture Create(
		int depth = 24,
		RenderTextureFormat format = RenderTextureFormat.Default,
		RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default,
		bool hideInHierarcy = true)
	{
		return Create(null, Vector2.one, depth, format, readWrite, hideInHierarcy);
	}

	public static AutoSizedRenderTexture Create(
		UnityAction<RenderTexture> onRenderTextureChanged,
		int depth = 24,
		RenderTextureFormat format = RenderTextureFormat.Default,
		RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default,
		bool hideInHierarcy = true)
	{
		return Create(onRenderTextureChanged, Vector2.one, depth, format, readWrite, hideInHierarcy);
	}

	public static AutoSizedRenderTexture Create(UnityAction<RenderTexture> onRenderTextureChanged, Vector2 screenSizeFactor, int depth = 24, RenderTextureFormat format = RenderTextureFormat.Default, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, bool hideInHierarcy = true)
	{
		var me = CreateGameObject(hideInHierarcy);
		me.ScreenSizeFactor = screenSizeFactor;
		me.Depth = depth;
		me.Format = format;
		me.ReadWrite = readWrite;

		if (onRenderTextureChanged != null)
		{
			me.OnRenderTextureChanged.AddListener(onRenderTextureChanged);
		}

		me.CreateRenderTexture();
		return me;
	}

	private static AutoSizedRenderTexture CreateGameObject(bool hideInHierarcy)
	{
		var go = new GameObject();
		go.name = "_AutoSizedRenderTexture-" + go.GetInstanceID();

		if (hideInHierarcy)
		{
			go.hideFlags = HideFlags.HideInHierarchy;
		}

		return go.AddComponent<AutoSizedRenderTexture>();
	}

	#endregion

	#region Create Render Texture

	private void CreateRenderTexture()
	{
		RenderTexture = CreateRenderTexture(Width, Height, Depth, Format, ReadWrite);
		InvokeChangedEvent();
	}

	private static RenderTexture CreateRenderTexture(
		int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite)
	{
		Logger.LogFormat("Creating render texture. Resolution: {0}x{1} Depth: {2} Format: {3} ReadWrite: {4}", width, height, depth, format, readWrite);
		return new RenderTexture(width, height, depth, format, readWrite);
	}

	#endregion

	#region Update

	private void Update()
	{
		if (AutoResizeEnabled)
		{
			CalculateResize();
			InvokeChangedEventIfRenderTextureChangedFromField();
		}
		else
		{
			InvokeChangedEventIfRenderTextureChangedFromField();
		}
	}

	#endregion

	#region Resize

	private void CalculateResize()
	{
		if (RenderTexture == null ||
			RenderTexture.width != Width ||
			RenderTexture.height != Height ||
			RenderTexture.depth != Depth ||
			(Format != RenderTextureFormat.Default && RenderTexture.format != Format))
		{
			CreateRenderTexture();
		}
	}

	#endregion
}
