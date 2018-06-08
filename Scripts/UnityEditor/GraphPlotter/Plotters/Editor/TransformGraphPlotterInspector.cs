using UnityEngine;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	[CustomEditor(typeof(TransformGraphPlotter))]
	public class TransformGraphPlotterInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			var Me = target as TransformGraphPlotter;

			EditorGUILayout.Space();

			// Position
			bool newShowPosition = EditorGUILayout.ToggleLeft("Position", Me.showPosition);
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

				EditorGUILayout.BeginHorizontal();
				TransformGraphPlotter.Space newPositionSpace = (TransformGraphPlotter.Space)EditorGUILayout.EnumPopup("Space", Me.positionSpace);
				if (newPositionSpace != Me.positionSpace)
				{
					Undo.RecordObject(target, "Changed position space");
					Me.positionSpace = newPositionSpace;
				}
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
			bool newShowRotation = EditorGUILayout.ToggleLeft("Rotation", Me.showRotation);
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

				float newMin, newMax;
				TransformGraphPlotter.Space newRotationSpace = (TransformGraphPlotter.Space)EditorGUILayout.EnumPopup("Space", Me.rotationSpace);
				if (newRotationSpace != Me.rotationSpace)
				{
					Undo.RecordObject(target, "Changed rotation space");
					Me.rotationSpace = newRotationSpace;
				}

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


			// Scale
			bool newShowScale = EditorGUILayout.ToggleLeft("Scale", Me.showScale);
			if (newShowScale != Me.showScale)
			{
				Undo.RecordObject(target, "Toggle scale");
				Me.showScale = newShowScale;
			}

			if (Me.showScale)
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Fields");
				bool newShowScale_x = EditorGUILayout.Toggle(Me.showScale_x, GUILayout.Width(14));
				if (newShowScale_x != Me.showScale_x)
				{
					Undo.RecordObject(target, "Toggle scale x");
					Me.showScale_x = newShowScale_x;
				}
				GUILayout.Label("x", GUILayout.Width(18));

				bool newShowScale_y = EditorGUILayout.Toggle(Me.showScale_y, GUILayout.Width(14));
				if (newShowScale_y != Me.showScale_y)
				{
					Undo.RecordObject(target, "Toggle scale y");
					Me.showScale_y = newShowScale_y;
				}
				GUILayout.Label("y", GUILayout.Width(18));

				bool newShowScale_z = EditorGUILayout.Toggle(Me.showScale_z, GUILayout.Width(14));
				if (newShowScale_z != Me.showScale_z)
				{
					Undo.RecordObject(target, "Toggle scale z");
					Me.showScale_z = newShowScale_z;
				}
				GUILayout.Label("z", GUILayout.Width(18));
				EditorGUILayout.EndHorizontal();

				TransformGraphPlotter.ScaleSpace newScaleSpace = (TransformGraphPlotter.ScaleSpace)EditorGUILayout.EnumPopup("Space", Me.scaleSpace);
				if (newScaleSpace != Me.scaleSpace)
				{
					Undo.RecordObject(target, "Change scale space");
					Me.scaleSpace = newScaleSpace;
				}

				float newMin, newMax;
				Utils.AxisSettings(Me, ref Me.scaleMode, Me.scaleMin, out newMin, Me.scaleMax, out newMax);
				if (newMin != Me.scaleMin)
				{
					Me.scaleMin = newMin;
					if (Me.monitor_scale != null)
					{
						Me.monitor_scale.Min = Me.scaleMin;
					}
				}

				if (newMax != Me.scaleMax)
				{
					Me.scaleMax = newMax;
					if (Me.monitor_scale != null)
					{
						Me.monitor_scale.Max = Me.scaleMax;
					}
				}

				EditorGUILayout.EndVertical();

				EditorGUILayout.Space();
			}

			EditorGUILayout.Space();

			Utils.OpenButton(Me.gameObject);

			Me.UpdateMonitors();

			if (GUI.changed)
				EditorUtility.SetDirty(target);
		}
	}

}
