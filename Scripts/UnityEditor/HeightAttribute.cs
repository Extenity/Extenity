using System;
using UnityEngine;

//namespace Extenity.UnityEditor
//{

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class HeightAttribute : PropertyAttribute
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

//}
