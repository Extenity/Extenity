using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEngine;
using UnityEditor;

namespace Extenity.UIToolbox
{

	[CustomEditor(typeof(SpriteNumberedText))]
	public class SpriteNumberedTextInspector : ExtenityEditorBase<SpriteNumberedText>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		private int Number;

		protected override void OnAfterDefaultInspectorGUI()
		{

			EditorGUILayoutTools.DrawHeader("Value");
			var newNumber = EditorGUILayout.IntField("Set Number", Number);
			if (Number != newNumber)
			{
				Number = newNumber;
				Me.SetNumberedText(Number);
			}

			GUILayout.Space(10f);
		}
	}

}
