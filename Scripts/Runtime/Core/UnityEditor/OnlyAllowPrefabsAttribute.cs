using System;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class OnlyAllowPrefabsAttribute : PropertyAttribute
	{
		public readonly bool allowImportedModels;

		public OnlyAllowPrefabsAttribute(bool allowImportedModels = true)
		{
			this.allowImportedModels = allowImportedModels;
		}
	}

}
