using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Extenity.CameraToolbox;

public abstract class ExtenityEditorBase<T> : Editor where T : UnityEngine.Behaviour
{
	#region Initialization

	protected virtual void OnEnableBase() { }
	protected abstract void OnEnableDerived();
	protected virtual void OnDisableBase() { }
	protected abstract void OnDisableDerived();

	protected void OnEnable()
	{
		Me = target as T;
		Configuration = new SerializedObject(target);

		OnEnableBase();
		OnEnableDerived();

		//RegisterUpdate();
	}

	protected void OnDisable()
	{
		DeregisterUpdate(true);

		OnDisableDerived();
		OnDisableBase();
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
		UpdateMovementDetection();
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
	public bool IsDefaultInspectorScriptFieldEnabed = false;
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
			if (IsDefaultInspectorScriptFieldEnabed)
			{
				DrawDefaultInspector();
			}
			else
			{
				DrawPropertiesExcluding(Configuration, ExcludedPropertiesInDefaultInspector);
			}
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

	#region Transform Movement Detection

	public Vector3 MovementDetectionPreviousPosition { get; private set; }
	public Quaternion MovementDetectionPreviousRotation { get; private set; }
	public Vector3 MovementDetectionPreviousScale { get; private set; }

	protected virtual void OnMovementDetected() { }

	private bool _IsMovementDetectionEnabled;
	public bool IsMovementDetectionEnabled
	{
		get { return _IsMovementDetectionEnabled; }
		set
		{
			if (value == _IsMovementDetectionEnabled)
				return;

			if (value)
				InitializeMovementDetection();

			if (value)
				RegisterUpdate();
			else
				DeregisterUpdate();

			_IsMovementDetectionEnabled = value;
		}
	}

	private void InitializeMovementDetection()
	{
		var transform = Me.transform;
		MovementDetectionPreviousPosition = transform.position;
		MovementDetectionPreviousRotation = transform.rotation;
		MovementDetectionPreviousScale = transform.localScale;
	}

	private void UpdateMovementDetection()
	{
		if (!IsMovementDetectionEnabled)
			return;

		var transform = Me.transform;
		bool detected =
			MovementDetectionPreviousPosition != transform.position ||
			MovementDetectionPreviousRotation != transform.rotation ||
			MovementDetectionPreviousScale != transform.localScale;

		if (detected)
		{
			OnMovementDetected();
			MovementDetectionPreviousPosition = transform.position;
			MovementDetectionPreviousRotation = transform.rotation;
			MovementDetectionPreviousScale = transform.localScale;
		}
	}

	#endregion

	#region Mouse

	protected static Vector2 MouseSceneViewPosition
	{
		get
		{
			var mouseScreenPosition = Event.current.mousePosition;
			mouseScreenPosition.y = SceneView.lastActiveSceneView.camera.pixelHeight - mouseScreenPosition.y;
			return mouseScreenPosition;
		}
	}

	protected static bool IsMouseCloseToScreenPoint(Vector2 mousePosition, Vector3 screenPosition, float maximumDistanceFromMouse)
	{
		int diffX = (int)(mousePosition.x - screenPosition.x);
		if (diffX > maximumDistanceFromMouse || diffX < -maximumDistanceFromMouse)
			return false;
		int diffY = (int)(mousePosition.y - screenPosition.y);
		return diffY <= maximumDistanceFromMouse && diffY >= -maximumDistanceFromMouse;
	}

	protected bool IsMouseCloseToWorldPointInScreenCoordinates(Camera camera, Vector3 worldPoint, Vector2 mousePosition, float maximumDistanceFromMouse)
	{
		var screenPosition = camera.WorldToScreenPointWithReverseCheck(worldPoint);
		return
			screenPosition.HasValue &&
			IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, maximumDistanceFromMouse);
	}

	protected Vector2 GetDifferenceBetweenMousePositionAndWorldPoint(Camera camera, Vector3 worldPoint, Vector2 mousePosition, float maximumDistanceFromMouse = 0f)
	{
		var screenPosition = camera.WorldToScreenPointWithReverseCheck(worldPoint);
		if (screenPosition.HasValue)
		{
			var diff = mousePosition - screenPosition.Value.ToVector2XY();
			if (maximumDistanceFromMouse > 0f)
			{
				if (diff.sqrMagnitude < maximumDistanceFromMouse * maximumDistanceFromMouse)
				{
					return diff;
				}
			}
			else
			{
				return diff;
			}
		}
		return new Vector2(float.PositiveInfinity, float.PositiveInfinity);
	}

	#endregion

	#region Layout

	public static readonly GUILayoutOption BigButtonHeight = GUILayout.Height(30f);

	#endregion

	#region Horizontal Line

	private GUILayoutOption[] HorizontalLineLayoutOptions;

	public void DrawHorizontalLine()
	{
		if (HorizontalLineLayoutOptions == null)
		{
			HorizontalLineLayoutOptions = new[] { GUILayout.ExpandWidth(true), GUILayout.Height(1) };
		}

		GUILayout.Box("", HorizontalLineLayoutOptions);
	}

	#endregion
}
