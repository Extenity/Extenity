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

	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
	public sealed class IgnoreEnsuredNamespaceAttribute : Attribute
	{
		public readonly string IgnoreNamespacesThatStartWith;

		public IgnoreEnsuredNamespaceAttribute([CanBeNull] string ignoreNamespacesThatStartWith)
		{
			IgnoreNamespacesThatStartWith = ignoreNamespacesThatStartWith;
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	public sealed class OverrideEnsuredNamespaceAttribute : Attribute
	{
		public readonly string NamespaceShouldStartWith;

		public OverrideEnsuredNamespaceAttribute([CanBeNull] string namespaceShouldStartWith)
		{
			NamespaceShouldStartWith = namespaceShouldStartWith;
		}
	}

}
