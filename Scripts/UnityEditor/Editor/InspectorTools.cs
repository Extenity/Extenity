using System;
using UnityEngine;
using System.Collections;
using System.Reflection;
using UnityEditor;

public static class InspectorTools
{
	#region InspectorWindow

	private static Type _InspectorWindowType;
	public static Type InspectorWindowType
	{
		get
		{
			if (_InspectorWindowType == null)
			{
				_InspectorWindowType = typeof(EditorApplication).Assembly.GetType("UnityEditor.InspectorWindow", true);
			}
			return _InspectorWindowType;
		}
	}

	private static FieldInfo _CurrentInspectorWindowField;
	public static FieldInfo CurrentInspectorWindowField
	{
		get
		{
			if (_CurrentInspectorWindowField == null)
			{
				_CurrentInspectorWindowField = InspectorWindowType.GetField("s_CurrentInspectorWindow", BindingFlags.Public | BindingFlags.Static);
			}
			return _CurrentInspectorWindowField;
		}
	}

	public static object CurrentInspectorWindow
	{
		get { return CurrentInspectorWindowField.GetValue(null); }
	}

	#endregion

	#region ActiveEditorTracker

	private static Type _ActiveEditorTrackerType;
	public static Type ActiveEditorTrackerType
	{
		get
		{
			if (_ActiveEditorTrackerType == null)
			{
				_ActiveEditorTrackerType = typeof(EditorApplication).Assembly.GetType("UnityEditor.ActiveEditorTracker", true);
			}
			return _ActiveEditorTrackerType;
		}
	}

	private static FieldInfo _CurrentActiveTrackerField;
	public static FieldInfo CurrentActiveTrackerField
	{
		get
		{
			if (_CurrentActiveTrackerField == null)
			{
				_CurrentActiveTrackerField = InspectorWindowType.GetField("m_Tracker", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			return _CurrentActiveTrackerField;
		}
	}

	public static object CurrentActiveTracker
	{
		get { return CurrentActiveTrackerField.GetValue(CurrentInspectorWindow); }
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
			var isLockedPropertyInfo = ActiveEditorTrackerType.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);
			return (bool)isLockedPropertyInfo.GetValue(CurrentActiveTracker, null);
		}
	}

	#endregion
}
