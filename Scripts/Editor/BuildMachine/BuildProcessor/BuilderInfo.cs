using System;

namespace Extenity.BuildMachine.Editor
{

	public struct BuilderInfo
	{
		public readonly string Name;
		public readonly Type Type;
		public readonly BuildStepInfo[] Steps;

		public bool IsValid =>
			!string.IsNullOrWhiteSpace(Name) &&
			Type != null;

		public BuilderInfo(string name, Type type, BuildStepInfo[] steps)
		{
			Name = name;
			Type = type;
			Steps = steps;
		}
	}

}
