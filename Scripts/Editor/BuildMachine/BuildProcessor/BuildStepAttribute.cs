using System;

namespace Extenity.BuildMachine.Editor
{

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class BuildStepAttribute : Attribute
	{
		public readonly BuildStepType Type;
		public readonly int Order;
		public readonly string[] Categories;

		public BuildStepAttribute(BuildStepType type, int order, params string[] categories)
		{
			Type = type;
			Order = order;
			Categories = categories;
		}
	}

}
