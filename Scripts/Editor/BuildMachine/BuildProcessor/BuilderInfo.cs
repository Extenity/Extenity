using System;

namespace Extenity.BuildMachine.Editor
{

	//[JsonObject] Nope. This is not for serialization.
	public struct BuilderInfo
	{
		public readonly string Name;
		public readonly Type Type;
		public readonly Type OptionsType;
		public readonly BuildStepInfo[] Steps;

		public bool IsValid =>
			!string.IsNullOrWhiteSpace(Name) &&
			Type != null;

		public BuilderInfo(string name, Type type, Type optionsType, BuildStepInfo[] steps)
		{
			Name = name;
			Type = type;
			OptionsType = optionsType;
			Steps = steps;
		}
	}

}
