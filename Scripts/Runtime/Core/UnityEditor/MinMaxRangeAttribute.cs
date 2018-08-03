using System;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class MinMaxRangeAttribute : PropertyAttribute
	{
		public readonly float MinLimit;
		public readonly float MaxLimit;

		public MinMaxRangeAttribute(float minLimit, float maxLimit)
		{
			MinLimit = minLimit;
			MaxLimit = maxLimit;
		}
	}

}
