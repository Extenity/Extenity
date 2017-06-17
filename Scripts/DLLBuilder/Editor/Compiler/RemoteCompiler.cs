using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEditor;
using System.IO;
using Extenity.ConsistencyToolbox;
using Extenity.DataToolbox;
using Guid = System.Guid;

namespace Extenity.DLLBuilder
{

	[InitializeOnLoad]
	public static class RemoteCompiler
	{
		#region Configuration

		public static readonly float RequestCheckerInterval = 1f;
		public static readonly string RequestFilePath = "Temp/ExtenityDLLRemoteCompiler/Request.json";

		#endregion

		#region Periodic Compile Request Checker

		static RemoteCompiler()
		{
			var timer = new Timer(RequestCheckerInterval * 1000);
			timer.Elapsed += OnTimeToCheckRequests;
			timer.AutoReset = true;
			timer.Enabled = true;
		}

		private static void OnTimeToCheckRequests(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			CheckCompileRequestFromFile();
		}

		#endregion

		#region Compile Request

		[Serializable]
		public class CompileRequest : IConsistencyChecker
		{
			/// <summary>
			/// Paths of projects that requested compilation.
			/// </summary>
			public string[] RequesterProjectChain;
			public Guid BuildJobID;

			public void CheckConsistency(ref List<ConsistencyError> errors)
			{
				if (RequesterProjectChain == null || RequesterProjectChain.Length == 0)
				{
					errors.Add(new ConsistencyError(this, "Requester project path list is empty."));
				}
				else
				{
					for (var i = 0; i < RequesterProjectChain.Length; i++)
					{
						var projectPath = RequesterProjectChain[i];
						if (string.IsNullOrEmpty(projectPath))
						{
							errors.Add(new ConsistencyError(this, "Requester project path at index '" + i + "' is empty."));
						}
						//if (!Directory.Exists(projectPath)) No need to check for this, since we don't actually use these folders other than logging purposes.
						//{
						//}
					}
				}

				if (BuildJobID == Guid.Empty)
				{
					errors.Add(new ConsistencyError(this, "Build Job ID is not specified."));
				}
			}

			public override string ToString()
			{
				return "Build Job '" + BuildJobID + "' as requested by following projects:\n" + StringTools.Serialize(RequesterProjectChain, '\n');
			}
		}

		#endregion

		#region Receive Compilation Request

		private static void CheckCompileRequestFromFile()
		{
			try
			{
				var content = File.ReadAllText(RequestFilePath);
				DeleteCompileRequestFile();
				Debug.Log("## file content: " + content);

				var request = JsonUtility.FromJson<CompileRequest>(content);
				request.CheckConsistencyAndThrow();

				Debug.Log("Remote DLL compile request received: " + request);

				throw new NotImplementedException();
			}
			catch (DirectoryNotFoundException)
			{
				// ignored
			}
			catch (FileNotFoundException)
			{
				// ignored
			}
			catch (Exception exception)
			{
				Debug.LogError("Failed to process " + Constants.DLLBuilderName + " remote request file. Reason: " + exception);
				// Delete request file so that it won't bother console logs again.
				DeleteCompileRequestFile();
			}
		}

		private static void DeleteCompileRequestFile()
		{
			try
			{
				File.Delete(RequestFilePath);
			}
			catch
			{
				// ignored
			}
		}

		#endregion

		#region Create Compilation Request

		public static void CreateCompilationRequestForProject(CompileRequest existingRequest, string projectPath)
		{
			if (string.IsNullOrEmpty(projectPath))
				throw new ArgumentNullException("projectPath");
			existingRequest.CheckConsistencyAndThrow();

			//existingRequest.RequesterProjectChain.Add(PathOfThisProject);

			throw new NotImplementedException();
		}

		public static void CreateCompilationRequestForProject(Guid buildJobID, string projectPath)
		{
			if (string.IsNullOrEmpty(projectPath))
				throw new ArgumentNullException("projectPath");
			if (buildJobID == Guid.Empty)
				throw new ArgumentNullException("buildJobID", "Build Job ID must be specified.");

			throw new NotImplementedException();
		}

		#endregion
	}

}
