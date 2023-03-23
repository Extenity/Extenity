using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using UnityEditor;

namespace Extenity.BuildMachine.Editor
{

	[InitializeOnLoad]
	public static class BuilderManager
	{
		#region Initialization

		static BuilderManager()
		{
			//BuilderLog.Info("Initializing BuildMachine");
			BuilderInfos = GatherBuilderInfos();
			BuildJobRunner.ContinueFromRunningJobAfterAssemblyReload();
		}

		#endregion

		#region Builder Infos

		public static readonly BuilderInfo[] BuilderInfos;

		#endregion

		#region Gather Builder and Build Step Info

		private static List<Type> CollectBuilderTypes()
		{
			var result = new List<Type>();
			var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			var assemblies = new List<Assembly>(allAssemblies.Length);

			var assetsDirectoryString = Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar;
			var scriptAssembliesDirectoryString = Path.DirectorySeparatorChar + "ScriptAssemblies" + Path.DirectorySeparatorChar;

			foreach (var assembly in allAssemblies)
			{
				if (!assembly.IsDynamic &&
				    (assembly.Location.Contains(assetsDirectoryString, StringComparison.Ordinal) ||
				     assembly.Location.Contains(scriptAssembliesDirectoryString, StringComparison.Ordinal)))
				{
					var name = assembly.FullName;
					if (!name.StartsWith("UnityEngine", StringComparison.Ordinal) &&
					    !name.StartsWith("UnityEditor", StringComparison.Ordinal) &&
					    !name.StartsWith("Unity.", StringComparison.Ordinal) &&
					    !name.StartsWith("com.unity.", StringComparison.Ordinal) &&
					    !name.StartsWith("ExCSS", StringComparison.Ordinal) &&
					    !name.StartsWith("System.", StringComparison.Ordinal) &&
					    !name.StartsWith("mscorlib", StringComparison.Ordinal) &&
					    !name.StartsWith("netstandard", StringComparison.Ordinal) &&
					    !name.StartsWith("Mono.", StringComparison.Ordinal) &&
					    !name.StartsWith("Extenity.", StringComparison.Ordinal) &&
					    !name.StartsWith("Google.", StringComparison.Ordinal) &&
					    !name.StartsWith("Newtonsoft.", StringComparison.Ordinal) &&
					    !name.StartsWith("nunit.", StringComparison.Ordinal) &&
					    !name.StartsWith("Cinemachine", StringComparison.Ordinal) &&
					    !name.StartsWith("DOTween", StringComparison.Ordinal) &&
					    !name.StartsWith("StompyRobot", StringComparison.Ordinal) &&
					    !name.StartsWith("SRDebugger", StringComparison.Ordinal) &&
					    !name.StartsWith("ICSharpCode", StringComparison.Ordinal) &&
					    !name.StartsWith("CodeStage", StringComparison.Ordinal) &&
					    !name.StartsWith("Sirenix", StringComparison.Ordinal))
					{
						assemblies.Add(assembly);
					}
				}
			}

			// foreach (var assembly in assemblies.OrderBy(item => item.Location))
			// {
			// 	BuilderLog.Info("assembly:   " + assembly.Location);
			// }

			for (var iAssembly = 0; iAssembly < assemblies.Count; iAssembly++)
			{
				var assembly = assemblies[iAssembly];
				var types = assembly.GetTypes();
				for (var iType = 0; iType < types.Length; iType++)
				{
					var type = types[iType];
					if (typeof(Builder).IsAssignableFrom(type) && !type.IsAbstract)
					{
						result.Add(type);
					}
				}
			}
			return result;
		}

		private static BuilderInfo[] GatherBuilderInfos()
		{
			var types = CollectBuilderTypes();

			var builderInfos = new BuilderInfo[types.Count];
			for (var i = 0; i < types.Count; i++)
			{
				var type = types[i];

				// Make sure the class is serializable
				if (!type.HasAttribute<JsonObjectAttribute>())
				{
					Log.Error($"Builder '{type.Name}' has no '{nameof(JsonObjectAttribute)}'.");
				}

				// Get BuilderInfo class attribute
				var infoAttribute = type.GetAttribute<BuilderInfoAttribute>(true);
				if (infoAttribute == null)
				{
					Log.Error($"Builder '{type.Name}' has no '{nameof(BuilderInfoAttribute)}'.");
				}

				// Get Options type
				var optionsType = type.GetField(nameof(Builder<BuilderOptions>.Options)).FieldType;

				// TODO: This is no longer legit. It needs to be implemented for Newtonsoft Json.
				//// Complain about non-serializable fields
				//var nonSerializedFields = type
				//	.GetNonSerializedFields()
				//	.Where(field => !field.Name.StartsWith("_"))
				//	.ToArray();
				//if (nonSerializedFields.Length > 0)
				//{
				//	var text = string.Join(", ", nonSerializedFields.Select(entry => entry.Name));
				//	BuilderLog.Error($"Builder '{type.Name}' has non-serializable field(s) '{text}' which is not allowed to prevent any confusion. Builders need to be fully serializable to prevent losing data between assembly reloads and Unity Editor relaunches. Start the name with '_' to ignore this check if the non-serialized field is essential.");
				//}

				// Get Build Step and Finalization Step methods
				var steps = GatherBuildStepMethods(type);

				builderInfos[i] = new BuilderInfo(
					infoAttribute?.Name,
					infoAttribute?.BuildTarget ?? BuildTarget.NoTarget,
					type,
					optionsType,
					steps);
			}

			return builderInfos;
		}

		private static BuildStepInfo[] GatherBuildStepMethods(Type type)
		{
			var methods = type
			              .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
			              .Where(method =>
				              {
					              if (!method.IsVirtual && method.ReturnType == typeof(IEnumerator))
					              {
						              var parameters = method.GetParameters();
						              // See 113654126.
						              if (parameters.Length == 2 &&
						                  parameters[0].ParameterType == typeof(BuildJob) &&
						                  parameters[1].ParameterType == typeof(BuildStepInfo)
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
					throw new BuildMachineException("Failed to sort Build Step method list because there were methods with the same order value.");
				}
			}

			return methods.Select(method => new BuildStepInfo(method)).ToArray();
		}

		#endregion

		#region Log

		private static readonly Logger Log = new("Builder");

		#endregion
	}

}
