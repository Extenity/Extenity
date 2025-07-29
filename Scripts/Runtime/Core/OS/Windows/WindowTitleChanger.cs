#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	public class WindowTitleChanger : MonoBehaviour
	{
		public string Title;

		private void Awake()
		{
			ChangeTitle();
		}

		public void ChangeTitle()
		{
			if (!string.IsNullOrEmpty(Title))
			{
				OperatingSystemTools.ChangeWindowTitle(Title);
			}
		}
	}

}

#endif
