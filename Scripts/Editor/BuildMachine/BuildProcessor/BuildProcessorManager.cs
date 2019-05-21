using System;
using System.Linq;
using Extenity.DataToolbox;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

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

		public static readonly BuildProcessorDefinition[] BuildProcessors;

		#endregion

		#region Gather Build Processors

		private static BuildProcessorDefinition[] GatherBuildProcessorTypes()
		{
			var types = (
					from assembly in AppDomain.CurrentDomain.GetAssemblies()
					from type in assembly.GetTypes()
					where typeof(BuildProcessorBase).IsAssignableFrom(type) && !type.IsAbstract
					select type
				).ToList();

			var buildProcessors = new BuildProcessorDefinition[types.Count];
			for (var i = 0; i < types.Count; i++)
			{
				var type = types[i];

				var attribute = type.GetAttribute<BuildProcessorInfoAttribute>(true);
				if (attribute == null)
				{
					throw new Exception($"Build processor '{type.Name}' has no '{nameof(BuildProcessorInfoAttribute)}'.");
				}

				buildProcessors[i] = new BuildProcessorDefinition(attribute.Name, type);
			}

			return buildProcessors;
		}

		#endregion
	}

}
