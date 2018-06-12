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

			// Try to connect the link automatically
			if (!Me.Rigidbody2D)
			{
				Undo.RecordObject(Me, "Automatic linking");
				Me.Rigidbody2D = Me.GetComponent<Rigidbody2D>();
			}
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(GetProperty("Rigidbody2D"));

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

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.PositionGraph, ref Me.PositionRange);

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

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.RotationGraph, ref Me.RotationRange);

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

				CommonEditor.DrawAxisRangeConfiguration(Me,Me.VelocityGraph, ref Me.VelocityRange);

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

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.AngularVelocityGraph, ref Me.AngularVelocityRange);

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.UpdateGraph();
		}
	}

}