using System;

namespace Extenity.BuildMachine.Editor
{

	public class BuildProcessorInfoAttribute : Attribute
	{
		public readonly string Name;

		public BuildProcessorInfoAttribute(string name)
		{
			Name = name;
		}
	}

}
