using System;
using System.Linq;
using Extenity.DataToolbox;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	public class BuildProcessorMetadata
	{
		public readonly string Name;
		public readonly Type Type;

		public BuildProcessorMetadata(string name, Type type)
		{
			Name = name;
			Type = type;
		}
	}

	[InitializeOnLoad]
	public static class BuildProcessorManager
	{
		#region Initialization

		static BuildProcessorManager()
		{
			BuildProcessors = GatherBuildProcessorTypes();
		}

		#endregion

		#region Build Processors

		public static readonly BuildProcessorMetadata[] BuildProcessors;

		#endregion

		#region Gather Build Processors

		private static BuildProcessorMetadata[] GatherBuildProcessorTypes()
		{
			var types = (
					from assembly in AppDomain.CurrentDomain.GetAssemblies()
					from type in assembly.GetTypes()
					where typeof(BuildProcessorBase).IsAssignableFrom(type) && !type.IsAbstract
					select type
				).ToList();

			var buildProcessors = new BuildProcessorMetadata[types.Count];
			for (var i = 0; i < types.Count; i++)
			{
				var type = types[i];

				var attribute = type.GetAttribute<BuildProcessorInfoAttribute>(true);
				if (attribute == null)
				{
					throw new Exception($"Build processor '{type.Name}' has no '{nameof(BuildProcessorInfoAttribute)}'.");
				}

				buildProcessors[i] = new BuildProcessorMetadata(attribute.Name, type);
			}

			return buildProcessors;
		}

		#endregion
	}

}
