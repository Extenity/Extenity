using System;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class MultiSelectionChangedTracker
	{
		public Action SelectionChanged;
		public int RegisterCount { get; private set; }
		public bool IsRegistered { get { return RegisterCount > 0; } }

		public MultiSelectionChangedTracker(Action selectionChanged, bool registerNow, bool callImmediatelly = true)
		{
			SelectionChanged = selectionChanged;

			if (registerNow)
			{
				Register(callImmediatelly);
			}
		}

		public void Register(bool callImmediatelly = true)
		{
			RegisterCount++;
			if (RegisterCount == 1)
			{
				Selection.selectionChanged += InternalSelectionChanged;
			}
			//if (callImmediatelly && IsRegistered) Won't matter if it's registered or not. User wants to call it anyway.
			if (callImmediatelly)
			{
				InternalSelectionChanged();
			}
		}

		public void Deregister(bool callImmediatelly = true)
		{
			RegisterCount--;
			if (RegisterCount == 0)
			{
				Selection.selectionChanged -= InternalSelectionChanged;
			}
			//if (callImmediatelly && IsRegistered) Won't matter if it's registered or not. User wants to call it anyway.
			if (callImmediatelly)
			{
				InternalSelectionChanged();
			}
		}

		public void ForceCallSelectionChanged()
		{
			InternalSelectionChanged();
		}

		private void InternalSelectionChanged()
		{
			if (SelectionChanged != null)
			{
				SelectionChanged();
			}
		}
	}

}
