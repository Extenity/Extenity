using System.Collections.Generic;
using Extenity.ProfilingToolbox;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class CodeProfilerUI : TreeView<CodeProfilerEntry>
	{
		#region Initialization

		protected override void OnEnable()
		{
			CodeProfiler.RequestAcivation();
			InitializeItems();
		}

		#endregion

		#region Deinitialization

		protected override void OnDisable()
		{
			CodeProfiler.ReleaseAcivation();
			DeinitializeItems();
		}

		#endregion

		#region Update

		protected void LateUpdate()
		{
			// Iterate over all entry UIs and make them refresh their values. No need to iterate over entry data, since a UI item representation
			// should be created by now for each entry.
			for (var i = 0; i < AllEntries.Count; i++)
			{
				AllEntries[i].RefreshValues();
			}
		}

		#endregion

		#region Items

		[Header("Code Profiler")]
		public CodeProfilerEntryUI EntryTemplate;

		private readonly List<CodeProfilerEntryUI> AllEntries = new List<CodeProfilerEntryUI>();

		private void InitializeItems()
		{
			EntryTemplate.gameObject.SetActive(false);

			CreateUIsForAlreadyExistingItems();
			CodeProfiler.OnEntryCreated.AddListener(CreateUIForItem);
		}

		private void DeinitializeItems()
		{
			CodeProfiler.OnEntryCreated.RemoveListener(CreateUIForItem);
			ClearItems();
		}

		private void CreateUIsForAlreadyExistingItems()
		{
			CodeProfiler.ForeachAllEntries(CreateUIForItem);
		}

		private void CreateUIForItem(CodeProfilerEntry entry)
		{
			var parentNode = GetNode(entry.Parent);
			var node = AddNode(entry, parentNode);
			AllEntries.Add((CodeProfilerEntryUI)node.Component);
		}

		private void ClearItems()
		{
			foreach (var entry in AllEntries)
			{
				Destroy(entry.gameObject);
			}
			AllEntries.Clear();
		}

		private Node GetNode(CodeProfilerEntry entry)
		{
			// Special case for root
			if (entry == CodeProfiler.BaseEntry)
			{
				return RootNode;
			}

			for (var i = 0; i < AllEntries.Count; i++)
			{
				var entryUI = AllEntries[i];
				if (entryUI.Node.Data == entry)
				{
					return entryUI.Node;
				}
			}
			return null;
		}

		#endregion
	}

}
