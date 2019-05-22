using System;

namespace Extenity.BuildMachine.Editor
{

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class BuildStepAttribute : Attribute
	{
		public readonly int Order;
		public readonly string[] Categories;

		public BuildStepAttribute(int order, string category)
		{
			Order = order;
			Categories = new[] { category };
		}

		public BuildStepAttribute(int order, params string[] categories)
		{
			Order = order;
			Categories = categories;
		}
	}

}
