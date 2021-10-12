using System;
using System.Linq;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.UIToolbox
{

	[CustomEditor(typeof(SpriteList))]
	public class SpriteListInspector : ExtenityEditorBase<SpriteList>
	{
		protected override void OnEnableDerived()
		{
			if (Me.Sprites.IsNotNullAndEmpty())
			{
				SpriteNames = Me.Sprites.Select(item => item.name).ToArray();
			}

			if (Me.Target != null && Me.Target.sprite)
			{
				SelectedSpriteIndex = Me.Sprites.IndexOf(Me.Target.sprite);
			}
		}

		protected override void OnDisableDerived()
		{
		}

		private string[] SpriteNames;
		private int SelectedSpriteIndex = -1;

		protected override void OnAfterDefaultInspectorGUI()
		{
			GUILayout.Space(20f);

			EditorGUILayoutTools.DrawHeader("Select Sprite For Target Image");

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Clear"))
			{
				Undo.RecordObject(Me, "Clear Sprite");
				Me.ClearSprite(true);
			}

			if (SpriteNames == null)
				SpriteNames = Array.Empty<string>();
			var newSpriteIndex  = EditorGUILayout.Popup(SelectedSpriteIndex, SpriteNames);
			if (SelectedSpriteIndex != newSpriteIndex)
			{
				SelectedSpriteIndex = newSpriteIndex;
				Undo.RecordObject(Me, "Select Sprite");
				Me.SelectSprite(SelectedSpriteIndex, true);
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20f);
		}
	}

}
