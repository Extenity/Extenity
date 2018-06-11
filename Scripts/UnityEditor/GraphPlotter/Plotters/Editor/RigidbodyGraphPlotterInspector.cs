using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(RigidbodyGraphPlotter))]
	public class RigidbodyGraphPlotterInspector : ExtenityEditorBase<RigidbodyGraphPlotter>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;

			// Try to connect the link automatically
			if (!Me.Rigidbody)
			{
				Undo.RecordObject(Me, "Automatic linking");
				Me.Rigidbody = Me.GetComponent<Rigidbody>();
			}
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(GetProperty("Rigidbody"));

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

				bool newShowPosition_z = EditorGUILayout.Toggle(Me.showPosition_z, GUILayout.Width(14));
				if (newShowPosition_z != Me.showPosition_z)
				{
					Undo.RecordObject(target, "Toggle position z");
					Me.showPosition_z = newShowPosition_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.monitor_position, ref Me.PositionRange);

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

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowRotation_x = EditorGUILayout.Toggle(Me.showRotation_x, GUILayout.Width(14));
				if (newShowRotation_x != Me.showRotation_x)
				{
					Undo.RecordObject(target, "Toggle rotation x");
					Me.showRotation_x = newShowRotation_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowRotation_y = EditorGUILayout.Toggle(Me.showRotation_y, GUILayout.Width(14));
				if (newShowRotation_y != Me.showRotation_y)
				{
					Undo.RecordObject(target, "Toggle rotation y");
					Me.showRotation_y = newShowRotation_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowRotation_z = EditorGUILayout.Toggle(Me.showRotation_z, GUILayout.Width(14));
				if (newShowRotation_z != Me.showRotation_z)
				{
					Undo.RecordObject(target, "Toggle rotation z");
					Me.showRotation_z = newShowRotation_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.monitor_rotation, ref Me.RotationRange);

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

				bool newShowVelocity_z = EditorGUILayout.Toggle(Me.showVelocity_z, GUILayout.Width(14));
				if (newShowVelocity_z != Me.showVelocity_z)
				{
					Undo.RecordObject(target, "Toggle velocity z");
					Me.showVelocity_z = newShowVelocity_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));

				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.monitor_velocity, ref Me.VelocityRange);

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

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PrefixLabel("Fields");
				bool newShowAngularVelocity_x = EditorGUILayout.Toggle(Me.showAngularVelocity_x, GUILayout.Width(14));
				if (newShowAngularVelocity_x != Me.showAngularVelocity_x)
				{
					Undo.RecordObject(target, "Toggle angular velocity x");
					Me.showAngularVelocity_x = newShowAngularVelocity_x;
				}

				GUILayout.Label("x", GUILayout.Width(18));
				bool newShowAngularVelocity_y = EditorGUILayout.Toggle(Me.showAngularVelocity_y, GUILayout.Width(14));
				if (newShowAngularVelocity_y != Me.showAngularVelocity_y)
				{
					Undo.RecordObject(target, "Toggle angular velocity y");
					Me.showAngularVelocity_y = newShowAngularVelocity_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));
				bool newShowAngularVelocity_z = EditorGUILayout.Toggle(Me.showAngularVelocity_z, GUILayout.Width(14));
				if (newShowAngularVelocity_z != Me.showAngularVelocity_z)
				{
					Undo.RecordObject(target, "Toggle angular velocity z");
					Me.showAngularVelocity_z = newShowAngularVelocity_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));

				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.monitor_angularVelocity, ref Me.AngularVelocityRange);

				EditorGUILayout.EndVertical();
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateMonitors();
		}
	}

}