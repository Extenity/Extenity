using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Extenity.DLLBuilder
{

	public static class DLLBuilder
	{
		#region Configuration

		//public static readonly string ExtenitySourcesBasePath = "Assets/Extenity/";

		//public static readonly string[] References =
		//{
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\mscorlib.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Core.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Data.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Xml.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\System.Xml.Linq.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\Boo.Lang.dll",
		//	@"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v3.5\Profile\Unity Full v3.5\UnityScript.Lang.dll",
		//};

		#endregion

		#region Process

		public static bool IsProcessing { get; private set; }

		public static void StartProcess()
		{
			var request = new BuildRequest
			{
				BuildJobID = Guid.NewGuid()
			};
			StartProcess(request);
		}

		public static void StartProcess(BuildRequest request)
		{
			var job = new BuildJob
			{
				BuildRequest = request
			};
			StartProcess(job);
		}

		public static void StartProcess(BuildJob job)
		{
			if (IsProcessing)
				throw new Exception("A process was already started.");
			IsProcessing = true;

			if (!job.IsStarted)
			{
				job.BuildRequest.AddCurrentProjectToRequesterProjectChain();
				Debug.Log(Constants.DLLBuilderName + " started to build all DLLs. Job ID: " + job.BuildRequest.BuildJobID);
				job.IsStarted = true;
			}
			else
			{
				// Means we are in the middle of build process. That is we are continuing after a recompilation.
				Debug.Log(Constants.DLLBuilderName + " continuing to build all DLLs. Job ID: " + job.BuildRequest.BuildJobID);
			}

			Repaint();


			Cleaner.ClearAllOutputDLLs(DLLBuilderConfiguration.Instance,
				() =>
				{
					Repaint();

					Compiler.CompileAllDLLs(
						() =>
						{
							try
							{
								Repaint();
								Packer.PackAll();
								Repaint();
								Distributer.DistributeToAll();
								Repaint();
								Debug.Log(Constants.DLLBuilderName + " successfully built all DLLs.");
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
							}
							IsProcessing = false;
							Repaint();
						},
						error =>
						{
							IsProcessing = false;
							Repaint();
							Debug.LogError(error);
						}
					);
				},
				exception =>
				{
					IsProcessing = false;
					Repaint();
					Debug.LogException(exception);
				}
			);
		}

		#endregion

		#region UI Repaint

		public static readonly UnityEvent OnRepaintRequested = new UnityEvent();

		public static void Repaint()
		{
			OnRepaintRequested.Invoke();
		}

		#endregion
	}

}
