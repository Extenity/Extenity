using System;

namespace Extenity.CodingToolbox
{

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class EnsuredNamespaceAttribute : Attribute
	{
		public readonly string NamespaceShouldStartWith;

		public EnsuredNamespaceAttribute(string namespaceShouldStartWith)
		{
			NamespaceShouldStartWith = namespaceShouldStartWith;
		}
	}

}
