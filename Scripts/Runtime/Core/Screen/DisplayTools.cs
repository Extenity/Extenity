#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.ScreenToolbox
{

	public static class DisplayTools
	{
		#region Active Display

		public static int GetActiveDisplayIndex()
		{
			var displays = Display.displays;
			for (var i = 0; i < displays.Length; i++)
			{
				if (displays[i].active)
					return i;
			}
			return -1;
		}

		#endregion
	}

}

#endif
