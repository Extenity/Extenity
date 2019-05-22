using System.Reflection;
using Extenity.DataToolbox;

namespace Extenity.BuildMachine.Editor
{

	// See 11283467.
	public enum BuildStepType
	{
		Warmup,
		PreBuild,
		AssetProcess,
		SceneProcess,
		UnityBuild,
		PostBuild,
		Deploy,
		Finalize
	}

	public struct BuildStepDefinition
	{
		public readonly BuildStepType Type;
		public int ID => Order;
		public readonly int Order;
		public readonly string Name;
		public readonly MethodInfo Method;

		public BuildStepDefinition(MethodInfo method)
		{
			var attribute = method.GetAttribute<BuildStepAttribute>(true);

			Type = attribute.Type;
			Order = attribute.Order;
			Name = method.Name;
			Method = method;
		}
	}

}
