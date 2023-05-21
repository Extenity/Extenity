using System.Reflection;
using Extenity.DataToolbox;

namespace Extenity.BuildMachine.Editor
{

	// See 11283467.
	public enum BuildStepType
	{
		PreBuild,
		LocalizationProcess,
		AssetProcess,
		SceneProcess,
		UnityBuild,
		PostBuild,
		LocalizationDeploy,
		AssetDeploy,
		ApplicationDeploy,
		Finalization,
	}

	//[JsonObject] Nope. This is not for serialization.
	public struct BuildStepInfo
	{
		public readonly BuildStepType Type;
		public int ID => Order;
		public readonly int Order;
		public readonly string Name;
		public readonly MethodInfo Method;

		public bool IsEmpty => Method == null;
		public static BuildStepInfo Empty = new BuildStepInfo(null);

		public BuildStepInfo(MethodInfo method)
		{
			if (method != null)
			{
				var attribute = method.GetAttribute<BuildStepAttribute>(true);

				Type = attribute.Type;
				Order = attribute.Order;
				Name = method.Name;
				Method = method;
			}
			else
			{
				Type = BuildStepType.PreBuild;
				Order = 0;
				Name = null;
				Method = null;
			}
		}
	}

}
