using Extenity.MathToolbox;
using TMPro;
using TMPro.Extensions;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class CodeProfilerEntryUI : MonoBehaviour
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Elements

		public TextMeshProUGUI LastDurationText;
		public TextMeshProUGUI AverageDurationText;
		public TextMeshProUGUI TotalCountText;

		public void Fill(float lastDuration, float averageDuration, int totalCount)
		{
			LastDurationText.SetCharArrayForValue("N3", lastDuration.ConvertSecondsToMilliseconds());
			AverageDurationText.SetCharArrayForValue("N3", averageDuration.ConvertSecondsToMilliseconds());
			TotalCountText.SetCharArrayForInt(totalCount);
		}

		#endregion
	}

}
