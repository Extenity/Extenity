using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.UIToolbox
{

	public class PageGroup : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
			InitializeTemplate();
			InitializePageSwitching();
		}

		protected void Start()
		{
			if (InitializeAtStart)
			{
				SetState(InitialActivePageIndex, InitialPageCount);
			}
		}

		#endregion

		#region Deinitialization

		private void OnDestroy()
		{
			DeinitializePageSwitching();
		}

		#endregion

		#region Template

		[BoxGroup("Setup")]
		public Page PageTemplate;

		private void InitializeTemplate()
		{
			PageTemplate.gameObject.SetActive(false);
		}

		public void DestroyTemplate()
		{
			if (PageTemplate != null)
			{
				Destroy(PageTemplate.gameObject);
			}

			if (PageIndicatorGroup)
			{
				PageIndicatorGroup.DestroyTemplate();
			}
		}

		#endregion

		#region Pages - Create/Destroy

		[FoldoutGroup("Status - Pages", Order = 100)]
		[ShowInInspector, ReadOnly]
		[ListDrawerSettings(Expanded = true)]
		[NonSerialized]
		public Page[] Pages;

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
					Pages = new Page[pageCount];
					var parent = PageTemplate.transform.parent;
					for (int i = 0; i < pageCount; i++)
					{
						Pages[i] = Instantiate(PageTemplate, parent);
						Pages[i].gameObject.name = "Page-" + i;
						Pages[i].gameObject.SetActive(true);
					}

					// Refresh horizontal snap children list
					{
						// HorizontalScrollSnap.ChildObjects = new GameObject[pageCount];
						// for (int i = 0; i < pageCount; i++)
						// {
						// 	HorizontalScrollSnap.ChildObjects[i] = Pages[i].gameObject;
						// }
						// HorizontalScrollSnap.enabled = true;

						// Just let it reset itself.
						HorizontalScrollSnap.ChildObjects = Array.Empty<GameObject>();
					}

					OnPageCountChanged.Invoke();
				}
			}
			finally
			{
				PageCountModificationLock = false;
			}

			// Note that we should be processing page switching outside of Page Modification operations above.
			SwitchToLastActivePageIfCurrentIndexExceedsThePageCount();
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

		[BoxGroup("Initialize At Start")]
		public bool InitializeAtStart = true;
		[BoxGroup("Initialize At Start")]
		[EnableIf(nameof(InitializeAtStart))]
		public int InitialActivePageIndex = 0;
		[BoxGroup("Initialize At Start")]
		[EnableIf(nameof(InitializeAtStart))]
		public int InitialPageCount = 1;

		#endregion

		#region Page Switching

		[BoxGroup("Setup")]
		public HorizontalScrollSnap HorizontalScrollSnap;

		[NonSerialized]
		public int ActivePageIndex = -1;

		public bool IsAnyPageActive => ActivePageIndex >= 0;

		private bool PageSwitchingLock;

		[FoldoutGroup("Events")]
		public readonly UnityEvent OnActivePageChanged = new UnityEvent();

		private void InitializePageSwitching()
		{
			HorizontalScrollSnap.OnSelectionPageChangedEvent.AddListener(OnScrollSnapPageChanged);
		}

		private void DeinitializePageSwitching()
		{
			HorizontalScrollSnap.OnSelectionPageChangedEvent.RemoveListener(OnScrollSnapPageChanged);
		}

		private void SwitchToLastActivePageIfCurrentIndexExceedsThePageCount()
		{
			if (ActivePageIndex >= PageCount)
			{
				SetState(PageCount - 1, PageCount);
			}
		}

		private void OnScrollSnapPageChanged(int pageIndex)
		{
			SetState(pageIndex, PageCount);
		}

		public void SetState(int activePageIndex, int pageCount)
		{
			if (PageSwitchingLock)
				throw new Exception("Tried to change the active page on an ongoing operation.");
			PageSwitchingLock = true;

			try
			{
				InitializePages(pageCount);

				if (ActivePageIndex != activePageIndex)
				{
					ActivePageIndex = activePageIndex;

					UpdatePageIndicatorState();

					OnActivePageChanged.Invoke();
				}
			}
			finally
			{
				PageSwitchingLock = false;
			}
		}

		#endregion

		#region Page Indicator

		[BoxGroup("Setup")]
		public PageIndicatorGroup PageIndicatorGroup;

		private void UpdatePageIndicatorState()
		{
			if (PageIndicatorGroup)
			{
				PageIndicatorGroup.SetState(ActivePageIndex, PageCount);
			}
		}

		#endregion
	}

}
