using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.DebugToolbox.GraphPlotting.Editor
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
			var newShowPosition = EditorGUILayout.ToggleLeft(" Position", Me.PlotPosition);
			if (newShowPosition != Me.PlotPosition)
			{
				Undo.RecordObject(target, "Toggle position");
				Me.PlotPosition = newShowPosition;
			}

			if (Me.PlotPosition)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				var newShowPosition_x = EditorGUILayout.Toggle(Me.PlotPositionX, GUILayout.Width(14));
				if (newShowPosition_x != Me.PlotPositionX)
				{
					Undo.RecordObject(target, "Toggle position x");
					Me.PlotPositionX = newShowPosition_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				var newShowPosition_y = EditorGUILayout.Toggle(Me.PlotPositionY, GUILayout.Width(14));
				if (newShowPosition_y != Me.PlotPositionY)
				{
					Undo.RecordObject(target, "Toggle position y");
					Me.PlotPositionY = newShowPosition_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				var newShowPosition_z = EditorGUILayout.Toggle(Me.PlotPositionZ, GUILayout.Width(14));
				if (newShowPosition_z != Me.PlotPositionZ)
				{
					Undo.RecordObject(target, "Toggle position z");
					Me.PlotPositionZ = newShowPosition_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.PositionGraph, ref Me.PositionRange);

				EditorGUILayout.Space();
			}

			// Rotation
			var newShowRotation = EditorGUILayout.ToggleLeft(" Rotation", Me.PlotRotation);
			if (newShowRotation != Me.PlotRotation)
			{
				Undo.RecordObject(target, "Toggle rotation");
				Me.PlotRotation = newShowRotation;
			}

			if (Me.PlotRotation)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				var newShowRotation_x = EditorGUILayout.Toggle(Me.PlotRotationX, GUILayout.Width(14));
				if (newShowRotation_x != Me.PlotRotationX)
				{
					Undo.RecordObject(target, "Toggle rotation x");
					Me.PlotRotationX = newShowRotation_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				var newShowRotation_y = EditorGUILayout.Toggle(Me.PlotRotationY, GUILayout.Width(14));
				if (newShowRotation_y != Me.PlotRotationY)
				{
					Undo.RecordObject(target, "Toggle rotation y");
					Me.PlotRotationY = newShowRotation_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				var newShowRotation_z = EditorGUILayout.Toggle(Me.PlotRotationZ, GUILayout.Width(14));
				if (newShowRotation_z != Me.PlotRotationZ)
				{
					Undo.RecordObject(target, "Toggle rotation z");
					Me.PlotRotationZ = newShowRotation_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.RotationGraph, ref Me.RotationRange);

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			// Velocity
			var newShowVelocity = EditorGUILayout.ToggleLeft(" Velocity", Me.PlotVelocity);
			if (newShowVelocity != Me.PlotVelocity)
			{
				Undo.RecordObject(target, "Toggle velocity");
				Me.PlotVelocity = newShowVelocity;
			}

			if (Me.PlotVelocity)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PrefixLabel("Fields");
				var newShowVelocity_x = EditorGUILayout.Toggle(Me.PlotVelocityX, GUILayout.Width(14));
				if (newShowVelocity_x != Me.PlotVelocityX)
				{
					Undo.RecordObject(target, "Toggle velocity x");
					Me.PlotVelocityX = newShowVelocity_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				var newShowVelocity_y = EditorGUILayout.Toggle(Me.PlotVelocityY, GUILayout.Width(14));
				if (newShowVelocity_y != Me.PlotVelocityY)
				{
					Undo.RecordObject(target, "Toggle velocity y");
					Me.PlotVelocityY = newShowVelocity_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				var newShowVelocity_z = EditorGUILayout.Toggle(Me.PlotVelocityZ, GUILayout.Width(14));
				if (newShowVelocity_z != Me.PlotVelocityZ)
				{
					Undo.RecordObject(target, "Toggle velocity z");
					Me.PlotVelocityZ = newShowVelocity_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));

				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.VelocityGraph, ref Me.VelocityRange);

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			// Angular velocity
			var newShowAngularVelocity = EditorGUILayout.ToggleLeft(" Angular velocity", Me.PlotAngularVelocity);
			if (newShowAngularVelocity != Me.PlotAngularVelocity)
			{
				Undo.RecordObject(target, "Toggle angular velocity");
				Me.PlotAngularVelocity = newShowAngularVelocity;
			}

			if (Me.PlotAngularVelocity)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.PrefixLabel("Fields");
				var newShowAngularVelocity_x = EditorGUILayout.Toggle(Me.PlotAngularVelocityX, GUILayout.Width(14));
				if (newShowAngularVelocity_x != Me.PlotAngularVelocityX)
				{
					Undo.RecordObject(target, "Toggle angular velocity x");
					Me.PlotAngularVelocityX = newShowAngularVelocity_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				var newShowAngularVelocity_y = EditorGUILayout.Toggle(Me.PlotAngularVelocityY, GUILayout.Width(14));
				if (newShowAngularVelocity_y != Me.PlotAngularVelocityY)
				{
					Undo.RecordObject(target, "Toggle angular velocity y");
					Me.PlotAngularVelocityY = newShowAngularVelocity_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				var newShowAngularVelocity_z = EditorGUILayout.Toggle(Me.PlotAngularVelocityZ, GUILayout.Width(14));
				if (newShowAngularVelocity_z != Me.PlotAngularVelocityZ)
				{
					Undo.RecordObject(target, "Toggle angular velocity z");
					Me.PlotAngularVelocityZ = newShowAngularVelocity_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));

				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.AngularVelocityGraph, ref Me.AngularVelocityRange);

				EditorGUILayout.EndVertical();
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.SetupGraph();
		}
	}

}

#endif
