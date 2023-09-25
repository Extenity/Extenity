using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Extenity.ParallelToolbox.Editor
{

	public static class ProgressTools
	{
		public static async Task WaitUntilAllProgressItemsToFinish()
		{
			while (Progress.running)
			{
				await Task.Yield();
			}
		}

		public static async Task WaitUntilAllProgressItemsToFinishAndLog(string subject)
		{
			if (!Progress.running)
			{
				Log.Info("Skipped waiting for Unity Progress to finish as there is no progress running. Subject: " + subject);
				return;
			}

			var iWait = 0;
			var stringBuilder = new StringBuilder();
			var items = new List<Progress.Item>();

			while (Progress.running)
			{
				try
				{
					iWait++;

					stringBuilder.Clear();
					stringBuilder.Append("Waiting for Unity Progress to finish at '");
					stringBuilder.Append(subject);
					stringBuilder.Append("'. Remaining time: ");
					stringBuilder.Append(Progress.globalRemainingTime);
					stringBuilder.Append("  %");
					stringBuilder.Append((Progress.globalProgress * 100).ToString("N2"));
					stringBuilder.Append("  Waited ");
					stringBuilder.Append(iWait);
					stringBuilder.AppendLine(" times.");

					// Not sure if this is the best way to get the progress details for all items.
					// There is also Progress.GetId() and Progress.GetName() but not sure about thread safety of those.
					items.Clear();
					foreach (var item in Progress.EnumerateItems())
					{
						items.Add(item);
					}

					for (int i = 0; i < items.Count; i++)
					{
						var item = items[i];
						stringBuilder.Append("   [");
						stringBuilder.Append((i + 1).ToString());
						stringBuilder.Append("/");
						stringBuilder.Append(items.Count.ToString());
						stringBuilder.Append("] ");
						stringBuilder.Append("  %");
						stringBuilder.Append((item.progress * 100).ToString("N2"));
						stringBuilder.Append("  ");
						stringBuilder.AppendLine(item.name);
					}

					Log.Info(stringBuilder.ToString());

					/*
					for (int i = 0; i < items.Count; i++)
					{
						var item = items[i];
						var json = Newtonsoft.Json.JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
						Log.Info($"Progress details [{(i + 1)}/{items.Count}]:\n{json}");
					}
					*/
				}
				catch (Exception exception)
				{
					Log.Error("Failed to log Unity Progress details. Exception: " + exception);
				}

				await Task.Yield();
			}

			Log.Info("Completed waiting for Unity Progress to finish at '" + subject + "'.");
		}

		#region Log

		private static readonly Logger Log = new(nameof(ProgressTools));

		#endregion
	}

}