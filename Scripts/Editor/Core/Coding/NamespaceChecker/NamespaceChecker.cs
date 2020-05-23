using System.Linq;
using System.Reflection;
using Extenity.ProfilingToolbox;
using Sirenix.Utilities;
using UnityEditor.Callbacks;

namespace Extenity.CodingToolbox.Editor
{

	public static class NamespaceChecker
	{
		[DidReloadScripts]
		public static void EnsureAllNamespacesInAllAssemblies()
		{
			using (new QuickProfilerStopwatch($"{nameof(NamespaceChecker)} calculations took {{0}}", 1f))
			{
				var assemblies = AssemblyUtilities.GetAllAssemblies();
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

						// Log.Info($"Checking if all classes in assembly '{assembly}' have namespaces that start with '{namespaceShouldStartWith}'.");

						var types = assembly.GetTypes();
						for (var i = 0; i < types.Length; i++)
						{
							var type = types[i];
							var name = type.Name;

							// Skip compiler generated types. Source: https://stackoverflow.com/questions/187495/how-to-read-assembly-attributes
							if (name.StartsWith("__StaticArrayInitTypeSize") ||
							    name.StartsWith("<>") ||
							    name.StartsWith("_<>") ||
							    name.StartsWith("<PrivateImplementationDetails>"))
							{
								continue;
							}

							// Skip mysterious types that comes out of nowhere.
							if (name.Equals("EmbeddedAttribute") ||
							    name.Equals("IsReadOnlyAttribute"))
							{
								continue;
							}

							var checkedAgainst = namespaceShouldStartWith;

							var overrideAttribute = type.GetAttribute<OverrideEnsuredNamespaceAttribute>();
							if (overrideAttribute != null)
							{
								checkedAgainst = overrideAttribute.NamespaceShouldStartWith;
							}

							if (string.IsNullOrWhiteSpace(checkedAgainst) && string.IsNullOrWhiteSpace(type.Namespace))
							{
								continue; // That's alright. It can be set to have no namespace.
							}

							if (string.IsNullOrWhiteSpace(type.Namespace) ||
							    string.IsNullOrWhiteSpace(checkedAgainst) ||
							    !type.Namespace.StartsWith(checkedAgainst))
							{
								Log.Error($"Namespace of type '{name}' should start with '{checkedAgainst ?? "[NA]"}' instead of '{type.Namespace ?? "[NA]"}'.");
							}
						}
					}
				}
			}
		}
	}

}
