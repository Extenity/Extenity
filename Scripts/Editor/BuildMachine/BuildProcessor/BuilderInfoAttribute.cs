using System;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class BuilderInfoAttribute : Attribute
	{
		public readonly string Name;
		public readonly BuildTarget BuildTarget;

		public BuilderInfoAttribute(string name, BuildTarget buildTarget)
		{
			Name = name;
			BuildTarget = buildTarget;
		}
	}

}
