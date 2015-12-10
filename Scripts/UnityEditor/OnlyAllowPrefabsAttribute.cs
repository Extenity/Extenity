using System;
using UnityEngine;

//namespace Extenity.UnityEditor
//{

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class OnlyAllowPrefabsAttribute : PropertyAttribute
{
	public readonly bool allowImportedModels;

	public OnlyAllowPrefabsAttribute(bool allowImportedModels = true)
	{
		this.allowImportedModels = allowImportedModels;
	}
}

//}
