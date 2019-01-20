#if BeyondAudioUsesWwiseAudio

using System;
using System.Collections.Generic;
using System.IO;
using Extenity.DataToolbox;

namespace Extenity.BeyondAudio.BankInfo
{

	public class EventInfo
	{
		public uint ID;
		public string Name;
		public string WwiseObjectPath;
		//public string Notes; // It's also possible to get this info if you wish.
		//public string Bank; // It's also possible to get this info if you wish.

		public string ParentDirectory => WwiseObjectPath?.GetParentDirectoryName();
	}

	public class StateInfo
	{
		public uint ID;
		public string Name;
		public string StateGroup;
		//public string Notes; // It's also possible to get this info if you wish.
		//public string Bank; // It's also possible to get this info if you wish.
	}

	public class GameParameterInfo
	{
		public uint ID;
		public string Name;
		public string WwiseObjectPath;
		//public string Notes; // It's also possible to get this info if you wish.
		//public string Bank; // It's also possible to get this info if you wish.

		public string ParentDirectory => WwiseObjectPath?.GetParentDirectoryName();
	}

	public class AudioBusInfo
	{
		public uint ID;
		public string Name;
		public string WwiseObjectPath;
		//public string Notes; // It's also possible to get this info if you wish.
		//public string Bank; // It's also possible to get this info if you wish.

		public string ParentDirectory => WwiseObjectPath?.GetParentDirectoryName();
	}

	public class WwiseBankContentInfo
	{
		public readonly List<EventInfo> Events = new List<EventInfo>();
		public readonly List<StateInfo> States = new List<StateInfo>();
		public readonly List<GameParameterInfo> GameParameters = new List<GameParameterInfo>();
		public readonly List<AudioBusInfo> AudioBuses = new List<AudioBusInfo>();

		//public void Clear()
		//{
		//	Events.Clear();
		//	States.Clear();
		//	GameParameters.Clear();
		//	AudioBuses.Clear();
		//}

		public void GatherDataFromStreamingAssets()
		{
			var directoryPath = AkBasePathGetter.GetSoundbankBasePath();
			foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.txt", SearchOption.AllDirectories))
			{
				GatherDataFromTextFile(filePath);
			}
		}

		public void GatherDataFromTextFile(string filePath)
		{
			var lines = File.ReadAllLines(filePath);
			for (var iLine = 0; iLine < lines.Length; iLine++)
			{
				var line = lines[iLine];
				if (line.StartsWith("Event\t"))
				{
					iLine++;
					for (; iLine < lines.Length; iLine++)
					{
						line = lines[iLine];
						if (string.IsNullOrEmpty(line))
							break;
						ReadLine_Event(line);
					}
				}
				else if (line.StartsWith("State\t"))
				{
					iLine++;
					for (; iLine < lines.Length; iLine++)
					{
						line = lines[iLine];
						if (string.IsNullOrEmpty(line))
							break;
						ReadLine_State(line);
					}
				}
				else if (line.StartsWith("Game Parameter\t"))
				{
					iLine++;
					for (; iLine < lines.Length; iLine++)
					{
						line = lines[iLine];
						if (string.IsNullOrEmpty(line))
							break;
						ReadLine_GameParameter(line);
					}
				}
				else if (line.StartsWith("Audio Bus\t"))
				{
					iLine++;
					for (; iLine < lines.Length; iLine++)
					{
						line = lines[iLine];
						if (string.IsNullOrEmpty(line))
							break;
						ReadLine_AudioBus(line);
					}
				}
			}
		}

		private void ReadLine_Event(string line)
		{
			var split = line.Split(new[] { '\t' }, StringSplitOptions.None);
			Events.Add(new EventInfo
			{
				ID = uint.Parse(split[1]),
				Name = split[2],
				WwiseObjectPath = split[5],
			});
		}

		private void ReadLine_State(string line)
		{
			var split = line.Split(new[] { '\t' }, StringSplitOptions.None);
			States.Add(new StateInfo
			{
				ID = uint.Parse(split[1]),
				Name = split[2],
				StateGroup = split[3],
			});
		}

		private void ReadLine_GameParameter(string line)
		{
			var split = line.Split(new[] { '\t' }, StringSplitOptions.None);
			GameParameters.Add(new GameParameterInfo
			{
				ID = uint.Parse(split[1]),
				Name = split[2],
				WwiseObjectPath = split[5],
			});
		}

		private void ReadLine_AudioBus(string line)
		{
			var split = line.Split(new[] { '\t' }, StringSplitOptions.None);
			AudioBuses.Add(new AudioBusInfo
			{
				ID = uint.Parse(split[1]),
				Name = split[2],
				WwiseObjectPath = split[5],
			});
		}

		public void DebugLog()
		{
			Log.Info($"Events ({Events.Count}):");
			foreach (var entry in Events)
			{
				Log.Info($"{entry.ID}\t| <b>{entry.Name}</b>\t| {entry.ParentDirectory}\t| {entry.WwiseObjectPath}");
			}

			Log.Info($"States ({States.Count}):");
			foreach (var entry in States)
			{
				Log.Info($"{entry.ID}\t| <b>{entry.Name}</b>\t| {entry.StateGroup}");
			}

			Log.Info($"GameParameters ({GameParameters.Count}):");
			foreach (var entry in GameParameters)
			{
				Log.Info($"{entry.ID}\t| <b>{entry.Name}</b>\t| {entry.ParentDirectory}\t| {entry.WwiseObjectPath}");
			}

			Log.Info($"AudioBuses ({AudioBuses.Count}):");
			foreach (var entry in AudioBuses)
			{
				Log.Info($"{entry.ID}\t| <b>{entry.Name}</b>\t| {entry.ParentDirectory}\t| {entry.WwiseObjectPath}");
			}
		}
	}

}

#endif
