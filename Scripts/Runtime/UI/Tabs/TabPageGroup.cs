using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UIToolbox
{

	public class TabPageGroup : MonoBehaviour
	{
		#region Initialization

		void Start()
		{
			InitializeTemplate();

			// if (InitializeAtStart)
			// {
			// 	SetState(InitialActivePageIndex, InitialPageCount);
			// }
		}

		#endregion

		#region Template

		[BoxGroup("Setup")]
		public TabPage PageTemplate;

		private void InitializeTemplate()
		{
			PageTemplate.gameObject.SetActive(false);
		}

		public void DestroyTemplate()
		{
			Destroy(PageTemplate.gameObject);
		}

		#endregion

		#region Pages - Create/Destroy

		[FoldoutGroup("Status - Pages", Order = 100)]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(Expanded = true)]
		[NonSerialized]
		public TabPage[] Pages;

		public int PageCount => Pages?.Length ?? 0;

		[FoldoutGroup("Events", Order = 90)]
		public readonly UnityEvent OnPageCountChanged = new UnityEvent();

		private bool PageCountModificationLock;

		public void InitializePages(int pageCount)
		{
			if (PageCountModificationLock)
				throw new Exception("Tried to change page count on an ongoing operation.");
			PageCountModificationLock = true;

			try
			{
				if (pageCount < 0 || pageCount > 50) // 50 is just a hard limit that is not expected.
					throw new ArgumentOutOfRangeException();

				if (Pages?.Length != pageCount)
				{
					// Destroy previously created pages first.
					DestroyAllPages();

					// Create pages.
					Pages = new TabPage[pageCount];
					var parent = PageTemplate.transform.parent;
					for (int i = 0; i < pageCount; i++)
					{
						// Debug.Log("Instantiating tab page " + i);
						Pages[i] = Instantiate(PageTemplate, parent);
						Pages[i].gameObject.name = "TabPage-" + i;
						// Debug.Log("Activating tab page " + i);
						Pages[i].gameObject.SetActive(true);
					}

					OnPageCountChanged.Invoke();
				}
			}
			finally
			{
				PageCountModificationLock = false;
			}

			// Note that we should be processing page switching outside of Page Modification operations above.
			// SwitchToLastActivePageIfCurrentIndexExceedsThePageCount();
		}

		private void DestroyAllPages()
		{
			if (Pages == null)
				return;
			foreach (var page in Pages)
			{
				DestroyImmediate(page.gameObject);
			}
			Pages = null;
		}

		#endregion

		#region Initialize At Start

		// [BoxGroup("Initialize At Start")]
		// public bool InitializeAtStart = true;
		// [BoxGroup("Initialize At Start")]
		// [EnableIf(nameof(InitializeAtStart))]
		// public int InitialActivePageIndex = 0;
		// [BoxGroup("Initialize At Start")]
		// [EnableIf(nameof(InitializeAtStart))]
		// public int InitialPageCount = 1;

		#endregion

		#region Page Switching

		[NonSerialized]
		public int ActiveTabIndex = -1;

		public bool IsAnyTabActive => ActiveTabIndex >= 0;

		private bool TabSwitchingLock;

		[FoldoutGroup("Events")]
		public readonly UnityEvent OnActiveTabChanged = new UnityEvent();

		private void SwitchToLastActiveTabIfCurrentIndexExceedsTheTabCount()
		{
			if (ActiveTabIndex >= PageCount)
			{
				SwitchToTab(PageCount - 1);
			}
		}

		public void SwitchToTab(int tabIndex)
		{
			if (TabSwitchingLock)
				throw new Exception("Tried to change the active tab on an ongoing operation.");
			TabSwitchingLock = true;

			try
			{
				if (ActiveTabIndex != tabIndex)
				{
					ActiveTabIndex = tabIndex;

					for (int i = 0; i < Pages.Length; i++)
					{
						Pages[i].gameObject.SetActive(tabIndex == i);
					}

					OnActiveTabChanged.Invoke();
				}
			}
			finally
			{
				TabSwitchingLock = false;
			}
		}

		#endregion
	}

}
