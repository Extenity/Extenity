using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(Rigidbody2DGraphPlotter))]
	public class Rigidbody2DGraphPlotterInspector : ExtenityEditorBase<Rigidbody2DGraphPlotter>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.Space();

			// Position
			bool newShowPosition = EditorGUILayout.ToggleLeft(" Position", Me.showPosition);
			if (newShowPosition != Me.showPosition)
			{
				Undo.RecordObject(target, "Toggle position");
				Me.showPosition = newShowPosition;
			}

			if (Me.showPosition)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowPosition_x = EditorGUILayout.Toggle(Me.showPosition_x, GUILayout.Width(14));
				if (newShowPosition_x != Me.showPosition_x)
				{
					Undo.RecordObject(target, "Toggle position x");
					Me.showPosition_x = newShowPosition_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowPosition_y = EditorGUILayout.Toggle(Me.showPosition_y, GUILayout.Width(14));
				if (newShowPosition_y != Me.showPosition_y)
				{
					Undo.RecordObject(target, "Toggle position y");
					Me.showPosition_y = newShowPosition_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				float newMin, newMax;
				Utils.AxisSettings(Me, ref Me.positionMode, Me.positionMin, out newMin, Me.positionMax, out newMax);
				if (newMin != Me.positionMin)
				{
					Me.positionMin = newMin;
					if (Me.monitor_position != null)
					{
						Me.monitor_position.Min = Me.positionMin;
					}
				}

				if (newMax != Me.positionMax)
				{
					Me.positionMax = newMax;
					if (Me.monitor_position != null)
					{
						Me.monitor_position.Max = Me.positionMax;
					}
				}

				EditorGUILayout.Space();
			}

			// Rotation
			bool newShowRotation = EditorGUILayout.ToggleLeft(" Rotation", Me.showRotation);
			if (newShowRotation != Me.showRotation)
			{
				Undo.RecordObject(target, "Toggle rotation");
				Me.showRotation = newShowRotation;
			}

			if (Me.showRotation)
			{
				EditorGUILayout.BeginVertical();

				bool newRotationClamp = EditorGUILayout.Toggle("Clamp", Me.rotationClamp);
				if (newRotationClamp != Me.rotationClamp)
				{
					Undo.RecordObject(target, "Toggle clamp rotation");
					Me.rotationClamp = newRotationClamp;
				}

				float newMin, newMax;
				Utils.AxisSettings(Me, ref Me.rotationMode, Me.rotationMin, out newMin, Me.rotationMax, out newMax);
				if (newMin != Me.rotationMin)
				{
					Me.rotationMin = newMin;
					if (Me.monitor_rotation != null)
					{
						Me.monitor_rotation.Min = Me.rotationMin;
					}
				}

				if (newMax != Me.rotationMax)
				{
					Me.rotationMax = newMax;
					if (Me.monitor_rotation != null)
					{
						Me.monitor_rotation.Max = Me.rotationMax;
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			// Velocity
			bool newShowVelocity = EditorGUILayout.ToggleLeft(" Velocity", Me.showVelocity);
			if (newShowVelocity != Me.showVelocity)
			{
				Undo.RecordObject(target, "Toggle velocity");
				Me.showVelocity = newShowVelocity;
			}

			if (Me.showVelocity)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PrefixLabel("Fields");
				bool newShowVelocity_x = EditorGUILayout.Toggle(Me.showVelocity_x, GUILayout.Width(14));
				if (newShowVelocity_x != Me.showVelocity_x)
				{
					Undo.RecordObject(target, "Toggle velocity x");
					Me.showVelocity_x = newShowVelocity_x;
				}

				GUILayout.Label("x", GUILayout.Width(18));
				bool newShowVelocity_y = EditorGUILayout.Toggle(Me.showVelocity_y, GUILayout.Width(14));
				if (newShowVelocity_y != Me.showVelocity_y)
				{
					Undo.RecordObject(target, "Toggle velocity y");
					Me.showVelocity_y = newShowVelocity_y;
				}

				GUILayout.Label("y", GUILayout.Width(18));

				EditorGUILayout.EndHorizontal();

				float newMin, newMax;
				Utils.AxisSettings(Me, ref Me.velocityMode, Me.velocityMin, out newMin, Me.velocityMax, out newMax);
				if (newMin != Me.velocityMin)
				{
					Me.velocityMin = newMin;
					if (Me.monitor_velocity != null)
					{
						Me.monitor_velocity.Min = Me.velocityMin;
					}
				}

				if (newMax != Me.velocityMax)
				{
					Me.velocityMax = newMax;
					if (Me.monitor_velocity != null)
					{
						Me.monitor_velocity.Max = Me.velocityMax;
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			// Angular velocity
			bool newShowAngularVelocity = EditorGUILayout.ToggleLeft(" Angular velocity", Me.showAngularVelocity);
			if (newShowAngularVelocity != Me.showAngularVelocity)
			{
				Undo.RecordObject(target, "Toggle angular velocity");
				Me.showAngularVelocity = newShowAngularVelocity;
			}

			if (Me.showAngularVelocity)
			{
				EditorGUILayout.BeginVertical();

				float newMin, newMax;
				Utils.AxisSettings(Me, ref Me.angularVelocityMode, Me.angularVelocityMin, out newMin, Me.angularVelocityMax, out newMax);
				if (newMin != Me.angularVelocityMin)
				{
					Me.angularVelocityMin = newMin;
					if (Me.monitor_angularVelocity != null)
					{
						Me.monitor_angularVelocity.Min = Me.angularVelocityMin;
					}
				}

				if (newMax != Me.angularVelocityMax)
				{
					Me.angularVelocityMax = newMax;
					if (Me.monitor_angularVelocity != null)
					{
						Me.monitor_angularVelocity.Max = Me.angularVelocityMax;
					}
				}


				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			Utils.OpenButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateMonitors();
		}
	}

}