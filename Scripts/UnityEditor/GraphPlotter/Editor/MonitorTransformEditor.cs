// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MonitorComponents 
{
	[CustomEditor(typeof(MonitorTransform))]
	public class MonitorTransformEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			MonitorTransform monitorTransform = target as MonitorTransform;

			EditorGUILayout.Space();

			// Position
			bool newShowPosition = EditorGUILayout.ToggleLeft("Position", monitorTransform.showPosition);
			if (newShowPosition != monitorTransform.showPosition)
			{
				Undo.RecordObject(target, "Toggle position");
				monitorTransform.showPosition = newShowPosition;
			}

			if (monitorTransform.showPosition)
			{	
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowPosition_x = EditorGUILayout.Toggle(monitorTransform.showPosition_x, GUILayout.Width(14));
				if (newShowPosition_x != monitorTransform.showPosition_x)
				{
					Undo.RecordObject(target, "Toggle position x");
					monitorTransform.showPosition_x = newShowPosition_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowPosition_y = EditorGUILayout.Toggle(monitorTransform.showPosition_y, GUILayout.Width(14));
				if (newShowPosition_y != monitorTransform.showPosition_y)
				{
					Undo.RecordObject(target, "Toggle position y");
					monitorTransform.showPosition_y = newShowPosition_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowPosition_z = EditorGUILayout.Toggle(monitorTransform.showPosition_z, GUILayout.Width(14));
				if (newShowPosition_z != monitorTransform.showPosition_z)
				{
					Undo.RecordObject(target, "Toggle position z");
					monitorTransform.showPosition_z = newShowPosition_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				MonitorTransform.Space newPositionSpace = (MonitorTransform.Space) EditorGUILayout.EnumPopup("Space", monitorTransform.positionSpace);
				if (newPositionSpace != monitorTransform.positionSpace)
				{
					Undo.RecordObject(target, "Changed position space");
					monitorTransform.positionSpace = newPositionSpace;
				}
				EditorGUILayout.EndHorizontal();

				float newMin, newMax;
				Utils.AxisSettings(monitorTransform, ref monitorTransform.positionMode, monitorTransform.positionMin, out newMin, monitorTransform.positionMax, out newMax);
				if (newMin != monitorTransform.positionMin)
				{
					monitorTransform.positionMin = newMin;
					if (monitorTransform.monitor_position != null)
					{
						monitorTransform.monitor_position.Min = monitorTransform.positionMin;
					}
				}

				if (newMax != monitorTransform.positionMax)
				{
					monitorTransform.positionMax = newMax;
					if (monitorTransform.monitor_position != null)
					{
						monitorTransform.monitor_position.Max = monitorTransform.positionMax;
					}
				}

				EditorGUILayout.Space();
			}

			// Rotation
			bool newShowRotation = EditorGUILayout.ToggleLeft("Rotation", monitorTransform.showRotation);
			if (newShowRotation != monitorTransform.showRotation)
			{
				Undo.RecordObject(target, "Toggle rotation");
				monitorTransform.showRotation = newShowRotation;
			}

			if (monitorTransform.showRotation)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowRotation_x = EditorGUILayout.Toggle(monitorTransform.showRotation_x, GUILayout.Width(14));
				if (newShowRotation_x != monitorTransform.showRotation_x)
				{
					Undo.RecordObject(target, "Toggle rotation x");
					monitorTransform.showRotation_x = newShowRotation_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowRotation_y = EditorGUILayout.Toggle(monitorTransform.showRotation_y, GUILayout.Width(14));
				if (newShowRotation_y != monitorTransform.showRotation_y)
				{
					Undo.RecordObject(target, "Toggle rotation y");
					monitorTransform.showRotation_y = newShowRotation_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowRotation_z = EditorGUILayout.Toggle(monitorTransform.showRotation_z, GUILayout.Width(14));
				if (newShowRotation_z != monitorTransform.showRotation_z)
				{
					Undo.RecordObject(target, "Toggle rotation z");
					monitorTransform.showRotation_z = newShowRotation_z;
				}

				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				float newMin, newMax;
				MonitorTransform.Space newRotationSpace = (MonitorTransform.Space) EditorGUILayout.EnumPopup("Space", monitorTransform.rotationSpace);
				if (newRotationSpace != monitorTransform.rotationSpace)
				{
					Undo.RecordObject(target, "Changed rotation space");
					monitorTransform.rotationSpace = newRotationSpace;
				}

				Utils.AxisSettings(monitorTransform, ref monitorTransform.rotationMode, monitorTransform.rotationMin, out newMin, monitorTransform.rotationMax, out newMax);
				if (newMin != monitorTransform.rotationMin)
				{
					monitorTransform.rotationMin = newMin;
					if (monitorTransform.monitor_rotation != null)
					{
						monitorTransform.monitor_rotation.Min = monitorTransform.rotationMin;
					}
				}

				if (newMax != monitorTransform.rotationMax)
				{
					monitorTransform.rotationMax = newMax;
					if (monitorTransform.monitor_rotation != null)
					{
						monitorTransform.monitor_rotation.Max = monitorTransform.rotationMax;
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}


			// Scale
			bool newShowScale = EditorGUILayout.ToggleLeft("Scale", monitorTransform.showScale);
			if (newShowScale != monitorTransform.showScale)
			{
				Undo.RecordObject(target, "Toggle scale");
				monitorTransform.showScale = newShowScale;
			}

			if (monitorTransform.showScale)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowScale_x = EditorGUILayout.Toggle(monitorTransform.showScale_x, GUILayout.Width(14));
				if (newShowScale_x != monitorTransform.showScale_x)
				{
					Undo.RecordObject(target, "Toggle scale x");
					monitorTransform.showScale_x = newShowScale_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowScale_y = EditorGUILayout.Toggle(monitorTransform.showScale_y, GUILayout.Width(14));
				if (newShowScale_y != monitorTransform.showScale_y)
				{
					Undo.RecordObject(target, "Toggle scale y");
					monitorTransform.showScale_y = newShowScale_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowScale_z = EditorGUILayout.Toggle(monitorTransform.showScale_z, GUILayout.Width(14));
				if (newShowScale_z != monitorTransform.showScale_z)
				{
					Undo.RecordObject(target, "Toggle scale z");
					monitorTransform.showScale_z = newShowScale_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				MonitorTransform.ScaleSpace newScaleSpace = (MonitorTransform.ScaleSpace) EditorGUILayout.EnumPopup("Space", monitorTransform.scaleSpace);
				if (newScaleSpace != monitorTransform.scaleSpace)
				{
					Undo.RecordObject(target, "Change scale space");
					monitorTransform.scaleSpace = newScaleSpace;
				}

				float newMin, newMax;
				Utils.AxisSettings(monitorTransform, ref monitorTransform.scaleMode, monitorTransform.scaleMin, out newMin, monitorTransform.scaleMax, out newMax);
				if (newMin != monitorTransform.scaleMin)
				{
					monitorTransform.scaleMin = newMin;
					if (monitorTransform.monitor_scale != null)
					{
						monitorTransform.monitor_scale.Min = monitorTransform.scaleMin;
					}
				}

				if (newMax != monitorTransform.scaleMax)
				{
					monitorTransform.scaleMax = newMax;
					if (monitorTransform.monitor_scale != null)
					{
						monitorTransform.monitor_scale.Max = monitorTransform.scaleMax;
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			EditorGUILayout.Space();

			Utils.OpenButton(monitorTransform.gameObject);

			monitorTransform.UpdateMonitors();

			if (GUI.changed)
	            EditorUtility.SetDirty (target);
		}
	}
}
