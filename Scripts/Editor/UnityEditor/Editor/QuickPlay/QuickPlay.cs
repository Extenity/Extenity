// QuickPlay shortcuts are not supported outside of Windows environment.
#if UNITY_EDITOR_WIN

using System;
using System.Linq;
using Extenity.ApplicationToolbox;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Extenity.UnityEditorToolbox.Editor
{


	[InitializeOnLoad]
	public static class QuickPlay
	{
		#region Configuration

		public static readonly VirtualKeyCode[] DefaultShortcutKeyCombination =
		{
			VirtualKeyCode.LControl,
			VirtualKeyCode.LShift,
			VirtualKeyCode.X,
		};

		public static readonly VirtualKeyCode[][] PredefinedShortcutKeyCombinations =
		{
			new []{
				VirtualKeyCode.LControl,
				VirtualKeyCode.LShift,
				VirtualKeyCode.X,
			},
			new []{
				VirtualKeyCode.RControl,
				VirtualKeyCode.RWin,
				VirtualKeyCode.X,
			},
		};

		public static readonly QuickPlayCommand[] Commands =
		{
			new QuickPlayCommand_ClearConsole(),
			new QuickPlayCommand_StopPlaying(),
			new QuickPlayCommand_RefreshAssetDatabase(),
			new QuickPlayCommand_StartPlaying(),
		};

		#endregion

		#region Initialization

		static QuickPlay()
		{
			InitializeAndCheckPostponedCommands();
		}

		[DidReloadScripts]
		private static void InitializeAndCheckPostponedCommands()
		{
			// Mark it as currently processing at the start of everything, until proven otherwise.
			// It will be set to false below, if there is no postponed process detected.
			IsProcessing = true;

			// Register to editor updates for detecting global shortcut keys.
			EditorApplication.update -= DetectGlobalShortcutKeyPress;
			EditorApplication.update += DetectGlobalShortcutKeyPress;

			// See if there is a postponed command, which should be continued after assembly reload.
			{
				int continueFromCommand = -1;
				for (var i = 0; i < Commands.Length; i++)
				{
					var command = Commands[i];
					if (command.CheckAfterAssemblyReload())
					{
						continueFromCommand = i;
						break;
					}
				}

				if (continueFromCommand < 0)
				{
					IsProcessing = false;
				}
				else
				{
					IsProcessing = true;
					EditorApplication.delayCall += () =>
					{
						Process(continueFromCommand);
					};
				}
			}
		}

		#endregion

		#region Global Shortcut Key Press

		private static bool IsKeyCombinationDown = true;

		private static void DetectGlobalShortcutKeyPress()
		{
			if (IsProcessing)
				return;

			var currentlyPressing = true;
			for (var i = 0; i < ShortcutKeyCombination.Length; i++)
			{
				if (!OperatingSystemTools.IsAsyncKeyStateDown(ShortcutKeyCombination[i]))
				{
					currentlyPressing = false;
					break;
				}
			}

			if (currentlyPressing)
			{
				if (!IsKeyCombinationDown) // Prevent calling refresh multiple times before user releases the keys
				{
					IsKeyCombinationDown = true;
					Process(-1);
				}
			}
			else
			{
				IsKeyCombinationDown = false;
			}
		}

		#endregion

		#region Global Shortcut Key Preference

		private const string ShortcutKeyPrefKey = "QuickPlay.Shortcut";

		public static string ShortcutKeyCombinationToString(VirtualKeyCode[] keys)
		{
			return string.Join(", ", keys);
		}

		public static VirtualKeyCode[] ShortcutKeyCombinationFromString(string keysString)
		{
			return keysString.Split(',')
				.Where(item => !string.IsNullOrWhiteSpace(item))
				.Select(item =>
				{
					Enum.TryParse(item.Trim(), true, out VirtualKeyCode result);
					if ((int)result == 0)
					{
						throw new Exception($"Invalid key '{item.Trim()}'");
					}
					return result;
				})
				.ToArray();
		}

		private static VirtualKeyCode[] _ShortcutKeyCombination;
		public static VirtualKeyCode[] ShortcutKeyCombination
		{
			get
			{
				if (_ShortcutKeyCombination == null)
				{
					try
					{
						var keysString = EditorPrefs.GetString(ShortcutKeyPrefKey, null);
						_ShortcutKeyCombination = ShortcutKeyCombinationFromString(keysString);
					}
					catch
					{
						// Ignored.
					}

					if (_ShortcutKeyCombination == null || _ShortcutKeyCombination.Length == 0)
					{
						_ShortcutKeyCombination = DefaultShortcutKeyCombination;
					}
				}
				return _ShortcutKeyCombination;
			}
			set
			{
				_ShortcutKeyCombination = value;
				EditorPrefs.SetString(ShortcutKeyPrefKey, ShortcutKeyCombinationToString(value));
			}
		}


		#endregion

		#region Process

		private static bool IsProcessing;

		public static Action OnProcessSucceeded;

		private static void Process(int continueFromCommand)
		{
			IsProcessing = true;

			Log.Info("QuickPlay triggered." + (continueFromCommand >= 0 ? $" Continuing from command '{Commands[continueFromCommand].PrettyName}'." : ""));

			if (continueFromCommand < 0)
			{
				continueFromCommand = 0;
			}

			for (var i = continueFromCommand; i < Commands.Length; i++)
			{
				var command = Commands[i];

				command.Process();

				switch (command.PostponeType)
				{
					case QuickPlayPostponeType.NotPostponed:
						{
							// Continue to the next command.
							continue;
						}
					case QuickPlayPostponeType.WaitingForAssemblyReload:
						{
							// Nothing to do here. We will wait for Unity to reload assemblies, then
							// eventually call the static constructor (InitializeOnLoad).
							break;
						}
					case QuickPlayPostponeType.WaitingForDelayedCall:
						{
							EditorApplication.delayCall += InitializeAndCheckPostponedCommands;
							break;
						}
					default:
						throw new ArgumentOutOfRangeException();
				}
				Log.Info($"QuickPlay command '{command.PrettyName}' postponed.");
				return;
			}

			IsProcessing = false;

			OnProcessSucceeded?.Invoke();
		}

		#endregion

		#region Log

		private static readonly Logger Log = new(nameof(QuickPlay));

		#endregion
	}

}

#endif
