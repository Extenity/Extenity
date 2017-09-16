using System;
using System.IO;
using System.Reflection;
using UnityEngine;

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
			typeof(string)
		};

		#endregion

		#region Status

		private static bool IsProcessing;

		#endregion

		public static void Obfuscate(string dllPath)
		{
			if (IsProcessing)
			{
				throw new Exception("Tried to start another obfuscation while there is an ongoing operation.");
			}
			IsProcessing = true;

			FailMessageWrittenByObfuscator = false;
			Application.logMessageReceived -= OnLogMessageReceived;
			Application.logMessageReceived += OnLogMessageReceived;
			Application.logMessageReceivedThreaded -= OnLogMessageReceived;
			Application.logMessageReceivedThreaded += OnLogMessageReceived;

			try
			{
				//var startTime = DateTime.Now;
				var method = GetObfuscateMethod();
				//Debug.LogFormat("Method search took '{0}'", (DateTime.Now - startTime).ToStringMinutesSecondsMilliseconds());

				if (method == null)
				{
					throw new Exception("Failed to find obfuscation proxy method in loaded assemblies.");
				}
				//if (method.ReturnParameter.ParameterType.IsSubclassOf(typeof(ObfuscateResult)))
				//{
				//	throw new Exception(string.Format("Obfuscation proxy method return parameter type should be '{0}' instead of '{1}'.",
				//		typeof(ObfuscateResult),
				//		method.ReturnParameter.ParameterType));
				//}

				// Check if DLL file really exist
				if (!string.IsNullOrEmpty(dllPath))
				{
					if (!File.Exists(dllPath))
					{
						throw new Exception("Could not find the file to obfuscate at path '" + dllPath + "'.");
					}
				}

				method.Invoke(null, new object[] { dllPath });
				//var obfuscationJobAsObject = method.Invoke(null, new object[] { dllPath });
				//return (ObfuscateResult)obfuscationJobAsObject;

				if (FailMessageWrittenByObfuscator)
					throw new Exception("Obfuscator failed! Check previous error messages.");
			}
			catch
			{
				throw;
			}
			finally
			{
				IsProcessing = false;
				Application.logMessageReceived -= OnLogMessageReceived;
				Application.logMessageReceivedThreaded -= OnLogMessageReceived;
			}
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

		#region Catch Obfuscator Error Messages

		private static bool FailMessageWrittenByObfuscator = false;

		private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception:
				case LogType.Warning:
					FailMessageWrittenByObfuscator = true;
					break;
				case LogType.Log:
					// Nothing to do.
					break;
				default:
					throw new ArgumentOutOfRangeException("type", type, null);
			}
		}

		#endregion
	}

}
