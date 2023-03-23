using System;
using Extenity.DataToolbox;
using Extenity.MathToolbox;
using Extenity.ReflectionToolbox;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace Extenity.UnityEditorToolbox.Editor
{

	public abstract class ExtenityEditorWindowBase : EditorWindow
	{
		#region Configuration

		public struct WindowSpecifications
		{
			public string Title;
			public Texture2D Icon;

			public Vector2 MinimumWindowSize;
			public Vector2 MaximumWindowSize;

			public bool EnableRightMouseButtonScrolling;

			public bool WantsMouseMove;
			public bool WantsMouseEnterLeaveWindow;

			public bool AutoRepaintOnSceneChange;
		}

		protected abstract WindowSpecifications Specifications { get; }

		#endregion

		#region Initialization

		protected virtual void OnEnableDerived() { }

		protected void OnEnable()
		{
			// Load the state from EditorPrefs. This must be done as the first thing.
			// So all values are set before initialization codes. This also allows
			// hardcoded configuration (like the ones in Specifications) to be overwritten
			// onto whatever the previously serialized values were.
			if (EnableSavingStateToEditorPrefs)
				LoadStateFromEditorPrefs();

			var specs = Specifications;

			SetTitleAndIcon(specs.Title, specs.Icon);

			if (specs.MinimumWindowSize.IsAnyNonZero())
				minSize = specs.MinimumWindowSize;
			if (specs.MaximumWindowSize.IsAnyNonZero())
				maxSize = specs.MaximumWindowSize;

			IsRightMouseButtonScrollingEnabled = specs.EnableRightMouseButtonScrolling;

			if (wantsMouseMove != specs.WantsMouseMove)
				wantsMouseMove = specs.WantsMouseMove;
			if (wantsMouseEnterLeaveWindow != specs.WantsMouseEnterLeaveWindow)
				wantsMouseEnterLeaveWindow = specs.WantsMouseEnterLeaveWindow;
			if (autoRepaintOnSceneChange != specs.AutoRepaintOnSceneChange)
				autoRepaintOnSceneChange = specs.AutoRepaintOnSceneChange;

			InitializeAutoRepaint();
			InitializeOnSceneGUI();
			InitializeOnSelectionChanged();
			// InitializeOnCompilation();

			OnEnableDerived();

			// Do this after OnEnableDerived to allow the window to disable this feature.
			if (DisableWindowGUIOnCompilation)
			{
				SetToDisableWindowOnCompilation();
			}

			InitializeUpdate();
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDisableDerived() { }

		protected void OnDisable()
		{
			DeinitializeUpdate();
			DeinitializeOnSceneGUI();
			DeinitializeOnSelectionChanged();
			// DeinitializeOnCompilation();
			DeinitializeDisablingWindowOnCompilation();

			OnDisableDerived();

			// Save the state to EditorPrefs. This must be done as the last thing.
			// This allows the window to delete any fields that should not be serialized.
			if (EnableSavingStateToEditorPrefs)
				SaveStateToEditorPrefs();
		}

		protected virtual void OnDestroyDerived() { }

		protected void OnDestroy()
		{
			DeinitializeClosingWindowOnAssemblyReloadOrPlayModeChange();

			OnDestroyDerived();
		}

		#endregion

		#region Update

		protected virtual void Update() { }
		protected virtual void _InternalUpdate() { }
		public int RequestedUpdateRegistrationCount { get; private set; }

		private void InitializeUpdate()
		{
			// Do not register for updates just yet. Will register only when necessary.
			// RegisterUpdate();
		}

		private void DeinitializeUpdate()
		{
			DeregisterUpdate(true);
		}

		private void UpdateInternal()
		{
			// if (Me == null)
			// 	return;

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
		[NonSerialized]
		private float LastRepaintSceneViewTime;
		[NonSerialized]
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
		[NonSerialized]
		private float LastRepaintInspectorTime;
		[NonSerialized]
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


		private void InitializeAutoRepaint()
		{
			_IsAutoRepaintSceneViewEnabled = false;
			_IsAutoRepaintInspectorEnabled = false;

			LastRepaintSceneViewTime = 0f;
			LastRepaintInspectorTime = 0f;
		}

		private void UpdateAutoRepaint()
		{
			var currentTime = Time.realtimeSinceStartup;

			// SceneView
			if (LastRepaintSceneViewTime + AutoRepaintSceneViewPeriod < currentTime ||
			    LastRepaintSceneViewTime > currentTime) // Fix for Time.realtimeSinceStartup being reset between play mode changes.
			{
				SceneView.RepaintAll();
				LastRepaintSceneViewTime = currentTime;
			}

			// Inspector
			if (LastRepaintInspectorTime + AutoRepaintInspectorPeriod < currentTime ||
			    LastRepaintInspectorTime > currentTime) // Fix for Time.realtimeSinceStartup being reset between play mode changes.
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

		#region OnGUI

		protected abstract void OnGUIDerived();

		protected void OnGUI()
		{
			var wasDisabled = false;
			if (DisableWindowGUIOnCompilation)
			{
				if (EditorApplication.isCompiling)
				{
					wasDisabled = true;
					EditorGUI.BeginDisabledGroup(true);
				}
			}

			try
			{
				CalculateRightMouseButtonScrolling();
				OnGUIDerived();
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
			finally
			{
				if (wasDisabled)
				{
					EditorGUI.EndDisabledGroup();
				}
			}
		}

		#endregion

		#region BeforeSceneGUI / DuringSceneGUI

		private void InitializeOnSceneGUI()
		{
			// Register to the event if the method in base class is overriden.
			if (this.IsMethodOverriden(nameof(BeforeSceneGUI), new Type[] { typeof(SceneView) }))
			{
				SceneView.beforeSceneGui -= BeforeSceneGUI;
				SceneView.beforeSceneGui += BeforeSceneGUI;
			}
			if (this.IsMethodOverriden(nameof(DuringSceneGUI), new Type[] { typeof(SceneView) }))
			{
				SceneView.duringSceneGui -= DuringSceneGUI;
				SceneView.duringSceneGui += DuringSceneGUI;
			}
		}

		private void DeinitializeOnSceneGUI()
		{
			SceneView.beforeSceneGui -= BeforeSceneGUI;
			SceneView.duringSceneGui -= DuringSceneGUI;
		}

		protected virtual void BeforeSceneGUI(SceneView sceneView)
		{
		}

		protected virtual void DuringSceneGUI(SceneView sceneView)
		{
		}

		#endregion

		#region OnSelectionChanged

		private void InitializeOnSelectionChanged()
		{
			// Register to the event if the method in base class is overriden.
			if (this.IsMethodOverriden(nameof(OnSelectionChanged)))
			{
				Selection.selectionChanged -= OnSelectionChanged;
				Selection.selectionChanged += OnSelectionChanged;
			}
		}

		private void DeinitializeOnSelectionChanged()
		{
			Selection.selectionChanged -= OnSelectionChanged;
		}

		protected virtual void OnSelectionChanged()
		{
		}

		#endregion

		#region On Compilation

		/*
		private void InitializeOnCompilation()
		{
			// Register to the event if the method in base class is overriden.
			if (this.IsMethodOverriden(nameof(OnAssemblyCompilationStarted), new[] { typeof(string) }))
			{
				CompilationPipeline.assemblyCompilationStarted -= OnAssemblyCompilationStarted;
				CompilationPipeline.assemblyCompilationStarted += OnAssemblyCompilationStarted;
			}
			// Register to the event if the method in base class is overriden.
			if (this.IsMethodOverriden(nameof(OnAssemblyCompilationFinished), new[] { typeof(string), typeof(CompilerMessage[]) }))
			{
				CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
				CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
			}
		}

		private void DeinitializeOnCompilation()
		{
			CompilationPipeline.assemblyCompilationStarted -= OnAssemblyCompilationStarted;
			CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
		}

		protected virtual void OnAssemblyCompilationStarted(string outputAssemblyPath)
		{
		}

		protected virtual void OnAssemblyCompilationFinished(string outputAssemblyPath, CompilerMessage[] compilerMessages)
		{
		}
		*/

		#endregion

		#region Disable Window On Compilation

		public bool DisableWindowGUIOnCompilation = true;

		private GUIContent CompilationMessage;

		public void SetToDisableWindowOnCompilation(string message = "Compiling...")
		{
			DisableWindowGUIOnCompilation = true;
			CompilationMessage = new GUIContent(message);

			CompilationPipeline.compilationStarted -= _OnAssemblyCompilationStarted_ForDisablingWindow;
			CompilationPipeline.compilationStarted += _OnAssemblyCompilationStarted_ForDisablingWindow;
			CompilationPipeline.compilationFinished -= _OnAssemblyCompilationFinished_ForDisablingWindow;
			CompilationPipeline.compilationFinished += _OnAssemblyCompilationFinished_ForDisablingWindow;
		}

		private void DeinitializeDisablingWindowOnCompilation()
		{
			CompilationPipeline.compilationStarted -= _OnAssemblyCompilationStarted_ForDisablingWindow;
			CompilationPipeline.compilationFinished -= _OnAssemblyCompilationFinished_ForDisablingWindow;
		}

		private void _OnAssemblyCompilationStarted_ForDisablingWindow(object _)
		{
			ShowNotification(CompilationMessage);
			Repaint();
		}

		private void _OnAssemblyCompilationFinished_ForDisablingWindow(object _)
		{
			RemoveNotification();
			Repaint();
		}

		#endregion

		#region Relaunch Window

		protected void Relaunch(Action windowLauncherMethod, bool resetStateInEditorPrefs)
		{
			if (resetStateInEditorPrefs)
			{
				DeleteStateFromEditorPrefs();
				OverrideToSkipSavingStateToEditorPrefs = true;
			}
			Close();

			windowLauncherMethod();
		}

		#endregion

		#region Title And Icon

		/// <summary>
		/// Call this inside OnEnable. Icon texture should have been set for DontDestroyOnLoad and HideAndDontSave set for hideFlags.
		/// </summary>
		public void SetTitleAndIcon(string title, Texture2D icon)
		{
			titleContent = new GUIContent(title, icon);
		}

		#endregion

		#region Scroll

		protected Vector2 ScrollPosition = Vector2.zero;

		#endregion

		#region Scroll Window With Right Mouse Button

		public bool IsRightMouseButtonScrollingEnabled = false;
		private bool WasScrollingWithRightMouseButton;

		private void CalculateRightMouseButtonScrolling()
		{
			if (Event.current.isMouse)
			{
				if (IsRightMouseButtonScrollingEnabled && Event.current.button == 1 && Event.current.type == EventType.MouseDrag)
				{
					ScrollPosition -= Event.current.delta;

					Event.current.Use();
					Repaint();

					WasScrollingWithRightMouseButton = true;
				}

				// Prevent any right click events if right click is used for scrolling.
				if (WasScrollingWithRightMouseButton && Event.current.type == EventType.MouseUp)
				{
					WasScrollingWithRightMouseButton = false;
					Event.current.Use();
					Repaint();
				}
			}
		}

		#endregion

		#region Repaint After Script Reload

		//[DidReloadScripts]
		//private static void InternalRepaintAfterReload()
		//{
		//	// TODO: This will be implemented along with registering all windows in a static list. So that we can iterate the list here and call Repaint on them. Note that there is also EditorApplication.isCompiling if this approach fails in a way.
		//}

		#endregion

		#region Layout

		public static readonly GUILayoutOption SmallButtonHeight = GUILayout.Height(18);
		public static readonly GUILayoutOption MediumButtonHeight = GUILayout.Height(24);
		public static readonly GUILayoutOption BigButtonHeight = GUILayout.Height(36f);

		#endregion

		#region Thread Safe Repaint

		public void ThreadSafeRepaint()
		{
			EditorApplication.delayCall += Repaint;
		}

		#endregion

		#region Set To Close Window On AssemblyReload Or PlayModeChange

		protected void SetToCloseWindowOnAssemblyReloadOrPlayModeChange()
		{
			EditorApplication.playModeStateChanged += _CloseWindowOnAssemblyReloadOrPlayModeChange;
			AssemblyReloadEvents.beforeAssemblyReload += _CloseWindowOnAssemblyReloadOrPlayModeChange;
		}

		private void DeinitializeClosingWindowOnAssemblyReloadOrPlayModeChange()
		{
			EditorApplication.playModeStateChanged -= _CloseWindowOnAssemblyReloadOrPlayModeChange;
			AssemblyReloadEvents.beforeAssemblyReload -= _CloseWindowOnAssemblyReloadOrPlayModeChange;
		}

		private void _CloseWindowOnAssemblyReloadOrPlayModeChange(PlayModeStateChange stateChange)
		{
			_CloseWindowOnAssemblyReloadOrPlayModeChange();
		}

		private void _CloseWindowOnAssemblyReloadOrPlayModeChange()
		{
			Log.Info($"Closing '{titleContent.text}' window automatically.");
			DeinitializeClosingWindowOnAssemblyReloadOrPlayModeChange();
			Close();
		}

		#endregion

		#region Save Window State To EditorPrefs

		/// <summary>
		/// Override this with returning 'true' for saving the editor state to EditorPrefs
		/// when closing the window and loading the state when opening the window.
		/// </summary>
		protected virtual bool EnableSavingStateToEditorPrefs { get; }

		[NonSerialized]
		private bool OverrideToSkipSavingStateToEditorPrefs;

		private string EditorPrefsStateKey => GetType().Name + ".State";

		private void SaveStateToEditorPrefs()
		{
			if (OverrideToSkipSavingStateToEditorPrefs)
				return;

			try
			{
				var key = PlayerPrefsTools.GenerateKey(EditorPrefsStateKey, PathHashPostfix.Yes);
				var serialized = EditorJsonUtility.ToJson(this, false);
				EditorPrefs.SetString(key, serialized);
			}
			catch (Exception exception)
			{
				Log.Warning($"Encountered an error while saving '{GetType().Name}' window state to {nameof(EditorPrefs)}. Ignoring the error. Details: " + exception);
			}
		}

		private void LoadStateFromEditorPrefs()
		{
			try
			{
				var key = PlayerPrefsTools.GenerateKey(EditorPrefsStateKey, PathHashPostfix.Yes);
				var serialized = EditorPrefs.GetString(key, "");
				if (!string.IsNullOrWhiteSpace(serialized))
				{
					EditorJsonUtility.FromJsonOverwrite(serialized, this);
				}
			}
			catch (Exception exception)
			{
				Log.Warning($"Encountered an error while loading '{GetType().Name}' window state from {nameof(EditorPrefs)}. Ignoring the error. Details: " + exception);
			}
		}

		private void DeleteStateFromEditorPrefs()
		{
			var key = PlayerPrefsTools.GenerateKey(EditorPrefsStateKey, PathHashPostfix.Yes);
			EditorPrefs.DeleteKey(key);
		}

		#endregion

		#region serializedObject

		// Name intentionally left small casing to make it compatible with Editor.serializedObject
		private SerializedObject _serializedObject;
		public SerializedObject serializedObject
		{
			get
			{
				if (_serializedObject == null)
				{
					_serializedObject = new SerializedObject(this);
				}
				return _serializedObject;
			}
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(ExtenityEditorWindowBase));

		#endregion
	}

}
