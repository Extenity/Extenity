using System;
using System.IO;
using System.Linq;
using System.Text;
using Extenity.DataToolbox;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Extenity.BuildToolbox.Editor
{

	#region Included DLLs Logger

	public class DLLBuildReport : IPostBuildPlayerScriptDLLs
	{
		public int callbackOrder => 100000;

		public void OnPostBuildPlayerScriptDLLs(BuildReport report)
		{
			var dllsWithoutDebugFiles = report.files.Select(item => item.path)
				.Where(path =>
				   !path.EndsWith(".pdb", StringComparison.OrdinalIgnoreCase) &&
				   !path.EndsWith(".mdb", StringComparison.OrdinalIgnoreCase))
				.Select(path => Path.GetFileName(path) + "\t" + Path.GetDirectoryName(path))
				.OrderBy(path => path)
				.ToList();
			Log.Info($"Included DLLs ({dllsWithoutDebugFiles.Count}):\n" + string.Join("\n", dllsWithoutDebugFiles));
			report.DetailedLog(nameof(DLLBuildReport));
		}
	}

	#endregion

	public static class BuildReportTools
	{
		#region Logging

		public static void DetailedLog(this BuildReport report, string callerTag)
		{
			Log.Info($"Build report in details (tagged '{callerTag}'):\n" + report.ToDetailedLogString());
		}

		public static string ToDetailedLogString(this BuildReport report)
		{
			// ReSharper disable HeapView.ClosureAllocation
			// ReSharper disable HeapView.BoxingAllocation
			var stringBuilder = new StringBuilder();
			var indentation = "";

			// Summary
			try
			{
				Title("Summary");
				{
					Line("Result: " + report.summary.result);
					Line("Total Errors: " + report.summary.totalErrors);
					Line("Total Warnings: " + report.summary.totalWarnings);
					Line("Total Size: " + report.summary.totalSize);
					Line("Total Time: " + report.summary.totalTime);
					Line("Platform: " + report.summary.platform);
					Line("Platform Group: " + report.summary.platformGroup);
					Line("Options: " + report.summary.options);
					Line("Output Path: " + report.summary.outputPath);
					Line("Started At: " + report.summary.buildStartedAt);
					Line("Ended At: " + report.summary.buildEndedAt);
					Line("GUID: " + report.summary.guid);
					//Line("CRC: " + report.summary.crc);
					//Line("Build Type: " + report.summary.buildType);
				}
				//DecreaseIndent(); Not needed
			}
			catch (Exception exception)
			{
				Line("ERROR: " + exception.Message);
			}
			finally
			{
				ResetIndent();
			}

			// Steps
			try
			{
				Title($"Steps ({report.steps.Length}):");
				for (var i = 0; i < report.steps.Length; i++)
				{
					var step = report.steps[i];
					Title($"Step {i}: {step.name}");
					{
						Line("Depth: " + step.depth);
						Line("Duration: " + step.duration);
						Title($"Messages ({step.messages.Length}):");
						foreach (var message in step.messages)
						{
							{
								Line(message.type + ": " + message.content);
							}
						}
						DecreaseIndent();
					}
					DecreaseIndent();
				}
				//DecreaseIndent(); Not needed
			}
			catch (Exception exception)
			{
				Line("ERROR: " + exception.Message);
			}
			finally
			{
				ResetIndent();
			}

			// Files
			try
			{
				Title($"Files ({report.files.Length}):");
				for (var i = 0; i < report.files.Length; i++)
				{
					var file = report.files[i];
					Title($"File {i}: {file.path}");
					{
						Line("Role: " + file.role);
						Line("Size: " + file.size);
					}
					DecreaseIndent();
				}
				//DecreaseIndent(); Not needed
			}
			catch (Exception exception)
			{
				Line("ERROR: " + exception.Message);
			}
			finally
			{
				ResetIndent();
			}

			// Stripping Info
			try
			{
				Title("Stripping Info");
				{
					var info = report.strippingInfo;

					// Just curious if it's a file. Turns out it's not.
					//Line("Asset Path: " + AssetDatabase.GetAssetPath(report.info));

					if (info.includedModules.Any())
					{
						Line("No included modules listed");
					}
					else
					{
						var includedModules = info.includedModules.ToList();
						Title($"Included Modules {includedModules.Count}");
						foreach (var includedModule in includedModules)
						{
							Title("Module: " + includedModule);
							{
								var reasons = info.GetReasonsForIncluding(includedModule).ToList();
								Title($"Reasons ({reasons.Count}):");
								foreach (var reason in reasons)
								{
									Line(reason);
								}
								DecreaseIndent();
							}
							DecreaseIndent();
						}
						DecreaseIndent();
					}
				}
				//DecreaseIndent(); Not needed
			}
			catch (Exception exception)
			{
				Line("ERROR: " + exception.Message);
			}
			finally
			{
				ResetIndent();
			}

			return stringBuilder.ToString();

			void Title(string message)
			{
				stringBuilder.AppendLine(indentation + ClipMesage(message));
				IncreaseIndent();
			}
			void Line(string message)
			{
				stringBuilder.AppendLine(indentation + ClipMesage(message));
			}
			string ClipMesage(string message)
			{
				var index = message.IndexOfNextLineEnding(0);
				if (index < 0)
					return message;
				return message.Substring(0, index) + "... (clipped multiline)";
			}
			void IncreaseIndent()
			{
				indentation += "\t";
			}
			void DecreaseIndent()
			{
				indentation = indentation.Substring(0, Mathf.Max(0, indentation.Length - 1));
			}
			void ResetIndent()
			{
				indentation = "";
			}
			// ReSharper restore HeapView.ClosureAllocation
			// ReSharper restore HeapView.BoxingAllocation
		}

		#endregion
	}

}