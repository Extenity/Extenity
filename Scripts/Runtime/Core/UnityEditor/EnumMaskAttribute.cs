// This is the way that Attributes are supported in different environments like
// both in Unity and in UniversalExtenity. Also don't add 'using UnityEngine' or 'using System'
// in this code file to prevent any possible confusions. Use 'using' selectively, like
// 'using Exception = System.Exception;'
// See 11746845.
#if UNITY
using BaseAttribute = UnityEngine.PropertyAttribute;
#else
using BaseAttribute = System.Attribute;
#endif

namespace Extenity.UnityEditorToolbox
{

	public class EnumMaskAttribute : BaseAttribute
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
