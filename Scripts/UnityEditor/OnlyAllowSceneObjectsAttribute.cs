using System;
using UnityEngine;

//namespace Extenity.UnityEditor
//{

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class OnlyAllowSceneObjectsAttribute : PropertyAttribute
{
	public OnlyAllowSceneObjectsAttribute()
	{
	}
}

//}
