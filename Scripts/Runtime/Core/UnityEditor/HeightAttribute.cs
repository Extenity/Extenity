using AttributeUsage = System.AttributeUsageAttribute;
using AttributeTargets = System.AttributeTargets;

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

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class HeightAttribute : BaseAttribute
	{
		public readonly float height;
		public readonly bool includeChildren;

		//public HeightAttribute(float height)
		//{
		//	this.height = height;
		//	this.includeChildren = false;
		//}

		public HeightAttribute(float height, bool includeChildren = false)
		{
			this.height = height;
			this.includeChildren = includeChildren;
		}
	}

}
