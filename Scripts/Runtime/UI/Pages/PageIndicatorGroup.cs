using System;
using UnityEngine;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Usage: Just set SetState(currentPageIndex, pageCount) and indicators will be created and visualized automatically.
	/// </summary>
	public class PageIndicatorGroup : MonoBehaviour
	{
		#region Initialization

		protected void Start()
		{
			InitializeTemplate();
		}

		#endregion

		#region Template

		public PageIndicator PageIndicatorTemplate;

		private void InitializeTemplate()
		{
			PageIndicatorTemplate.gameObject.SetActive(false);
		}

		public void DestroyTemplate()
		{
			if (PageIndicatorTemplate)
			{
				Destroy(PageIndicatorTemplate.gameObject);
			}
		}

		#endregion

		#region Page Indicators

		private PageIndicator[] PageIndicators;

		#endregion

		#region Create/Destroy Page Indicators

		private void InitializeForPageCountIfRequired(int pageCount)
		{
			if (pageCount < 0 || pageCount > 50) // 50 is just a hard limit that is not expected.
				throw new ArgumentOutOfRangeException();

			if (PageIndicators?.Length != pageCount)
			{
				// Destroy previously created indicators first.
				DestroyAllPageIndicators();

				// Create indicators.
				PageIndicators = new PageIndicator[pageCount];
				var parent = PageIndicatorTemplate.transform.parent;
				for (int i = 0; i < pageCount; i++)
				{
					PageIndicators[i] = Instantiate(PageIndicatorTemplate, parent);
					PageIndicators[i].gameObject.name = "PageIndicator-" + i;
					PageIndicators[i].gameObject.SetActive(true);
				}
			}
		}

		private void DestroyAllPageIndicators()
		{
			if (PageIndicators == null)
				return;
			foreach (var pageIndicator in PageIndicators)
			{
				DestroyImmediate(pageIndicator.gameObject);
			}
			PageIndicators = null;
		}

		#endregion

		#region Switch Pages

		public void SetState(int activePageIndex, int pageCount)
		{
			InitializeForPageCountIfRequired(pageCount);

			for (int i = 0; i < PageIndicators.Length; i++)
			{
				if (i == activePageIndex)
				{
					PageIndicators[i].SetAsSelected();
				}
				else
				{
					PageIndicators[i].SetAsDeselected();
				}
			}
		}

		#endregion
	}

}
