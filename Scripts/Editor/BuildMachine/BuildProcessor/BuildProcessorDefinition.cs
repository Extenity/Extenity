using System;

namespace Extenity.BuildMachine.Editor
{

	public class BuildProcessorDefinition
	{
		public readonly string Name;
		public readonly Type Type;

		public BuildProcessorDefinition(string name, Type type)
		{
			Name = name;
			Type = type;
		}
	}

}
