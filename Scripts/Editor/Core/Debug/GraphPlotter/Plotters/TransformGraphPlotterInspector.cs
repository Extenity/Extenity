using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.DebugToolbox.GraphPlotting.Editor
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
				bool newShowPositionX = EditorGUILayout.Toggle(Me.PlotPositionX, GUILayout.Width(14));
				if (newShowPositionX != Me.PlotPositionX)
				{
					Undo.RecordObject(target, "Toggle position x");
					Me.PlotPositionX = newShowPositionX;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowPositionY = EditorGUILayout.Toggle(Me.PlotPositionY, GUILayout.Width(14));
				if (newShowPositionY != Me.PlotPositionY)
				{
					Undo.RecordObject(target, "Toggle position y");
					Me.PlotPositionY = newShowPositionY;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowPositionZ = EditorGUILayout.Toggle(Me.PlotPositionZ, GUILayout.Width(14));
				if (newShowPositionZ != Me.PlotPositionZ)
				{
					Undo.RecordObject(target, "Toggle position z");
					Me.PlotPositionZ = newShowPositionZ;
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

				CommonEditor.DrawVerticalRangeConfiguration(Me, Me.PositionGraph, ref Me.PositionRange);

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
				bool newShowRotationX = EditorGUILayout.Toggle(Me.PlotRotationX, GUILayout.Width(14));
				if (newShowRotationX != Me.PlotRotationX)
				{
					Undo.RecordObject(target, "Toggle rotation x");
					Me.PlotRotationX = newShowRotationX;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowRotationY = EditorGUILayout.Toggle(Me.PlotRotationY, GUILayout.Width(14));
				if (newShowRotationY != Me.PlotRotationY)
				{
					Undo.RecordObject(target, "Toggle rotation y");
					Me.PlotRotationY = newShowRotationY;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowRotationZ = EditorGUILayout.Toggle(Me.PlotRotationZ, GUILayout.Width(14));
				if (newShowRotationZ != Me.PlotRotationZ)
				{
					Undo.RecordObject(target, "Toggle rotation z");
					Me.PlotRotationZ = newShowRotationZ;
				}

				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				CoordinateSystem newRotationSpace = (CoordinateSystem)EditorGUILayout.EnumPopup("Space", Me.RotationSpace);
				if (newRotationSpace != Me.RotationSpace)
				{
					Undo.RecordObject(target, "Changed rotation space");
					Me.RotationSpace = newRotationSpace;
				}

				CommonEditor.DrawVerticalRangeConfiguration(Me, Me.RotationGraph, ref Me.RotationRange);

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
				bool newShowScaleX = EditorGUILayout.Toggle(Me.PlotScaleX, GUILayout.Width(14));
				if (newShowScaleX != Me.PlotScaleX)
				{
					Undo.RecordObject(target, "Toggle scale x");
					Me.PlotScaleX = newShowScaleX;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowScaleY = EditorGUILayout.Toggle(Me.PlotScaleY, GUILayout.Width(14));
				if (newShowScaleY != Me.PlotScaleY)
				{
					Undo.RecordObject(target, "Toggle scale y");
					Me.PlotScaleY = newShowScaleY;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowScaleZ = EditorGUILayout.Toggle(Me.PlotScaleZ, GUILayout.Width(14));
				if (newShowScaleZ != Me.PlotScaleZ)
				{
					Undo.RecordObject(target, "Toggle scale z");
					Me.PlotScaleZ = newShowScaleZ;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				ScaleCoordinateSystem newScaleSpace = (ScaleCoordinateSystem)EditorGUILayout.EnumPopup("Space", Me.ScaleSpace);
				if (newScaleSpace != Me.ScaleSpace)
				{
					Undo.RecordObject(target, "Change scale space");
					Me.ScaleSpace = newScaleSpace;
				}

				CommonEditor.DrawVerticalRangeConfiguration(Me, Me.ScaleGraph, ref Me.ScaleRange);

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			CommonEditor.OpenGraphPlotterButton(Me.gameObject);

			if (GUI.changed)
				Me.SetupGraph();
		}
	}

}
