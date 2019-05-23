using System;

namespace Extenity.BuildMachine.Editor
{

	public struct BuilderInfo
	{
		public readonly string Name;
		public readonly Type Type;
		public readonly BuildStepDefinition[] Steps;

		public bool IsValid =>
			!string.IsNullOrWhiteSpace(Name) &&
			Type != null;

		public BuilderInfo(string name, Type type, BuildStepDefinition[] steps)
		{
			Name = name;
			Type = type;
			Steps = steps;
		}
	}

}
