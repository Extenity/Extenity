using System;
using JetBrains.Annotations;

namespace Extenity.CodingToolbox
{

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
	public sealed class OverrideEnsuredNamespaceAttribute : Attribute
	{
		public readonly string NamespaceShouldStartWith;

		public OverrideEnsuredNamespaceAttribute([CanBeNull] string namespaceShouldStartWith)
		{
			NamespaceShouldStartWith = namespaceShouldStartWith;
		}
	}

}
