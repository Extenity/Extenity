using System;
using System.IO;
using System.Reflection;

namespace Extenity.DLLBuilder
{

	public static class ObfuscationLauncher
	{
		#region Configuration

		private const string ProxyClassName = "ObfuscationProxy";
		private const string ProxyMethodName = "Obfuscate";
		private const BindingFlags ProxyMethodBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
		private static readonly Type[] ProxyMethodInputParameters =
		{
			typeof(string),
			typeof(string)
		};

		#endregion

		private class DummyObfuscationJob : ObfuscationJob
		{
			public override bool Finished
			{
				get { return true; }
			}
		}

		public static ObfuscationJob Obfuscate(CompilerJob job)
		{
			// Fail whole obfuscation if one of the DLLs failed to compile.
			if (job.RuntimeDLLSucceeded == CompileResult.Failed || job.EditorDLLSucceeded == CompileResult.Failed)
			{
				var obfuscationJob = new DummyObfuscationJob();
				obfuscationJob.RuntimeDLLSucceeded = ObfuscateResult.Failed;
				obfuscationJob.EditorDLLSucceeded = ObfuscateResult.Failed;
				return obfuscationJob;
			}
			// Skip whole obfuscation if both of the DLLs skipped compilation.
			if (job.RuntimeDLLSucceeded == CompileResult.Skipped && job.EditorDLLSucceeded == CompileResult.Skipped)
			{
				var obfuscationJob = new DummyObfuscationJob();
				obfuscationJob.RuntimeDLLSucceeded = ObfuscateResult.Skipped;
				obfuscationJob.EditorDLLSucceeded = ObfuscateResult.Skipped;
				return obfuscationJob;
			}

			//var startTime = DateTime.Now;
			var method = GetObfuscateMethod();
			//Debug.LogFormat("Method search took '{0}'", (DateTime.Now - startTime).ToStringMinutesSecondsMilliseconds());

			if (method == null)
			{
				throw new Exception("Failed to find obfuscation proxy method in loaded assemblies.");
			}
			if (method.ReturnParameter.ParameterType.IsSubclassOf(typeof(ObfuscationJob)))
			{
				throw new Exception(string.Format("Obfuscation proxy method return parameter type should be '{0}' instead of '{1}'.",
					typeof(ObfuscationJob),
					method.ReturnParameter.ParameterType));
			}

			var runtimeDLLPath = job.RuntimeDLLSucceeded == CompileResult.Skipped
				? ""
				: job.Configuration.DLLPath;
			var editorDLLPath = job.EditorDLLSucceeded == CompileResult.Skipped
				? ""
				: job.Configuration.EditorDLLPath;

			// Check if DLL files really exist
			{
				if (!string.IsNullOrEmpty(runtimeDLLPath))
				{
					if (!File.Exists(runtimeDLLPath))
					{
						throw new Exception("Could not find the file to obfuscate at path '" + runtimeDLLPath + "'.");
					}
				}
				if (!string.IsNullOrEmpty(editorDLLPath))
				{
					if (!File.Exists(editorDLLPath))
					{
						throw new Exception("Could not find the file to obfuscate at path '" + editorDLLPath + "'.");
					}
				}
			}

			var obfuscationJobAsObject = method.Invoke(null, new object[] { runtimeDLLPath, editorDLLPath });
			return (ObfuscationJob)obfuscationJobAsObject;
		}

		private static MethodInfo GetObfuscateMethod()
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (var iAssembly = 0; iAssembly < assemblies.Length; iAssembly++)
			{
				var types = assemblies[iAssembly].GetTypes();
				for (var iType = 0; iType < types.Length; iType++)
				{
					var type = types[iType];
					if (type.Name == ProxyClassName)
					{
						var method = type.GetMethod(
							ProxyMethodName, ProxyMethodBindingFlags,
							null, CallingConventions.Any,
							ProxyMethodInputParameters, null);
						if (method != null)
						{
							return method;
						}
					}
				}
			}
			return null;
		}
	}

}
