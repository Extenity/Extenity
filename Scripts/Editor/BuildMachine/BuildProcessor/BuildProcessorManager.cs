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
					Log.Error($"Build processor '{type.Name}' has no '{nameof(BuildProcessorInfoAttribute)}'.");
				}

				// Serializable fields
				var nonSerializedFields = type
					.GetNonSerializedFields()
					.Where(field => !field.Name.StartsWith("_"))
					.ToArray();
				if (nonSerializedFields.Length > 0)
				{
					var text = string.Join(", ", nonSerializedFields.Select(entry => entry.Name));
					Log.Error($"Build processor '{type.Name}' has non-serializable field(s) '{text}' which is not allowed to prevent any confusion. Builders need to be fully serializable to prevent losing data between assembly reloads and Unity Editor relaunches. Start the name with '_' to ignore this check if the non-serialized field is essential.");
				}

				buildProcessors[i] = new BuildProcessorDefinition(attribute.Name, type);
			}

			return buildProcessors;
		}

		#endregion
	}

}
