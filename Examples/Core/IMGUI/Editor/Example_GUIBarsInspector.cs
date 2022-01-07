using Extenity.IMGUIToolbox.Editor;
using UnityEditor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;

namespace ExtenityExamples.IMGUIToolbox
{

	[CustomEditor(typeof(Example_GUIBars))]
	public class Example_GUIBarsInspector : ExtenityEditorBase<Example_GUIBars>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(50f);
			EditorGUILayoutTools.DrawHeader("Inspector Drawing Example");
			GUILayout.Label("Also hit Play button to see ingame example.");

			GUILayout.Space(10f);

			var width = GUILayoutUtility.GetLastRect().width;
			GUILayoutTools.Bars(width, Me.EditorHeight, 5f, true, Me.BarColorScale, Me.BarBackgroundColorScale, Me.BarValues.Length, index => Me.BarValues[index]);
		}
	}

}
