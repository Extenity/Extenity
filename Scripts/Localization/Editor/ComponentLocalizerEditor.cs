using UnityEditor;
using UnityEngine;
using System.Collections;

namespace Extenity
{

	[CustomEditor(typeof(ComponentLocalizer))]
	public class ComponentLocalizerEditor : Editor
	{
		private SerializedObject Configuration;
		//private ComponentLocalizer Me;

		//private SerializedProperty KeyProperty;

		private void OnEnable()
		{
			//Me = (ComponentLocalizer)target;
			Configuration = new SerializedObject(target);

			//KeyProperty = Configuration.FindProperty("Key");
		}

		public override void OnInspectorGUI()
		{
			Configuration.Update();
			EditorGUI.BeginDisabledGroup(Application.isPlaying);

			DrawDefaultInspector();

			//EditorGUIUtility.LookLikeControls();
			//EditorGUILayout.PropertyField(KeyProperty);

			EditorGUI.EndDisabledGroup();
			Configuration.ApplyModifiedProperties();
		}
	}

}
