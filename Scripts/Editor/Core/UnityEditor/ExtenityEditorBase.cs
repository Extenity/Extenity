using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Extenity.CameraToolbox;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.MathToolbox;
using Extenity.ProfilingToolbox;
using UnityEditor.SceneManagement;

namespace Extenity.UnityEditorToolbox.Editor
{

	public abstract class ExtenityObjectEditorBase<T> : UnityEditor.Editor where T : UnityEngine.Object
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
		protected virtual void _InternalUpdate() { }
		public int RequestedUpdateRegistrationCount { get; private set; }

		private void UpdateInternal()
		{
			if (Me == null)
				return;

			Update();
			UpdateAutoRepaint();
			_InternalUpdate();
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

		public void RepaintCurrentSceneView()
		{
			HandleUtility.Repaint();
		}

		public void RepaintAllSceneViews()
		{
			SceneView.RepaintAll();
		}

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
		public bool IsDefaultInspectorScriptFieldEnabled = false;
		public bool IsDefaultInspectorScriptFieldNotReadOnly = true;
		public bool IsInspectorDisabledWhenPlaying = false;

		private static readonly string[] ExcludedPropertiesInDefaultInspector = { "m_Script" };

		protected virtual void OnBeforeDefaultInspectorGUI() { }
		protected abstract void OnAfterDefaultInspectorGUI();

		public sealed override void OnInspectorGUI()
		{
			var currentEventType = Event.current.rawType;
			var guiProfilingRunningMean = BeginGUIProfiling(currentEventType);

			Configuration.Update();

			var disabled = IsInspectorDisabledWhenPlaying && Application.isPlaying;
			if (disabled)
				EditorGUI.BeginDisabledGroup(true);

			OnBeforeDefaultInspectorGUI();

			if (IsDefaultInspectorDrawingEnabled)
			{
				if (IsDefaultInspectorScriptFieldEnabled)
				{
					if (IsDefaultInspectorScriptFieldNotReadOnly)
					{
						DrawScriptField();
						DrawDefaultInspectorWithoutScriptField();
					}
					else
					{
						DrawDefaultInspector(); // This one seems to draw readonly script field.
					}
				}
				else
				{
					DrawDefaultInspectorWithoutScriptField();
				}
			}

			OnAfterDefaultInspectorGUI();

			if (disabled)
				EditorGUI.EndDisabledGroup();

			Configuration.ApplyModifiedProperties();

			EndGUIProfiling(guiProfilingRunningMean);
		}

		public void DrawDefaultInspectorWithoutScriptField()
		{
			DrawPropertiesExcluding(Configuration, ExcludedPropertiesInDefaultInspector);
		}

		public void DrawScriptField()
		{
			EditorGUILayout.PropertyField(GetProperty("m_Script"));
		}

		#endregion

		#region Invalidate Modified Properties

		[Obsolete("This method was using EditorUtility.SetDirty which no longer works as before. See Unity documentation.")]
		protected void InvalidateModifiedProperties()
		{
			EditorUtility.SetDirty(Me);
		}

		protected void InvalidateModifiedPropertiesForPrefab()
		{
			EditorUtility.SetDirty(Me);
		}

		protected void InvalidateScene()
		{
			// Try to invalidate only the scene that this object is included.
			var behaviour = Me as Behaviour;
			if (behaviour)
			{
				EditorSceneManager.MarkSceneDirty(behaviour.gameObject.scene);
			}
			else
			{
				// Just invalidate all scenes and move on.
				EditorSceneManager.MarkAllScenesDirty();
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

		protected static bool IsMouseCloseToScreenPoint(Vector2 mousePosition, Vector3 screenPosition, float maximumDistanceFromMouse, out Vector2 difference)
		{
			difference.x = mousePosition.x - screenPosition.x;
			difference.y = mousePosition.y - screenPosition.y;
			if (difference.x > maximumDistanceFromMouse || difference.x < -maximumDistanceFromMouse ||
				difference.y > maximumDistanceFromMouse && difference.y < -maximumDistanceFromMouse)
			{
				return false;
			}
			return true;
		}

		protected bool IsMouseCloseToWorldPointInScreenCoordinates(Camera camera, Vector3 worldPoint, Vector2 mousePosition, float maximumDistanceFromMouse)
		{
			var screenPosition = camera.WorldToScreenPointWithReverseCheck(worldPoint);
			return
				screenPosition.HasValue &&
				IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, maximumDistanceFromMouse);
		}

		protected bool IsMouseCloseToWorldPointInScreenCoordinates(Camera camera, Vector3 worldPoint, Vector2 mousePosition, float maximumDistanceFromMouse, out Vector2 difference)
		{
			var screenPosition = camera.WorldToScreenPointWithReverseCheck(worldPoint);
			if (screenPosition.HasValue)
			{
				return IsMouseCloseToScreenPoint(mousePosition, screenPosition.Value, maximumDistanceFromMouse, out difference);
			}
			difference = Vector2Tools.PositiveInfinity;
			return false;
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

		public static readonly GUILayoutOption SmallButtonHeight = GUILayout.Height(18);
		public static readonly GUILayoutOption MediumButtonHeight = GUILayout.Height(24);
		public static readonly GUILayoutOption BigButtonHeight = GUILayout.Height(36f);

		#endregion

		#region Horizontal Line

		private GUILayoutOption[] HorizontalLineLayoutOptions;

		public void DrawHorizontalLine()
		{
			if (HorizontalLineLayoutOptions == null)
			{
				HorizontalLineLayoutOptions = new[] { GUILayoutTools.ExpandWidth, GUILayout.Height(1) };
			}

			GUILayout.Box("", HorizontalLineLayoutOptions);
		}

		#endregion

		#region GUI Profiling

		private bool _IsGUIProfilingEnabled;
		public bool IsGUIProfilingEnabled { get { return _IsGUIProfilingEnabled; } }

		private Dictionary<EventType, TickAnalyzer> GUIProfilingTickAnalyzer;
		private Dictionary<EventType, RunningHotMeanFloat> GUIProfilingTimes_ProcessBuffer;
		private Dictionary<EventType, RunningHotMeanFloat> GUIProfilingTimes_RenderBuffer;
		private bool IsGUIProfilingBufferSwapQueued;
		private float GUIProfilingBeginTime = float.NaN;

		private void InvokeGUIProfilingTimesBufferSwap()
		{
			if (IsGUIProfilingBufferSwapQueued)
				return;
			IsGUIProfilingBufferSwapQueued = true;
			EditorApplication.delayCall += SwapGUIProfilingTimesBuffer;
		}

		private void SwapGUIProfilingTimesBuffer()
		{
			if (!IsGUIProfilingBufferSwapQueued)
				return;
			IsGUIProfilingBufferSwapQueued = false;

			// Copy ProcessBuffer to RenderBuffer
			GUIProfilingTimes_RenderBuffer.Clear();
			foreach (var item in GUIProfilingTimes_ProcessBuffer)
			{
				GUIProfilingTimes_RenderBuffer.Add(item.Key, item.Value);
			}
		}

		public void EnableGUIProfiling(bool enableAutoRepaintAsWell = true)
		{
			if (enableAutoRepaintAsWell)
			{
				IsAutoRepaintInspectorEnabled = true;
				if (AutoRepaintInspectorPeriod > 1f)
					AutoRepaintInspectorPeriod = 1f;
			}
			EditorApplication.delayCall += () => _IsGUIProfilingEnabled = true;
		}

		public void DisableGUIProfiling()
		{
			EditorApplication.delayCall += () => _IsGUIProfilingEnabled = false;
		}

		private RunningHotMeanFloat BeginGUIProfiling(EventType currentEventType)
		{
			if (!_IsGUIProfilingEnabled)
				return null;

			var now = Time.realtimeSinceStartup;

			if (GUIProfilingTimes_ProcessBuffer == null)
				GUIProfilingTimes_ProcessBuffer = new Dictionary<EventType, RunningHotMeanFloat>();
			if (GUIProfilingTimes_RenderBuffer == null)
				GUIProfilingTimes_RenderBuffer = new Dictionary<EventType, RunningHotMeanFloat>();
			if (GUIProfilingTickAnalyzer == null)
				GUIProfilingTickAnalyzer = new Dictionary<EventType, TickAnalyzer>();

			if (!GUIProfilingTimes_ProcessBuffer.TryGetValue(currentEventType, out var runningMean))
			{
				runningMean = new RunningHotMeanFloat(10);
				GUIProfilingTimes_ProcessBuffer.Add(currentEventType, runningMean);
			}

			if (!GUIProfilingTickAnalyzer.TryGetValue(currentEventType, out var tickAnalyzer))
			{
				tickAnalyzer = new TickAnalyzer();
				tickAnalyzer.Reset(now);
				GUIProfilingTickAnalyzer.Add(currentEventType, tickAnalyzer);
			}

			DrawGUIProfilingTimes(now);

			tickAnalyzer.Tick(now);
			GUIProfilingBeginTime = now;
			return runningMean;
		}

		private void EndGUIProfiling(RunningHotMeanFloat runningMean)
		{
			if (runningMean != null)
			{
				var now = Time.realtimeSinceStartup;
				runningMean.Push(now - GUIProfilingBeginTime);
				GUIProfilingBeginTime = float.NaN;
				InvokeGUIProfilingTimesBufferSwap();
			}
		}

		private void DrawGUIProfilingTimes(float now)
		{
			if (GUIProfilingTimes_RenderBuffer == null)
				return;

			EditorGUILayoutTools.DrawHeader("Profiling Results");
			foreach (var item in GUIProfilingTimes_RenderBuffer)
			{
				var tickAnalyzer = GUIProfilingTickAnalyzer[item.Key];
				var tps = now < 1f + tickAnalyzer.LastTickTime
					? tickAnalyzer.TicksPerSecond
					: 0;
				EditorGUILayout.TextField(item.Key.ToString(), $"{item.Value.Mean:N3} sec \t {tps} TPS");
			}
			GUILayout.Space(40f);
		}

		#endregion
	}

	public abstract class ExtenityEditorBase<T> : ExtenityObjectEditorBase<T> where T : UnityEngine.Behaviour
	{
		#region Update

		protected override void _InternalUpdate()
		{
			UpdateMovementDetection();
			base._InternalUpdate();
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
				if (IsGroundSnappingEnabled)
				{
					Me.transform.SnapToGround(GroundSnappingRaycastDistance, GroundSnappingRaycastSteps, GroundSnappingRaycastLayerMask, GroundSnappingOffset);
				}
				OnMovementDetected();
				MovementDetectionPreviousPosition = transform.position;
				MovementDetectionPreviousRotation = transform.rotation;
				MovementDetectionPreviousScale = transform.localScale;
			}
		}

		#endregion

		#region Ground Snapping

		public bool IsGroundSnappingEnabled { get; private set; }
		public int GroundSnappingRaycastLayerMask = 0;
		public float GroundSnappingRaycastDistance = 30f;
		public int GroundSnappingRaycastSteps = 20;
		public float GroundSnappingOffset = 0f;

		/// <summary>
		/// Note that ground snapping requires Movement Detection, which is automatically enabled when enabling ground snapping.
		/// </summary>
		public void EnableGroundSnapping(int raycastLayerMask)
		{
			GroundSnappingRaycastLayerMask = raycastLayerMask;
			if (!IsMovementDetectionEnabled)
				IsMovementDetectionEnabled = true;
			IsGroundSnappingEnabled = true;
		}

		/// <summary>
		/// Note that ground snapping requires Movement Detection, which is automatically enabled when enabling ground snapping.
		/// </summary>
		public void EnableGroundSnapping(int raycastLayerMask, float raycastDistance, int raycastSteps = 20, float offset = 0f)
		{
			GroundSnappingRaycastDistance = raycastDistance;
			GroundSnappingRaycastSteps = raycastSteps;
			GroundSnappingOffset = offset;
			EnableGroundSnapping(raycastLayerMask);
		}

		public void DisableGroundSnapping()
		{
			IsGroundSnappingEnabled = false;
		}

		#endregion
	}

	public static class ExtenityEditorBaseTools
	{
		//[MenuItem("CONTEXT/Component/Toggle Extenity Component Profiling", true)]
		//private static bool ContextMenu_ToggleExtenityComponentProfiling_Validate(MenuCommand menuCommand)
		//{
		//	var component = menuCommand.context as Component;
		//	return component && component.GetType().HasExtenityInspector();
		//}

		//[MenuItem("CONTEXT/Component/Toggle Extenity Component Profiling", priority = 1051)]
		//private static void ContextMenu_ToggleExtenityComponentProfiling(MenuCommand menuCommand)
		//{
		//	var component = menuCommand.context as Component;
		//	if (component)
		//	{
		//		Oops! How to get the inspector object of the component? Well done Unity!
		//	}
		//}

		#region

		/// <summary>
		/// First Type is the object's type. Second Type is the object's inspector type that is derived from ExtenityEditorBase.
		/// </summary>
		private static Dictionary<Type, Type> ExtenityEditorTypes;

		public static bool HasExtenityInspector(this Type type)
		{
			InitializeExtenityEditorTypesDictionaryIfNecessary();
			return ExtenityEditorTypes.ContainsKey(type);
		}

		public static Type FindExtenityInspectorType(this Type type)
		{
			InitializeExtenityEditorTypesDictionaryIfNecessary();
			ExtenityEditorTypes.TryGetValue(type, out var inspectorType);
			return inspectorType;
		}

		private static void InitializeExtenityEditorTypesDictionaryIfNecessary()
		{
			if (ExtenityEditorTypes == null)
			{
				//Log.Info("Searching for Extenity editor base types");

				ExtenityEditorTypes = new Dictionary<Type, Type>();

				foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				{
					foreach (var inspectorType in assembly.GetTypes())
					{
						var baseInspectorType = inspectorType.GetRawGenericSubclass(typeof(ExtenityEditorBase<>));
						if (baseInspectorType != null && inspectorType != typeof(ExtenityEditorBase<>))
						{
							var objectType = baseInspectorType.GetGenericArguments()[0];
							if (objectType == null)
							{
								throw new Exception();
							}
							ExtenityEditorTypes.Add(objectType, inspectorType);
							//Log.Info($"Found inspector type '{inspectorType.FullName}' of object type '{objectType.FullName}'.");
						}

					}
				}
			}
		}

		#endregion
	}

}
