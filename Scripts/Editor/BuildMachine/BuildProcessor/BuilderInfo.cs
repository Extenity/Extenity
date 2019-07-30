using System;
using Extenity.BuildToolbox.Editor;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	//[JsonObject] Nope. This is not for serialization.
	public struct BuilderInfo
	{
		public readonly string Name;
		public readonly BuildTarget BuildTarget;
		public BuildTargetGroup BuildTargetGroup => BuildTarget.GetBuildTargetGroup();
		public readonly Type Type;
		public readonly Type OptionsType;
		public readonly BuildStepInfo[] Steps;

		public bool IsValid =>
			!string.IsNullOrWhiteSpace(Name) &&
			Type != null;

		public BuilderInfo(string name, BuildTarget buildTarget, Type type, Type optionsType, BuildStepInfo[] steps)
		{
			Name = name;
			BuildTarget = buildTarget;
			Type = type;
			OptionsType = optionsType;
			Steps = steps;
		}
	}

}
