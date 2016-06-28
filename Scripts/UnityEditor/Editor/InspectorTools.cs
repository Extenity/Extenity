using System;
using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;

public static class InspectorTools
{
	#region Inspect Target In New Inspector

	/// <summary>
	/// Creates a new inspector window instance and locks it to inspect the specified target
	/// </summary>
	public static void InspectTargetInNewInspector(GameObject target)
	{
		// Create an InspectorWindow instance
		var inspectorInstance = ScriptableObject.CreateInstance(InspectorWindowType) as EditorWindow;
		// We display it - currently, it will inspect whatever gameObject is currently selected
		// So we need to find a way to let it inspect/aim at our target GO that we passed
		// For that we do a simple trick:
		// 1- Cache the current selected gameObject
		// 2- Set the current selection to our target GO (so now all inspectors are targeting it)
		// 3- Lock our created inspector to that target
		// 4- Fallback to our previous selection
		inspectorInstance.Show();
		// Cache previous selected gameObject
		var prevSelection = Selection.activeGameObject;
		// Set the selection to GO we want to inspect
		Selection.activeGameObject = target;
		// Get a ref to the "locked" property, which will lock the state of the inspector to the current inspected target
		var isLocked = InspectorWindowType.GetProperty("isLocked", BindingFlags.Instance | BindingFlags.Public);
		// Invoke `isLocked` setter method passing 'true' to lock the inspector
		isLocked.GetSetMethod().Invoke(inspectorInstance, new object[] { true });
		// Finally revert back to the previous selection so that other inspectors continue to inspect whatever they were inspecting...
		Selection.activeGameObject = prevSelection;
	}

	#endregion

	#region Caches

	private static Assembly _EditorAssembly;
	public static Assembly EditorAssembly
	{
		get
		{
			if (_EditorAssembly == null)
				_EditorAssembly = Assembly.GetAssembly(typeof(Editor));
			return _EditorAssembly;
		}
	}

	private static Type _InspectorWindowType;
	public static Type InspectorWindowType
	{
		get
		{
			if (_InspectorWindowType == null)
				_InspectorWindowType = EditorAssembly.GetType("UnityEditor.InspectorWindow", true, false);
			return _InspectorWindowType;
		}
	}

	private static FieldInfo _CurrentInspectorWindowField;
	public static FieldInfo CurrentInspectorWindowField
	{
		get
		{
			if (_CurrentInspectorWindowField == null)
				_CurrentInspectorWindowField = InspectorWindowType.GetField("s_CurrentInspectorWindow", BindingFlags.Public | BindingFlags.Static);
			return _CurrentInspectorWindowField;
		}
	}

	public static EditorWindow CurrentInspectorWindow
	{
		get { return CurrentInspectorWindowField.GetValue(null) as EditorWindow; }
	}

	private static Type _ActiveEditorTrackerType;
	public static Type ActiveEditorTrackerType
	{
		get
		{
			if (_ActiveEditorTrackerType == null)
				_ActiveEditorTrackerType = EditorAssembly.GetType("UnityEditor.ActiveEditorTracker", true, false);
			return _ActiveEditorTrackerType;
		}
	}

	private static FieldInfo _CurrentActiveTrackerField;
	public static FieldInfo CurrentActiveTrackerField
	{
		get
		{
			if (_CurrentActiveTrackerField == null)
				_CurrentActiveTrackerField = InspectorWindowType.GetField("m_Tracker", BindingFlags.NonPublic | BindingFlags.Instance);
			return _CurrentActiveTrackerField;
		}
	}

	public static object CurrentActiveTracker
	{
		get { return CurrentActiveTrackerField.GetValue(CurrentInspectorWindow); }
	}

	private static PropertyInfo _IsTrackerLockedPropertyInfo;
	public static PropertyInfo IsTrackerLockedPropertyInfo
	{
		get
		{
			if (_IsTrackerLockedPropertyInfo == null)
				_IsTrackerLockedPropertyInfo = ActiveEditorTrackerType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
			return _IsTrackerLockedPropertyInfo;
		}
	}

	#endregion

	#region RepaintAllInspectors

	private static MethodInfo _RepaintAllInspectorsMethod;
	public static MethodInfo RepaintAllInspectorsMethod
	{
		get
		{
			if (_RepaintAllInspectorsMethod == null)
			{
				_RepaintAllInspectorsMethod = InspectorWindowType.GetMethod("RepaintAllInspectors", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			}
			return _RepaintAllInspectorsMethod;
		}
	}

	public static void RepaintAllInspectors()
	{
		RepaintAllInspectorsMethod.Invoke(null, null);
	}

	#endregion

	#region IsInspectorLocked

	public static bool IsInspectorLocked
	{
		get
		{
			return (bool)IsTrackerLockedPropertyInfo.GetValue(CurrentActiveTracker, null);
		}
	}

	#endregion

	#region CloseInspector

	public static void CloseInspector()
	{
		var inspector = CurrentInspectorWindow;
		if (inspector != null)
		{
			inspector.Close();
		}
	}

	#endregion

	#region Drawing

	public static void DrawHorizontalLine()
	{
		EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
	}

	#endregion

	#region Drag and Drop

	public static bool AllDraggedObjectsContain<TComponent>() where TComponent : Component
	{
		foreach (var draggedObject in DragAndDrop.objectReferences)
		{
			var draggedGameObject = draggedObject as GameObject;

			if (draggedGameObject == null)
			{
				return false;
			}
			if (draggedGameObject.GetComponent<TComponent>() == null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool AllDraggedObjectsArePrefab()
	{
		foreach (var draggedObject in DragAndDrop.objectReferences)
		{
			var draggedGameObject = draggedObject as GameObject;

			if (draggedGameObject == null)
			{
				return false;
			}

			if (PrefabUtility.GetPrefabParent(draggedGameObject) != null || PrefabUtility.GetPrefabObject(draggedGameObject) == null)
			{
				return false;
			}
		}
		return true;
	}

	#endregion
}
