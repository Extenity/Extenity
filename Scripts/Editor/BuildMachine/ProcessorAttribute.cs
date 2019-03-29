using System;

namespace Extenity.BuildMachine.Editor
{

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class ProcessorAttribute : Attribute
	{
		public readonly int Order;
		public readonly string[] Categories;

		public ProcessorAttribute(int order, string category)
		{
			Order = order;
			Categories = new[] { category };
		}

		public ProcessorAttribute(int order, params string[] categories)
		{
			Order = order;
			Categories = categories;
		}
	}

}
