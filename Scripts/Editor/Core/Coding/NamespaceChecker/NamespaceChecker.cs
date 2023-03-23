using System;
using System.Linq;
using System.Reflection;
using Extenity.ProfilingToolbox;
using UnityEditor.Callbacks;

namespace Extenity.CodingToolbox.Editor
{

	public static class NamespaceChecker
	{
		[DidReloadScripts]
		public static void EnsureAllNamespacesInAllAssemblies()
		{
			using (new QuickProfilerStopwatch(Log, nameof(NamespaceChecker), 1f))
			{
				var assemblies = Sirenix.Utilities.AssemblyUtilities.GetAllAssemblies();
				foreach (var assembly in assemblies)
				{
					var attributes = assembly.GetCustomAttributes<EnsuredNamespaceAttribute>().ToArray();
					if (attributes.Length > 0)
					{
						if (attributes.Length > 1)
						{
							Log.Error($"Assembly '{assembly}' should not have more than one '{nameof(EnsuredNamespaceAttribute)}'.");
							continue;
						}
						var namespaceShouldStartWith = attributes[0].NamespaceShouldStartWith;

						var ignoreAttributes = assembly.GetCustomAttributes<IgnoreEnsuredNamespaceAttribute>();

						// Log.Info($"Checking if all classes in assembly '{assembly}' have namespaces that start with '{namespaceShouldStartWith}'.");

						var types = assembly.GetTypes();
						for (var i = 0; i < types.Length; i++)
						{
							var type = types[i];
							var name = type.Name;

							// Skip compiler generated types. Source: https://stackoverflow.com/questions/187495/how-to-read-assembly-attributes
							// Needed for:
							//    <PrivateImplementationDetails>
							//    __StaticArrayInitTypeSize=
							//    <>f__AnonymousType
							//    $BurstDirectCallInitializer
							if (name.StartsWith('_') ||
							    name.StartsWith('<') ||
							    name.StartsWith('$'))
							{
								continue;
							}

							// Skip mysterious types that come out of nowhere.
							if (name.Equals("EmbeddedAttribute", StringComparison.Ordinal) ||
							    name.Equals("IsReadOnlyAttribute", StringComparison.Ordinal) ||
							    name.StartsWith("UnitySourceGenerated", StringComparison.Ordinal) || // Encountered types: UnitySourceGeneratedAssemblyMonoScriptTypes, UnitySourceGeneratedAssemblyMonoScriptTypes_v1 
							    name.StartsWith("MonoScript", StringComparison.Ordinal) || // Encountered types: MonoScriptInfo, MonoScriptData
							    name.StartsWith("FileMonoScript", StringComparison.Ordinal)) // Encountered types: FileMonoScripts
							{
								continue;
							}

							var checkedAgainst = namespaceShouldStartWith;

							// No need to check if there are multiple attributes since its AllowMultiple is disabled.
							var overrideAttribute = type.GetCustomAttribute<OverrideEnsuredNamespaceAttribute>();
							if (overrideAttribute != null)
							{
								checkedAgainst = overrideAttribute.NamespaceShouldStartWith;
							}

							var typeNamespace = type.Namespace;
							var typeNamespaceIsEmpty = string.IsNullOrWhiteSpace(typeNamespace);
							if (string.IsNullOrWhiteSpace(checkedAgainst) && typeNamespaceIsEmpty)
							{
								continue; // That's alright. It can be set to have no namespace.
							}

							if (typeNamespaceIsEmpty ||
							    string.IsNullOrWhiteSpace(checkedAgainst) ||
							    !typeNamespace.StartsWith(checkedAgainst, StringComparison.Ordinal))
							{
								// Namespace does not meet expectations. But before finalizing the decision
								// that it's not okay, check if the namespace should be ignored.
								var shouldIgnore = false;
								if (!typeNamespaceIsEmpty)
								{
									foreach (var ignoreAttribute in ignoreAttributes)
									{
										if (typeNamespace.StartsWith(ignoreAttribute.IgnoreNamespacesThatStartWith, StringComparison.Ordinal))
										{
											shouldIgnore = true;
											break;
										}
									}
								}
								if (!shouldIgnore)
								{
									Log.Error($"Namespace of type '{name}' should start with '{checkedAgainst ?? "[NA]"}' instead of '{(typeNamespaceIsEmpty ? "[NA]" : typeNamespace)}'.");
								}
							}
						}
					}
				}
			}
		}

		#region Log

		private static readonly Logger Log = new(nameof(NamespaceChecker));

		#endregion
	}

}
