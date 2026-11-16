using System;
using System.Collections.Generic;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// The inspector of a <see cref="Checklist"/>: its categories, each with its groups and their items, open from
	/// the start. The search above the categories also looks inside groups and items.
	/// </summary>
	[CustomEditor(typeof(Checklist))]
	public sealed class ChecklistInspector : ObjectInspector<Checklist>
	{
		protected override void Configure(InspectorBuilder<Checklist> b)
		{
			b.HideScriptField();
			b.List(x => x.Categories)
			 .ExpandedByDefault()
			 .Searchable(ChecklistSearch.Matches)
			 .CustomAdd(checklist => new ChecklistCategory("") { Groups = new List<ChecklistGroup>() });
		}
	}

	/// <summary>
	/// A category: an icon that tells whether everything in it is completed or skipped, its title and its groups.
	/// </summary>
	[CustomPropertyDrawer(typeof(ChecklistCategory))]
	public sealed class ChecklistCategoryInspector : NestedInspector<ChecklistCategory>
	{
		protected override void Configure(InspectorBuilder<ChecklistCategory> b)
		{
			b.Header(HeaderMode.Inline);
			b.Title(category => category.CategoryTitle);
			b.Row(r =>
			{
				r.Add(ChecklistElements.StatusIcon<ChecklistCategory>(ChecklistStyles.BigIconSize, category => category.CheckIfAllGroupsAreCompletedOrSkipped()));
				r.Field(x => x.CategoryTitle).Label("Category").Grow();
			});
			b.List(x => x.Groups)
			 .ExpandedByDefault()
			 .Searchable(ChecklistSearch.Matches)
			 .CustomAdd(category => new ChecklistGroup("") { Items = new List<ChecklistItem>() });
		}
	}

	/// <summary>
	/// A group: an icon that tells whether all its items are completed or skipped, its title and its items.
	/// </summary>
	[CustomPropertyDrawer(typeof(ChecklistGroup))]
	public sealed class ChecklistGroupInspector : NestedInspector<ChecklistGroup>
	{
		protected override void Configure(InspectorBuilder<ChecklistGroup> b)
		{
			b.Header(HeaderMode.Inline);
			b.Title(group => group.GroupTitle);
			b.Row(r =>
			{
				r.Add(ChecklistElements.StatusIcon<ChecklistGroup>(ChecklistStyles.MidIconSize, group => group.CheckIfAllItemsAreCompletedOrSkipped()));
				r.Field(x => x.GroupTitle).Label("Group").Grow();
			});
			b.List(x => x.Items)
			 .ExpandedByDefault()
			 .Searchable((item, text) => item.IsMatch(text))
			 .CustomAdd(group => new ChecklistItem(""));
		}
	}

	/// <summary>
	/// An item: a summary line (a button that marks it completed, or back to needing attention, its status and
	/// a foldout with its title) and, while the foldout is open, its title, description and notes. An item
	/// without a title always shows its details, so that a new item can be filled in at once.
	/// </summary>
	[CustomPropertyDrawer(typeof(ChecklistItem))]
	public sealed class ChecklistItemInspector : NestedInspector<ChecklistItem>
	{
		internal const string StatusButtonName = "checklist-item-status-button";
		internal const string DetailsFoldoutName = "checklist-item-details-foldout";

		protected override void Configure(InspectorBuilder<ChecklistItem> b)
		{
			b.Header(HeaderMode.Inline);
			b.Title(item => item.Title);
			b.Row(r =>
			{
				r.Add(new CustomNode<ChecklistItem> { Factory = BuildStatusButton });
				r.Field(x => x.Status).HideLabel().Width(120);
				r.Add(new CustomNode<ChecklistItem> { Factory = BuildDetailsFoldout });
			}).AtTop();
			b.Field(x => x.Title).VisibleWhen(AreDetailsShown);
			b.Field(x => x.Description).VisibleWhen(AreDetailsShown);
			b.Field(x => x.Notes).VisibleWhen(AreDetailsShown);
		}

		internal static bool AreDetailsShown(ChecklistItem item, InspectorContext context)
		{
			return string.IsNullOrWhiteSpace(item.Title) || (context.Property?.isExpanded ?? false);
		}

		/// <summary>The status that a click on the status button gives an item.</summary>
		internal static ChecklistItemStatus GetToggledStatus(ChecklistItemStatus status)
		{
			return status == ChecklistItemStatus.Completed
				? ChecklistItemStatus.NeedsAttention
				: ChecklistItemStatus.Completed;
		}

		private static VisualElement BuildStatusButton(InspectorInstance instance)
		{
			var icon = new Image { pickingMode = PickingMode.Ignore };
			icon.style.width = ChecklistStyles.SmallIconSize - 4;
			icon.style.height = ChecklistStyles.SmallIconSize - 4;

			var button = new Button { name = StatusButtonName, tooltip = "Mark as completed, or as needing attention when it is completed" };
			button.style.width = ChecklistStyles.SmallIconSize;
			button.style.height = ChecklistStyles.SmallIconSize;
			button.style.paddingLeft = button.style.paddingRight = button.style.paddingTop = button.style.paddingBottom = 1;
			button.style.marginLeft = 0;
			button.style.flexShrink = 0;
			button.style.alignItems = Align.Center;
			button.style.justifyContent = Justify.Center;
			button.Add(icon);
			button.clicked += () =>
			{
				var property = instance.FindField(nameof(ChecklistItem.Status));
				var status = (ChecklistItemStatus)property.intValue;
				property.intValue = (int)GetToggledStatus(status);
				property.serializedObject.ApplyModifiedProperties();
			};

			instance.OnEvaluate(pass =>
			{
				if (pass.SingleTarget is ChecklistItem item)
					icon.image = ChecklistElements.GetIcon(item.CheckIfCompletedOrSkipped());
			});
			icon.image = ChecklistElements.GetIcon(IsCompletedOrSkipped((ChecklistItemStatus)instance.FindField(nameof(ChecklistItem.Status)).intValue));
			return button;
		}

		private static bool IsCompletedOrSkipped(ChecklistItemStatus status)
		{
			return status == ChecklistItemStatus.Completed || status == ChecklistItemStatus.Skipped;
		}

		private static VisualElement BuildDetailsFoldout(InspectorInstance instance)
		{
			var property = instance.Context.Property
			               ?? throw new InvalidOperationException($"A checklist item must be drawn from a property, not from '{instance.Context.RootObject}' itself.");
			var foldout = new Foldout { name = DetailsFoldoutName };
			foldout.SetValueWithoutNotify(property.isExpanded);
			foldout.text = GetFoldoutText(instance.FindField(nameof(ChecklistItem.Title)).stringValue);
			foldout.style.flexGrow = 1;
			foldout.style.marginLeft = 6;
			foldout.RegisterValueChangedCallback(change =>
			{
				if (change.target != foldout)
					return;
				var current = instance.Context.Property;
				if (current == null)
					return; // The item was removed; its row goes away with the next rebuild of the list.
				current.isExpanded = change.newValue;
				instance.Reevaluate();
			});
			instance.OnEvaluate(pass =>
			{
				if (pass.SingleTarget is ChecklistItem item)
					foldout.text = GetFoldoutText(item.Title);
				var current = instance.Context.Property;
				if (current != null)
					foldout.SetValueWithoutNotify(current.isExpanded);
			});
			return foldout;
		}

		private static string GetFoldoutText(string title)
		{
			return string.IsNullOrWhiteSpace(title) ? "(No title)" : title;
		}
	}

	internal static class ChecklistElements
	{
		internal const string StatusIconName = "checklist-status-icon";

		internal static Texture2D GetIcon(bool isCompletedOrSkipped)
		{
			return isCompletedOrSkipped
				? ChecklistIcons.Texture_Accept
				: ChecklistIcons.Texture_Reject;
		}

		/// <summary>An icon that follows the completion of the inspected value.</summary>
		internal static CustomNode<T> StatusIcon<T>(int size, Func<T, bool> isCompletedOrSkipped)
		{
			return new CustomNode<T>
			{
				Factory = instance =>
				{
					var icon = new Image { name = StatusIconName, scaleMode = ScaleMode.ScaleToFit };
					icon.style.width = size;
					icon.style.height = size;
					icon.style.flexShrink = 0;
					icon.style.marginRight = 6;
					icon.style.alignSelf = Align.Center;
					instance.OnEvaluate(pass =>
					{
						if (pass.SingleTarget is T value)
						{
							var isDone = isCompletedOrSkipped(value);
							icon.image = GetIcon(isDone);
							icon.tooltip = isDone ? "Everything is completed or skipped." : "Some items are not completed yet.";
						}
					});
					return icon;
				},
			};
		}
	}

	/// <summary>
	/// The search of a checklist: a category or group matches when its title, or anything inside it, contains the
	/// text.
	/// </summary>
	internal static class ChecklistSearch
	{
		internal static bool Matches(ChecklistCategory category, string text)
		{
			if (category.IsMatch(text))
				return true;
			if (category.Groups != null)
			{
				foreach (var group in category.Groups)
				{
					if (Matches(group, text))
						return true;
				}
			}
			return false;
		}

		internal static bool Matches(ChecklistGroup group, string text)
		{
			if (group.IsMatch(text))
				return true;
			if (group.Items != null)
			{
				foreach (var item in group.Items)
				{
					if (item.IsMatch(text))
						return true;
				}
			}
			return false;
		}
	}

}
