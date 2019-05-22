using System.Reflection;
using Extenity.DataToolbox;

namespace Extenity.BuildMachine.Editor
{

	public struct BuildStepDefinition
	{
		public int ID => Order;
		public readonly int Order;
		public readonly string Name;
		public readonly MethodInfo Method;

		public BuildStepDefinition(MethodInfo method)
		{
			var attribute = method.GetAttribute<BuildStepAttribute>(true);

			Order = attribute.Order;
			Name = method.Name;
			Method = method;
		}
	}

}
