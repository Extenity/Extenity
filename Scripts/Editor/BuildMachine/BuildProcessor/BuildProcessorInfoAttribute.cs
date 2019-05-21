using System;

namespace Extenity.BuildMachine.Editor
{

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class BuildProcessorInfoAttribute : Attribute
	{
		public readonly string Name;

		public BuildProcessorInfoAttribute(string name)
		{
			Name = name;
		}
	}

}
