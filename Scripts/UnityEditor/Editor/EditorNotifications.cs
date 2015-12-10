using System;
using UnityEditor;
using UnityEngine;
using System.Collections;

[InitializeOnLoad]
public class EditorNotifications : Editor
{
	#region Initialization

	private static EditorNotifications instance;

	private EditorNotifications()
	{
		if (IsCreated)
			return; // One instance at a time is enough

		//Debug.Log("Creating EditorNotifications");
		instance = this;
		RegisterCustomUpdate();
	}

	private void OnDisable()
	{
		if (!IsCreated)
			return; // Nothing to destroy

		//Debug.Log("Destroying EditorNotifications");
		instance = null;
		DeregisterCustomUpdate();
	}

	protected static void CreateInstanceIfNecessary()
	{
		if (IsCreated)
			return;

		CreateInstance<EditorNotifications>();
	}

	protected static bool IsCreated
	{
		get { return instance != null; }
	}

	#endregion

	#region Update

	private delegate void CallbackCustomUpdate();

	private void CustomUpdate()
	{
		CheckCacheServerState();
	}

	private void RegisterCustomUpdate()
	{
		if (IsCustomUpdateRegistered)
			return;

		EditorApplication.update += CustomUpdate;
	}

	protected bool IsCustomUpdateRegistered
	{
		get
		{
			if (EditorApplication.update == null)
				return false;

			CallbackCustomUpdate del = CustomUpdate;

			foreach (var existingHandler in EditorApplication.update.GetInvocationList())
			{
				if (existingHandler == del)
					return true;
			}
			return false;
		}
	}

	private void DeregisterCustomUpdate()
	{
		EditorApplication.update -= CustomUpdate;
	}

	#endregion

	#region Cache Server Notification

	public delegate void CallbackOnChangeCacheServerEnabled(bool cacheServerEnabled);
	private event CallbackOnChangeCacheServerEnabled OnChangeCacheServerEnabled;

	private bool cacheServerEnabled;
	private double lastCacheServerCheckTime;
	private double cacheServerCheckPeriod = 1;

	private void CheckCacheServerState(bool dontRequireCallback = false)
	{
		if (!dontRequireCallback && OnChangeCacheServerEnabled == null)
			return;

		var now = EditorApplication.timeSinceStartup;
		if (lastCacheServerCheckTime + cacheServerCheckPeriod > now)
			return;
		lastCacheServerCheckTime = now;

		var cacheServerEnabledNew = EditorPrefs.GetBool("CacheServerEnabled", false);

		if (cacheServerEnabled != cacheServerEnabledNew)
		{
			cacheServerEnabled = cacheServerEnabledNew;
			if (OnChangeCacheServerEnabled != null)
				OnChangeCacheServerEnabled(cacheServerEnabled);
		}
	}

	public static void RegisterCacheServerEnabledChange(CallbackOnChangeCacheServerEnabled onChangeCacheServerEnabled)
	{
		CreateInstanceIfNecessary();

		if (IsRegisteredCacheServerEnabledChange(onChangeCacheServerEnabled))
			return;

		instance.OnChangeCacheServerEnabled += onChangeCacheServerEnabled;
	}

	public static void DeregisterCacheServerEnabledChange(CallbackOnChangeCacheServerEnabled onChangeCacheServerEnabled)
	{
		if (instance == null)
			return;

		instance.OnChangeCacheServerEnabled -= onChangeCacheServerEnabled;
	}

	public static bool IsRegisteredCacheServerEnabledChange(CallbackOnChangeCacheServerEnabled onChangeCacheServerEnabled)
	{
		if (instance == null)
			return false;
		if (instance.OnChangeCacheServerEnabled == null)
			return false;

		foreach (var existingHandler in instance.OnChangeCacheServerEnabled.GetInvocationList())
		{
			if (existingHandler == onChangeCacheServerEnabled)
				return true;
		}
		return false;
	}

	public static bool CacheServerEnabled
	{
		get
		{
			CreateInstanceIfNecessary();
			instance.CheckCacheServerState(true);
			return instance.cacheServerEnabled;
		}
	}

	#endregion
}
