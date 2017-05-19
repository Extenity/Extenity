using System;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class OnlyAllowSceneObjectsAttribute : PropertyAttribute
	{
		public OnlyAllowSceneObjectsAttribute()
		{
		}
	}

}
