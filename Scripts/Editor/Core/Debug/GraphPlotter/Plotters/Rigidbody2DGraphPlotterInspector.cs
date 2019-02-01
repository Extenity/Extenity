#if !DisablePhysics2D
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.DebugToolbox.GraphPlotting.Editor
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

				var newRotationClamp = EditorGUILayout.Toggle("Clamp", Me.ClampRotation);
				if (newRotationClamp != Me.ClampRotation)
				{
					Undo.RecordObject(target, "Toggle rotation clamp");
					Me.ClampRotation = newRotationClamp;
				}

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

				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me,Me.VelocityGraph, ref Me.VelocityRange);

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

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.AngularVelocityGraph, ref Me.AngularVelocityRange);

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.SetupGraph();
		}
	}

}
#endif
