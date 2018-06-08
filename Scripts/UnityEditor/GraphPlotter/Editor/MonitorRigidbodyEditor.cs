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
	[CustomEditor(typeof(MonitorRigidbody))]
	public class MonitorRigidbodyEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			MonitorRigidbody monitorRigidbody = target as MonitorRigidbody;

			EditorGUILayout.Space();

			// Sample Mode
			MonitorRigidbody.SampleMode newSampleMode = (MonitorRigidbody.SampleMode) EditorGUILayout.EnumPopup("Sample time", monitorRigidbody.sampleMode);
			if (newSampleMode != monitorRigidbody.sampleMode)
			{
				Undo.RecordObject(target, "Change sample time");
				monitorRigidbody.sampleMode = newSampleMode;
			}

			// Position
			bool newShowPosition = EditorGUILayout.ToggleLeft(" Position", monitorRigidbody.showPosition);
			if (newShowPosition != monitorRigidbody.showPosition)
			{
				Undo.RecordObject(target, "Toggle position");
				monitorRigidbody.showPosition = newShowPosition;
			}

			if (monitorRigidbody.showPosition)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");

				bool newShowPosition_x = EditorGUILayout.Toggle(monitorRigidbody.showPosition_x, GUILayout.Width(14));
				if (newShowPosition_x != monitorRigidbody.showPosition_x)
				{
					Undo.RecordObject(target, "Toggle position x");
					monitorRigidbody.showPosition_x = newShowPosition_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowPosition_y = EditorGUILayout.Toggle(monitorRigidbody.showPosition_y, GUILayout.Width(14));
				if (newShowPosition_y != monitorRigidbody.showPosition_y)
				{
					Undo.RecordObject(target, "Toggle position y");
					monitorRigidbody.showPosition_y = newShowPosition_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowPosition_z = EditorGUILayout.Toggle(monitorRigidbody.showPosition_z, GUILayout.Width(14));
				if (newShowPosition_z != monitorRigidbody.showPosition_z)
				{
					Undo.RecordObject(target, "Toggle position z");
					monitorRigidbody.showPosition_z = newShowPosition_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				float newMin, newMax;
				Utils.AxisSettings(monitorRigidbody, ref monitorRigidbody.positionMode, monitorRigidbody.positionMin, out newMin, monitorRigidbody.positionMax, out newMax);
				if (newMin != monitorRigidbody.positionMin)
				{
					monitorRigidbody.positionMin = newMin;
					if (monitorRigidbody.monitor_position != null)
					{
						monitorRigidbody.monitor_position.Min = monitorRigidbody.positionMin;
					}
				}

				if (newMax != monitorRigidbody.positionMax)
				{
					monitorRigidbody.positionMax = newMax;
					if (monitorRigidbody.monitor_position != null)
					{
						monitorRigidbody.monitor_position.Max = monitorRigidbody.positionMax;
					}
				}

				EditorGUILayout.Space();
			}

			// Rotation
			bool newShowRotation = EditorGUILayout.ToggleLeft(" Rotation", monitorRigidbody.showRotation);
			if (newShowRotation != monitorRigidbody.showRotation)
			{
				Undo.RecordObject(target, "Toggle rotation");
				monitorRigidbody.showRotation = newShowRotation;
			}

			if (monitorRigidbody.showRotation)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowRotation_x = EditorGUILayout.Toggle(monitorRigidbody.showRotation_x, GUILayout.Width(14));
				if (newShowRotation_x != monitorRigidbody.showRotation_x)
				{
					Undo.RecordObject(target, "Toggle rotation x");
					monitorRigidbody.showRotation_x = newShowRotation_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowRotation_y = EditorGUILayout.Toggle(monitorRigidbody.showRotation_y, GUILayout.Width(14));
				if (newShowRotation_y != monitorRigidbody.showRotation_y)
				{
					Undo.RecordObject(target, "Toggle rotation y");
					monitorRigidbody.showRotation_y = newShowRotation_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowRotation_z = EditorGUILayout.Toggle(monitorRigidbody.showRotation_z, GUILayout.Width(14));
				if (newShowRotation_z != monitorRigidbody.showRotation_z)
				{
					Undo.RecordObject(target, "Toggle rotation z");
					monitorRigidbody.showRotation_z = newShowRotation_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();
				
				float newMin, newMax;
				Utils.AxisSettings(monitorRigidbody, ref monitorRigidbody.rotationMode, monitorRigidbody.rotationMin, out newMin, monitorRigidbody.rotationMax, out newMax);
				if (newMin != monitorRigidbody.rotationMin)
				{
					monitorRigidbody.rotationMin = newMin;
					if (monitorRigidbody.monitor_rotation != null)
					{
						monitorRigidbody.monitor_rotation.Min = monitorRigidbody.rotationMin;
					}
				}

				if (newMax != monitorRigidbody.rotationMax)
				{
					monitorRigidbody.rotationMax = newMax;
					if (monitorRigidbody.monitor_rotation != null)
					{
						monitorRigidbody.monitor_rotation.Max = monitorRigidbody.rotationMax;
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			// Velocity
			bool newShowVelocity = EditorGUILayout.ToggleLeft(" Velocity", monitorRigidbody.showVelocity);
			if (newShowVelocity != monitorRigidbody.showVelocity)
			{
				Undo.RecordObject(target, "Toggle velocity");
				monitorRigidbody.showVelocity = newShowVelocity;
			}

			if (monitorRigidbody.showVelocity)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PrefixLabel("Fields");
				bool newShowVelocity_x = EditorGUILayout.Toggle(monitorRigidbody.showVelocity_x, GUILayout.Width(14));
				if (newShowVelocity_x != monitorRigidbody.showVelocity_x)
				{
					Undo.RecordObject(target, "Toggle velocity x");
					monitorRigidbody.showVelocity_x = newShowVelocity_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowVelocity_y = EditorGUILayout.Toggle(monitorRigidbody.showVelocity_y, GUILayout.Width(14));
				if (newShowVelocity_y != monitorRigidbody.showVelocity_y)
				{
					Undo.RecordObject(target, "Toggle velocity y");
					monitorRigidbody.showVelocity_y = newShowVelocity_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowVelocity_z = EditorGUILayout.Toggle(monitorRigidbody.showVelocity_z, GUILayout.Width(14));
				if (newShowVelocity_z != monitorRigidbody.showVelocity_z)
				{
					Undo.RecordObject(target, "Toggle velocity z");
					monitorRigidbody.showVelocity_z = newShowVelocity_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));

				EditorGUILayout.EndHorizontal();

				float newMin, newMax;
				Utils.AxisSettings(monitorRigidbody, ref monitorRigidbody.velocityMode, monitorRigidbody.velocityMin, out newMin, monitorRigidbody.velocityMax, out newMax);
				if (newMin != monitorRigidbody.velocityMin)
				{
					monitorRigidbody.velocityMin = newMin;
					if (monitorRigidbody.monitor_velocity != null)
					{
						monitorRigidbody.monitor_velocity.Min = monitorRigidbody.velocityMin;
					}
				}

				if (newMax != monitorRigidbody.velocityMax)
				{
					monitorRigidbody.velocityMax = newMax;
					if (monitorRigidbody.monitor_velocity != null)
					{
						monitorRigidbody.monitor_velocity.Max = monitorRigidbody.velocityMax;
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			// Angular velocity
			bool newShowAngularVelocity = EditorGUILayout.ToggleLeft(" Angular velocity", monitorRigidbody.showAngularVelocity);
			if (newShowAngularVelocity != monitorRigidbody.showAngularVelocity)
			{
				Undo.RecordObject(target, "Toggle angular velocity");
				monitorRigidbody.showAngularVelocity = newShowAngularVelocity;
			}

			if (monitorRigidbody.showAngularVelocity)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PrefixLabel("Fields");
				bool newShowAngularVelocity_x = EditorGUILayout.Toggle(monitorRigidbody.showAngularVelocity_x, GUILayout.Width(14));
				if (newShowAngularVelocity_x != monitorRigidbody.showAngularVelocity_x)
				{
					Undo.RecordObject(target, "Toggle angular velocity x");
					monitorRigidbody.showAngularVelocity_x = newShowAngularVelocity_x;
				}

				GUILayout.Label("x", GUILayout.Width(18));
				bool newShowAngularVelocity_y = EditorGUILayout.Toggle(monitorRigidbody.showAngularVelocity_y, GUILayout.Width(14));
				if (newShowAngularVelocity_y != monitorRigidbody.showAngularVelocity_y)
				{
					Undo.RecordObject(target, "Toggle angular velocity y");
					monitorRigidbody.showAngularVelocity_y = newShowAngularVelocity_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));
				bool newShowAngularVelocity_z = EditorGUILayout.Toggle(monitorRigidbody.showAngularVelocity_z, GUILayout.Width(14));
				if (newShowAngularVelocity_z != monitorRigidbody.showAngularVelocity_z)
				{
					Undo.RecordObject(target, "Toggle angular velocity z");
					monitorRigidbody.showAngularVelocity_z = newShowAngularVelocity_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));

				EditorGUILayout.EndHorizontal();

				float newMin, newMax;
				Utils.AxisSettings(monitorRigidbody, ref monitorRigidbody.angularVelocityMode, monitorRigidbody.angularVelocityMin, out newMin, monitorRigidbody.angularVelocityMax, out newMax);
				if (newMin != monitorRigidbody.angularVelocityMin)
				{
					monitorRigidbody.angularVelocityMin = newMin;
					if (monitorRigidbody.monitor_angularVelocity != null)
					{
						monitorRigidbody.monitor_angularVelocity.Min = monitorRigidbody.angularVelocityMin;
					}
				}

				if (newMax != monitorRigidbody.angularVelocityMax)
				{
					monitorRigidbody.angularVelocityMax = newMax;
					if (monitorRigidbody.monitor_angularVelocity != null)
					{
						monitorRigidbody.monitor_angularVelocity.Max = monitorRigidbody.angularVelocityMax;
					}
				}

				EditorGUILayout.EndVertical();
			}

			Utils.OpenButton(monitorRigidbody.gameObject);

			monitorRigidbody.UpdateMonitors();

			if (GUI.changed)
	            EditorUtility.SetDirty (target);
		}
	}
}