using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	[InitializeOnLoad]
	public static class BuilderManager
	{
		#region Initialization

		static BuilderManager()
		{
			BuilderInfos = GatherBuilderInfos();
		}

		#endregion

		#region Builder Infos

		public static readonly BuilderInfo[] BuilderInfos;

		#endregion

		#region Gather Builder and Build Step Info

		private static BuilderInfo[] GatherBuilderInfos()
		{
			var types = (
					from assembly in AppDomain.CurrentDomain.GetAssemblies()
					from type in assembly.GetTypes()
					where typeof(Builder).IsAssignableFrom(type) && !type.IsAbstract
					select type
				).ToList();

			var builderInfos = new BuilderInfo[types.Count];
			for (var i = 0; i < types.Count; i++)
			{
				var type = types[i];

				// Make sure the class is serializable
				if (!type.HasAttribute<SerializableAttribute>())
				{
					Log.Error($"Builder '{type.Name}' has no '{nameof(SerializableAttribute)}'.");
				}

				// Get BuilderInfo class attribute
				var infoAttribute = type.GetAttribute<BuilderInfoAttribute>(true);
				if (infoAttribute == null)
				{
					Log.Error($"Builder '{type.Name}' has no '{nameof(BuilderInfoAttribute)}'.");
				}

				// Complain about non-serializable fields
				var nonSerializedFields = type
					.GetNonSerializedFields()
					.Where(field => !field.Name.StartsWith("_"))
					.ToArray();
				if (nonSerializedFields.Length > 0)
				{
					var text = string.Join(", ", nonSerializedFields.Select(entry => entry.Name));
					Log.Error($"Builder '{type.Name}' has non-serializable field(s) '{text}' which is not allowed to prevent any confusion. Builders need to be fully serializable to prevent losing data between assembly reloads and Unity Editor relaunches. Start the name with '_' to ignore this check if the non-serialized field is essential.");
				}

				// Get build step methods
				var steps = CollectBuildStepMethods(type);

				builderInfos[i] = new BuilderInfo(infoAttribute?.Name, type, steps);
			}

			return builderInfos;
		}

		private static BuildStepDefinition[] CollectBuildStepMethods(Type type)
		{
			var methods = type
				.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
				.Where(method =>
					{
						if (!method.IsVirtual && method.ReturnType == typeof(IEnumerator))
						{
							var parameters = method.GetParameters();
							if (parameters.Length == 1 &&
								parameters[0].ParameterType == typeof(BuildStepDefinition)
							)
							{
								var attribute = method.GetAttribute<BuildStepAttribute>(true);
								if (attribute != null)
								{
									if (attribute.Order <= 0)
									{
										Log.Error($"The '{attribute.GetType().Name}' attribute should have an order above 0.");
									}
									return true;
								}
							}
						}
						return false;
					}
				)
				.OrderBy(method => method.GetAttribute<BuildStepAttribute>(true).Order)
				.ToList();

			//methods.LogList();

			// Check for duplicate Order values. Note that we already ordered it above.
			if (methods.Count > 1)
			{
				var detected = false;
				var previousMethod = methods[0];
				var previousMethodOrder = previousMethod.GetAttribute<BuildStepAttribute>(true).Order;
				for (int i = 1; i < methods.Count; i++)
				{
					var currentMethod = methods[i];
					var currentMethodOrder = currentMethod.GetAttribute<BuildStepAttribute>(true).Order;

					if (previousMethodOrder == currentMethodOrder)
					{
						detected = true;
						Log.Error($"Methods '{previousMethod.Name}' and '{currentMethod.Name}' have the same order of '{currentMethodOrder}'.");
					}

					previousMethod = currentMethod;
					previousMethodOrder = currentMethodOrder;
				}
				if (detected)
				{
					throw new Exception("Failed to sort processor method list because there were methods with the same order value.");
				}
			}

			return methods.Select(method => new BuildStepDefinition(method)).ToArray();
		}

		#endregion
	}

}
