using System;

namespace Extenity.BuildMachine.Editor
{

	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
	public class SceneProcessStepAttribute : Attribute
	{
		public readonly int Order;
		public readonly string[] Categories;

		public SceneProcessStepAttribute(int order, string category)
		{
			Order = order;
			Categories = new[] { category };
		}

		public SceneProcessStepAttribute(int order, params string[] categories)
		{
			Order = order;
			Categories = categories;
		}
	}

}
