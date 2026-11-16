using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>
	/// A popup that lists choices with a search field and submenus. This implementation wraps Unity's advanced
	/// dropdown, which gives the native look. Callers only depend on <see cref="Show"/>.
	/// </summary>
	public static class SearchablePicker
	{
		/// <param name="anchor">The rectangle of the control the popup belongs to, in the coordinates of its window (an element's worldBound).</param>
		/// <param name="onPick">Gets the value of the picked item.</param>
		public static void Show(Rect anchor, IReadOnlyList<DropdownItem<object>> items, Action<object> onPick, string title = null)
		{
			if (items == null)
				throw new ArgumentNullException(nameof(items));
			if (onPick == null)
				throw new ArgumentNullException(nameof(onPick));
			new PickerDropdown(items, onPick, title, anchor.width).Show(anchor);
		}

		private sealed class PickerItem : AdvancedDropdownItem
		{
			public readonly int Index;

			public PickerItem(string name, int index) : base(name)
			{
				Index = index;
			}
		}

		private sealed class PickerDropdown : AdvancedDropdown
		{
			private readonly IReadOnlyList<DropdownItem<object>> Items;
			private readonly Action<object> OnPick;
			private readonly string Title;

			public PickerDropdown(IReadOnlyList<DropdownItem<object>> items, Action<object> onPick, string title, float anchorWidth)
				: base(new AdvancedDropdownState())
			{
				Items = items;
				OnPick = onPick;
				Title = title;
				minimumSize = new Vector2(Mathf.Max(anchorWidth, 240f), 320f);
			}

			protected override AdvancedDropdownItem BuildRoot()
			{
				var root = new AdvancedDropdownItem(string.IsNullOrEmpty(Title) ? "Select" : Title);
				var groups = new Dictionary<string, AdvancedDropdownItem>(StringComparer.Ordinal);
				for (var i = 0; i < Items.Count; i++)
				{
					var parent = root;
					var group = Items[i].Group;
					if (!string.IsNullOrEmpty(group))
					{
						var path = "";
						foreach (var segment in group.Split('/'))
						{
							if (segment.Length == 0)
								continue;
							path += "/" + segment;
							if (!groups.TryGetValue(path, out var groupItem))
							{
								groupItem = new AdvancedDropdownItem(segment);
								parent.AddChild(groupItem);
								groups.Add(path, groupItem);
							}
							parent = groupItem;
						}
					}
					parent.AddChild(new PickerItem(string.IsNullOrEmpty(Items[i].Label) ? "(empty)" : Items[i].Label, i));
				}
				return root;
			}

			protected override void ItemSelected(AdvancedDropdownItem item)
			{
				if (item is PickerItem picked)
					OnPick(Items[picked.Index].Value);
			}
		}
	}

}
