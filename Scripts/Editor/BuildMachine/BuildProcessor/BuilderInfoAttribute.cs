using System;

namespace Extenity.BuildMachine.Editor
{

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class BuilderInfoAttribute : Attribute
	{
		public readonly string Name;

		public BuilderInfoAttribute(string name)
		{
			Name = name;
		}
	}

}
