using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public enum ChecklistItemStatus
	{
		NotStarted = 0,
		InProgress = 1,
		NeedsAttention = 2,
		Completed = 3,
		Skipped = 4,
	}

	[Serializable]
	public class ChecklistItem : ISearchFilterable
	{
		[HideInInspector]
		public string Title;

		[HideInInspector]
		public string Description;

		[HideInInspector]
		public ChecklistItemStatus Status;

		/// <summary>
		/// Allows entering notes, like the user might enter a note to self when a careful inspection is needed.
		/// </summary>
		[HideInInspector]
		public string Notes;

		#region Initialization

		public ChecklistItem(string title)
		{
			Title = title;
		}

		public ChecklistItem(string title, string description)
		{
			Title = title;
			Description = description;
		}

		#endregion

		#region Check If Completed

		public bool CheckIfCompleted()
		{
			return Status == ChecklistItemStatus.Completed;
		}

		public bool CheckIfCompletedOrSkipped()
		{
			return CheckIfCompleted() || Status == ChecklistItemStatus.Skipped;
		}

		#endregion

		#region Editor

		[NonSerialized]
		private bool IsFoldout;

		[OnInspectorGUI]
		private void DrawStatusIcon()
		{
			GUILayout.BeginVertical();

			// Summary line
			GUILayout.BeginHorizontal();
			{
				// Icon
				{
					var icon = CheckIfCompletedOrSkipped()
						? ChecklistIcons.Texture_Accept
						: ChecklistIcons.Texture_Reject;
					if (GUILayout.Button(icon, GUI.skin.GetStyle("Label"), ChecklistConstants.SmallIconLayoutOptions))
					{
						IsFoldout = !IsFoldout;
					}
				}

				// Status
				{
					var rect = GUILayoutUtility.GetRect(112, ChecklistConstants.SmallIconSize, GUILayout.Width(112));
					rect.y += 2;
					Status = EnumSelector<ChecklistItemStatus>.DrawEnumField(rect, GUIContent.none, Status, null);
				}

				// Foldout title
				{
					IsFoldout = SirenixEditorGUI.Foldout(IsFoldout, Title);
				}
			}
			GUILayout.EndHorizontal();

			// Details
			if (string.IsNullOrWhiteSpace(Title))
			{
				// Enforce opening the details when first adding an item.
				IsFoldout = true;
			}
			if (IsFoldout)
			{
				GUILayout.Space(6f);
				GUILayout.BeginHorizontal();
				GUILayout.Space(ChecklistConstants.SmallIconSize + 8);
				SirenixEditorGUI.BeginBox();
				GUILayout.BeginVertical();
				GUILayout.Space(6f);

				// Title
				{
					GUILayout.Label(nameof(Title));
					Title = GUILayout.TextField(Title);
				}

				GUILayout.Space(6f);

				// Description
				{
					GUILayout.Label(nameof(Description));
					var lineCount = Mathf.Max(3, Description.CountLines());
					var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * lineCount);
					Description = EditorGUI.TextArea(rect, Description, EditorStyles.textArea);
				}

				GUILayout.Space(6f);

				// Notes
				{
					GUILayout.Label(nameof(Notes));
					var lineCount = Mathf.Max(3, Notes.CountLines());
					var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * lineCount);
					Notes = EditorGUI.TextArea(rect, Notes, EditorStyles.textArea);
				}

				GUILayout.Space(6f);
				GUILayout.EndVertical();
				SirenixEditorGUI.EndBox();
				GUILayout.EndHorizontal();
			}

			GUILayout.EndVertical();
		}

		public bool IsMatch(string searchString)
		{
			return Description.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
			       Notes.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0 ||
			       Title.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		#endregion
	}

	[Serializable]
	public class ChecklistGroup : ISearchFilterable
	{
		[Title("Group", HorizontalLine = false)]
		[HorizontalGroup("Title")]
		[HideLabel, PropertySpace(SpaceBefore = 4)]
		public string GroupTitle;

		[ListDrawerSettings(Expanded = true), PropertySpace(SpaceAfter = 20)]
		public List<ChecklistItem> Items;

		#region Initialization

		public ChecklistGroup(string title)
		{
			GroupTitle = title;
		}

		#endregion

		#region Check If Completed

		public bool CheckIfAllItemsAreCompleted()
		{
			foreach (var item in Items)
			{
				if (!item.CheckIfCompleted())
				{
					return false;
				}
			}
			return true;
		}

		public bool CheckIfAllItemsAreCompletedOrSkipped()
		{
			foreach (var item in Items)
			{
				if (!item.CheckIfCompletedOrSkipped())
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Editor

		[OnInspectorGUI, PropertySpace(SpaceBefore = 18)]
		[HorizontalGroup("Title", Width = ChecklistConstants.MidIconSize), PropertyOrder(-1)]
		private void _DrawIcon()
		{
			var icon = CheckIfAllItemsAreCompletedOrSkipped()
				? ChecklistIcons.Texture_Accept
				: ChecklistIcons.Texture_Reject;
			GUILayout.Label(icon, ChecklistConstants.MidIconLayoutOptions);
		}

		public bool IsMatch(string searchString)
		{
			return GroupTitle.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		#endregion
	}

	[Serializable]
	public class ChecklistCategory : ISearchFilterable
	{
		[Title("Category", HorizontalLine = false)]
		[HorizontalGroup("Title")]
		[HideLabel, PropertySpace(SpaceBefore = 4)]
		public string CategoryTitle;

		[ListDrawerSettings(Expanded = true), PropertySpace(SpaceAfter = 20)]
		public List<ChecklistGroup> Groups;

		#region Initialization

		public ChecklistCategory(string title)
		{
			CategoryTitle = title;
		}

		#endregion

		#region Check If Completed

		public bool CheckIfAllGroupsAreCompleted()
		{
			foreach (var group in Groups)
			{
				if (!group.CheckIfAllItemsAreCompleted())
				{
					return false;
				}
			}
			return true;
		}

		public bool CheckIfAllGroupsAreCompletedOrSkipped()
		{
			foreach (var group in Groups)
			{
				if (!group.CheckIfAllItemsAreCompletedOrSkipped())
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region Editor

		[OnInspectorGUI, PropertySpace(SpaceBefore = 12)]
		[HorizontalGroup("Title", Width = ChecklistConstants.BigIconSize), PropertyOrder(-1)]
		private void _DrawIcon()
		{
			var icon = CheckIfAllGroupsAreCompletedOrSkipped()
				? ChecklistIcons.Texture_Accept
				: ChecklistIcons.Texture_Reject;
			GUILayout.Label(icon, ChecklistConstants.BigIconLayoutOptions);
		}

		public bool IsMatch(string searchString)
		{
			return CategoryTitle.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		#endregion
	}

}
