using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(TransformGraphPlotter))]
	public class TransformGraphPlotterInspector : ExtenityEditorBase<TransformGraphPlotter>
	{
		protected override void OnEnableDerived()
		{
			IsDefaultInspectorDrawingEnabled = false;

			// Try to connect the link automatically
			if (!Me.Transform)
			{
				Undo.RecordObject(Me, "Automatic linking");
				Me.Transform = Me.GetComponent<Transform>();
			}
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(GetProperty("Transform"));

			EditorGUILayout.Space();

			// Position
			bool newShowPosition = EditorGUILayout.ToggleLeft("Position", Me.PlotPosition);
			if (newShowPosition != Me.PlotPosition)
			{
				Undo.RecordObject(target, "Toggle position");
				Me.PlotPosition = newShowPosition;
			}

			if (Me.PlotPosition)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowPosition_x = EditorGUILayout.Toggle(Me.PlotPositionX, GUILayout.Width(14));
				if (newShowPosition_x != Me.PlotPositionX)
				{
					Undo.RecordObject(target, "Toggle position x");
					Me.PlotPositionX = newShowPosition_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowPosition_y = EditorGUILayout.Toggle(Me.PlotPositionY, GUILayout.Width(14));
				if (newShowPosition_y != Me.PlotPositionY)
				{
					Undo.RecordObject(target, "Toggle position y");
					Me.PlotPositionY = newShowPosition_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowPosition_z = EditorGUILayout.Toggle(Me.PlotPositionZ, GUILayout.Width(14));
				if (newShowPosition_z != Me.PlotPositionZ)
				{
					Undo.RecordObject(target, "Toggle position z");
					Me.PlotPositionZ = newShowPosition_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				CoordinateSystem newPositionSpace = (CoordinateSystem)EditorGUILayout.EnumPopup("Space", Me.PositionSpace);
				if (newPositionSpace != Me.PositionSpace)
				{
					Undo.RecordObject(target, "Changed position space");
					Me.PositionSpace = newPositionSpace;
				}
				EditorGUILayout.EndHorizontal();

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.PositionGraph, ref Me.PositionRange);

				EditorGUILayout.Space();
			}

			// Rotation
			bool newShowRotation = EditorGUILayout.ToggleLeft("Rotation", Me.PlotRotation);
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
				bool newShowRotation_x = EditorGUILayout.Toggle(Me.PlotRotationX, GUILayout.Width(14));
				if (newShowRotation_x != Me.PlotRotationX)
				{
					Undo.RecordObject(target, "Toggle rotation x");
					Me.PlotRotationX = newShowRotation_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowRotation_y = EditorGUILayout.Toggle(Me.PlotRotationY, GUILayout.Width(14));
				if (newShowRotation_y != Me.PlotRotationY)
				{
					Undo.RecordObject(target, "Toggle rotation y");
					Me.PlotRotationY = newShowRotation_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowRotation_z = EditorGUILayout.Toggle(Me.PlotRotationZ, GUILayout.Width(14));
				if (newShowRotation_z != Me.PlotRotationZ)
				{
					Undo.RecordObject(target, "Toggle rotation z");
					Me.PlotRotationZ = newShowRotation_z;
				}

				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				CoordinateSystem newRotationSpace = (CoordinateSystem)EditorGUILayout.EnumPopup("Space", Me.RotationSpace);
				if (newRotationSpace != Me.RotationSpace)
				{
					Undo.RecordObject(target, "Changed rotation space");
					Me.RotationSpace = newRotationSpace;
				}

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.RotationGraph, ref Me.RotationRange);

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}


			// Scale
			bool newShowScale = EditorGUILayout.ToggleLeft("Scale", Me.PlotScale);
			if (newShowScale != Me.PlotScale)
			{
				Undo.RecordObject(target, "Toggle scale");
				Me.PlotScale = newShowScale;
			}

			if (Me.PlotScale)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowScale_x = EditorGUILayout.Toggle(Me.PlotScaleX, GUILayout.Width(14));
				if (newShowScale_x != Me.PlotScaleX)
				{
					Undo.RecordObject(target, "Toggle scale x");
					Me.PlotScaleX = newShowScale_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowScale_y = EditorGUILayout.Toggle(Me.PlotScaleY, GUILayout.Width(14));
				if (newShowScale_y != Me.PlotScaleY)
				{
					Undo.RecordObject(target, "Toggle scale y");
					Me.PlotScaleY = newShowScale_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowScale_z = EditorGUILayout.Toggle(Me.PlotScaleZ, GUILayout.Width(14));
				if (newShowScale_z != Me.PlotScaleZ)
				{
					Undo.RecordObject(target, "Toggle scale z");
					Me.PlotScaleZ = newShowScale_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				ScaleCoordinateSystem newScaleSpace = (ScaleCoordinateSystem)EditorGUILayout.EnumPopup("Space", Me.ScaleSpace);
				if (newScaleSpace != Me.ScaleSpace)
				{
					Undo.RecordObject(target, "Change scale space");
					Me.ScaleSpace = newScaleSpace;
				}

				CommonEditor.DrawAxisRangeConfiguration(Me, Me.ScaleGraph, ref Me.ScaleRange);

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.SetupGraph();
		}
	}

}
