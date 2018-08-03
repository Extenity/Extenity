using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	public class EnumMaskAttribute : PropertyAttribute
	{
		public bool ShowValueInLabel = false;

		public EnumMaskAttribute()
		{
		}

		public EnumMaskAttribute(bool showValueInLabel)
		{
			ShowValueInLabel = showValueInLabel;
		}
	}

}
