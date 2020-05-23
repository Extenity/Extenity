using System;
using JetBrains.Annotations;

namespace Extenity.CodingToolbox
{

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class EnsuredNamespaceAttribute : Attribute
	{
		public readonly string NamespaceShouldStartWith;

		public EnsuredNamespaceAttribute([CanBeNull] string namespaceShouldStartWith)
		{
			NamespaceShouldStartWith = namespaceShouldStartWith;
		}
	}

}
