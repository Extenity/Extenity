using System;
using UnityEditor;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class MultiSelectionChangedTracker
	{
		public Action SelectionChanged;
		public int RegisterCount { get; private set; }
		public bool IsRegistered { get { return RegisterCount > 0; } }

		public MultiSelectionChangedTracker(Action selectionChanged, bool registerNow, bool callImmediately = true)
		{
			SelectionChanged = selectionChanged;

			if (registerNow)
			{
				Register(callImmediately);
			}
		}

		public void Register(bool callImmediately = true)
		{
			RegisterCount++;
			if (RegisterCount == 1)
			{
				Selection.selectionChanged += InternalSelectionChanged;
			}
			//if (callImmediately && IsRegistered) Won't matter if it's registered or not. User wants to call it anyway.
			if (callImmediately)
			{
				InternalSelectionChanged();
			}
		}

		public void Deregister(bool callImmediately = true)
		{
			RegisterCount--;
			if (RegisterCount == 0)
			{
				Selection.selectionChanged -= InternalSelectionChanged;
			}
			//if (callImmediately && IsRegistered) Won't matter if it's registered or not. User wants to call it anyway.
			if (callImmediately)
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
