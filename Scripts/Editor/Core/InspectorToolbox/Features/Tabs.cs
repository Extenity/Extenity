using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Extenity.InspectorToolbox.Editor
{

	/// <summary>Pages of fields and nodes behind a row of tabs. The open tab is remembered for the editor session.</summary>
	public sealed class TabsNode<T> : InspectorNode<T, TabsNode<T>>
	{
		public readonly List<GroupNode<T>> Pages = new List<GroupNode<T>>();

		public override IEnumerable<string> ClaimedFieldNames
		{
			get
			{
				foreach (var page in Pages)
				{
					foreach (var name in page.ClaimedFieldNames)
						yield return name;
				}
			}
		}

		public override VisualElement Build(InspectorInstance instance)
		{
			var tabView = new TabView();
			tabView.AddToClassList("ext-tabs");
			foreach (var page in Pages)
			{
				var tab = new Tab(page.Title);
				tab.Add(page.Build(instance));
				tabView.Add(tab);
			}

			var stateKey = "Extenity.InspectorToolbox.Tab|" + instance.Blueprint.TargetType.FullName + "|" + (Pages.Count > 0 ? Pages[0].Title : "");
			var selected = SessionState.GetInt(stateKey, 0);
			if (selected > 0 && selected < Pages.Count)
				tabView.selectedTabIndex = selected;
			tabView.activeTabChanged += (previous, current) => SessionState.SetInt(stateKey, tabView.selectedTabIndex);
			return tabView;
		}
	}

	/// <summary>Adds the pages of a tab view.</summary>
	public sealed class TabsBuilder<T>
	{
		private readonly InspectorBuilder<T> Builder;
		private readonly TabsNode<T> Node;

		internal TabsBuilder(InspectorBuilder<T> builder, TabsNode<T> node)
		{
			Builder = builder;
			Node = node;
		}

		/// <summary>Adds a page. Its content is declared like the content of a group.</summary>
		public TabsBuilder<T> Tab(string title, Action<GroupBuilder<T>> content)
		{
			if (string.IsNullOrEmpty(title))
				throw new ArgumentException("A tab needs a title.", nameof(title));
			if (content == null)
				throw new ArgumentNullException(nameof(content));
			var page = new GroupNode<T> { Title = title, IsPlain = true };
			Node.Pages.Add(page);
			content(new GroupBuilder<T>(Builder, page));
			return this;
		}
	}

	public static class TabsFeature
	{
		/// <summary>
		/// A tab view. It is drawn where the first field of its pages is declared, or at the bottom when the
		/// pages have no fields, unless the returned node is placed explicitly.
		/// </summary>
		public static TabsNode<T> Tabs<T>(this IInspectorNodeContainer<T> container, Action<TabsBuilder<T>> content)
		{
			if (content == null)
				throw new ArgumentNullException(nameof(content));
			var node = container.Add(new TabsNode<T>());
			content(new TabsBuilder<T>(container.Builder, node));
			if (node.Pages.Count == 0)
				throw new InvalidOperationException("A tab view needs at least one tab.");
			return node;
		}
	}

}
