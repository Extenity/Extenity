using System;
using System.Collections.Generic;
using Extenity.ProfilingToolbox;

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
			throw new NotImplementedException();
		}

		#endregion

		#region Items

		public CodeProfilerEntryUI EntryTemplate;

		private List<CodeProfilerEntryUI> AllEntries;

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
			AddNode(entry, entry.Parent);
		}

		private void ClearItems()
		{
			foreach (var entry in AllEntries)
			{
				Destroy(entry.gameObject);
			}
			AllEntries.Clear();
		}

		#endregion
	}

}
