using Extenity.ProfilingToolbox;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class CodeProfilerUI : MonoBehaviour
	{
		#region Initialization

		protected void OnEnable()
		{
			CodeProfiler.RequestAcivation();
			InitializeItems();
		}

		#endregion

		#region Deinitialization

		protected void OnDisable()
		{
			CodeProfiler.ReleaseAcivation();
			ClearItems();
		}

		#endregion

		#region Update

		protected void LateUpdate()
		{
		}

		#endregion

		#region Items

		public CodeProfilerEntryUI EntryTemplate;

		private void InitializeItems()
		{
			EntryTemplate.gameObject.SetActive(false);
		}

		private void ClearItems()
		{
			throw new System.NotImplementedException();
		}

		#endregion
	}

}
