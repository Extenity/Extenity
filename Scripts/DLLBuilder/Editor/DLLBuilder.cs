using System;
using UnityEngine;

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
			if (IsProcessing)
				throw new Exception("A process was already started.");
			IsProcessing = true;

			Cleaner.ClearOutputDLLs(
				() =>
				{
					Compiler.CompileAllDLLs(
						() =>
						{
							try
							{
								PackageBuilder.CopyExtenityAssetsToOutputProject();
								Distributer.DistributeToOutsideProjects();
							}
							catch (Exception exception)
							{
								Debug.LogException(exception);
							}
							IsProcessing = false;
						},
						error =>
						{
							IsProcessing = false;
						}
					);
				},
				exception =>
				{
					Debug.LogException(exception);
					IsProcessing = false;
				}
			);
		}

		#endregion
	}

}
