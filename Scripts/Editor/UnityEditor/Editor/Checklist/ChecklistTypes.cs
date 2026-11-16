using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
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
	public class ChecklistItem
	{
		public string Title;

		[TextArea(3, 30)]
		public string Description;

		public ChecklistItemStatus Status;

		/// <summary>
		/// Allows entering notes, like the user might enter a note to self when a careful inspection is needed.
		/// </summary>
		[TextArea(3, 30)]
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

		#region Search

		public bool IsMatch(string searchString)
		{
			return Contains(Description, searchString) ||
			       Contains(Notes, searchString) ||
			       Contains(Title, searchString);
		}

		internal static bool Contains(string text, string searchString)
		{
			return text != null && text.IndexOf(searchString, StringComparison.InvariantCultureIgnoreCase) >= 0;
		}

		#endregion
	}

	[Serializable]
	public class ChecklistGroup
	{
		public string GroupTitle;

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

		#region Search

		public bool IsMatch(string searchString)
		{
			return ChecklistItem.Contains(GroupTitle, searchString);
		}

		#endregion
	}

	[Serializable]
	public class ChecklistCategory
	{
		public string CategoryTitle;

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

		#region Search

		public bool IsMatch(string searchString)
		{
			return ChecklistItem.Contains(CategoryTitle, searchString);
		}

		#endregion
	}

}
