using System;
using System.Reflection;
using Extenity.MathToolbox;
using Extenity.ReflectionToolbox;
using UnityEngine;
using UnityEditor;

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

			InitializeOnSceneGUI();
			InitializeOnSelectionChanged();

			OnEnableDerived();
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDisableDerived() { }

		protected void OnDisable()
		{
			DeinitializeOnSceneGUI();
			DeinitializeOnSelectionChanged();

			OnDisableDerived();
		}

		protected virtual void OnDestroyDerived() { }

		protected void OnDestroy()
		{
			DeinitializeClosingWindowOnAssemblyReloadOrPlayModeChange();

			OnDestroyDerived();
		}

		#endregion

		#region OnGUI

		protected abstract void OnGUIDerived();

		protected void OnGUI()
		{
			CalculateRightMouseButtonScrolling();
			OnGUIDerived();
		}

		#endregion

		#region OnSceneGUI

		private void InitializeOnSceneGUI()
		{
			var methodInfo = GetType().GetMethod(nameof(OnSceneGUI),
				BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance,
				null, CallingConventions.Any, new Type[] { typeof(SceneView) }, null);

			// Register to the event if the method in base class is overriden.
			if (methodInfo.IsOverride())
			{
				SceneView.onSceneGUIDelegate -= OnSceneGUI;
				SceneView.onSceneGUIDelegate += OnSceneGUI;
			}
		}

		private void DeinitializeOnSceneGUI()
		{
			SceneView.onSceneGUIDelegate -= OnSceneGUI;
		}

		protected virtual void OnSceneGUI(SceneView sceneView)
		{
		}

		#endregion

		#region OnSelectionChanged

		private void InitializeOnSelectionChanged()
		{
			var methodInfo = GetType().GetMethod(nameof(OnSelectionChanged),
				BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance,
				null, CallingConventions.Any, new Type[0], null);

			// Register to the event if the method in base class is overriden.
			if (methodInfo.IsOverride())
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
	}

}
