using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class ExtenityEditorBase<T> : Editor where T : UnityEngine.Object
{
	#region Initialization

	protected abstract void OnEnableDerived();
	protected abstract void OnDisableDerived();

	protected void OnEnable()
	{
		Me = target as T;
		Configuration = new SerializedObject(target);

		OnEnableDerived();

		//RegisterUpdate();
	}

	protected void OnDisable()
	{
		DeregisterUpdate(true);

		OnDisableDerived();
	}

	#endregion

	#region Update

	protected virtual void Update() { }
	public int RequestedUpdateRegistrationCount { get; private set; }

	private void UpdateInternal()
	{
		if (Me == null)
			return;

		Update();
		UpdateAutoRepaint();
	}

	public void RegisterUpdate()
	{
		RequestedUpdateRegistrationCount++;
		if (RequestedUpdateRegistrationCount > 1)
			return;
		EditorApplication.update += UpdateInternal;
	}

	public void DeregisterUpdate(bool force = false)
	{
		RequestedUpdateRegistrationCount--;
		if (force || RequestedUpdateRegistrationCount <= 0)
		{
			EditorApplication.update -= UpdateInternal;
		}
	}

	#endregion

	#region Auto Repaint

	public float AutoRepaintSceneViewPeriod = 0.08f;
	private float LastRepaintSceneViewTime;
	private bool _IsAutoRepaintSceneViewEnabled;
	public bool IsAutoRepaintSceneViewEnabled
	{
		get { return _IsAutoRepaintSceneViewEnabled; }
		set
		{
			if (value == _IsAutoRepaintSceneViewEnabled)
				return;

			if (value)
				RegisterUpdate();
			else
				DeregisterUpdate();

			_IsAutoRepaintSceneViewEnabled = value;
		}
	}

	public float AutoRepaintInspectorPeriod = 0.08f;
	private float LastRepaintInspectorTime;
	private bool _IsAutoRepaintInspectorEnabled;
	public bool IsAutoRepaintInspectorEnabled
	{
		get { return _IsAutoRepaintInspectorEnabled; }
		set
		{
			if (value == _IsAutoRepaintInspectorEnabled)
				return;

			if (value)
				RegisterUpdate();
			else
				DeregisterUpdate();

			_IsAutoRepaintInspectorEnabled = value;
		}
	}

	private void UpdateAutoRepaint()
	{
		var currentTime = Time.realtimeSinceStartup;

		// SceneView
		if (LastRepaintSceneViewTime + AutoRepaintSceneViewPeriod < currentTime)
		{
			SceneView.RepaintAll();
			LastRepaintSceneViewTime = currentTime;
		}

		// Inspector
		if (LastRepaintInspectorTime + AutoRepaintInspectorPeriod < currentTime)
		{
			Repaint();
			LastRepaintInspectorTime = currentTime;
		}
	}

	//public override bool RequiresConstantRepaint() // This method is abandoned to provide RepaintPeriod functionality by using EditorApplication.update callback
	//{
	//	return base.RequiresConstantRepaint() || AutoRepaintEnabled;
	//}

	#endregion

	#region Configuration

	protected SerializedObject Configuration;
	protected T Me;

	#endregion

	#region Cached Properties

	public Dictionary<string, SerializedProperty> CachedSerializedProperties;

	public SerializedProperty GetProperty(string propertyName)
	{
		SerializedProperty serializedProperty;

		if (CachedSerializedProperties == null)
		{
			CachedSerializedProperties = new Dictionary<string, SerializedProperty>();
		}
		else
		{
			if (CachedSerializedProperties.TryGetValue(propertyName, out serializedProperty))
			{
				if (serializedProperty != null)
				{
					return serializedProperty;
				}
				else
				{
					CachedSerializedProperties.Remove(propertyName);
				}
			}
		}

		serializedProperty = Configuration.FindProperty(propertyName);
		if (serializedProperty != null)
		{
			CachedSerializedProperties.Add(propertyName, serializedProperty);
		}
		return serializedProperty;
	}

	#endregion

	#region Inspector GUI

	public bool IsDefaultInspectorDrawingEnabled = true;
	public bool IsInspectorDisabledWhenPlaying = false;

	private static readonly string[] ExcludedPropertiesInDefaultInspector = { "m_Script" };

	protected virtual void OnBeforeDefaultInspectorGUI() { }
	protected abstract void OnAfterDefaultInspectorGUI();

	public override sealed void OnInspectorGUI()
	{
		Configuration.Update();

		var disabled = IsInspectorDisabledWhenPlaying;
		if (disabled)
			EditorGUI.BeginDisabledGroup(Application.isPlaying);

		OnBeforeDefaultInspectorGUI();

		if (IsDefaultInspectorDrawingEnabled)
		{
			DrawPropertiesExcluding(serializedObject, ExcludedPropertiesInDefaultInspector);
		}

		OnAfterDefaultInspectorGUI();

		if (disabled)
			EditorGUI.EndDisabledGroup();

		Configuration.ApplyModifiedProperties();
	}

	#endregion

	#region Invalidate Modified Properties

	protected void InvalidateModifiedProperties()
	{
		EditorUtility.SetDirty(Me);
	}

	#endregion

	#region Mouse

	protected static bool IsMouseCloseToScreenPoint(Vector2 mousePosition, Vector3 screenPosition, float maximumDistanceFromMouse)
	{
		int diffX = (int)(mousePosition.x - screenPosition.x);
		if (diffX > maximumDistanceFromMouse || diffX < -maximumDistanceFromMouse)
			return false;
		int diffY = (int)(mousePosition.y - screenPosition.y);
		return diffY <= maximumDistanceFromMouse && diffY >= -maximumDistanceFromMouse;
	}

	#endregion

	#region GUI Components - Progress Bar

	public static void ProgressBar(float value, string inlineText)
	{
		ProgressBar(null, value, inlineText);
	}

	public static void ProgressBar(string title, float value, string inlineText)
	{
		if (!string.IsNullOrEmpty(title))
		{
			GUILayout.Label(title);
		}

		EditorGUILayout.BeginHorizontal();
		var rect = EditorGUILayout.BeginVertical();
		EditorGUI.ProgressBar(rect, value, inlineText);
		GUILayout.Space(16);
		EditorGUILayout.EndVertical();
		GUILayout.Label(("% " + (int)(value * 100f)).ToString(), GUILayout.Width(40f));
		EditorGUILayout.EndHorizontal();
	}

	public static void ProgressBar(float value)
	{
		ProgressBar(null, value, null);
	}

	public static void ProgressBar(string title, float value)
	{
		ProgressBar(title, value, null);
	}

	#endregion
}
