#if UNITY_2018_1_OR_NEWER

using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using UnityEditor;
using UnityEngine;

using UnityEngine.Profiling;

namespace Extenity.UnityEditorToolbox.Editor
{

	/// <summary>
	/// Based on Lotte Makes Stuff code.
	/// https://gist.github.com/LotteMakesStuff/8534e01043826754344a570a4cf21002
	/// https://twitter.com/LotteMakesStuff
	/// 
	/// Modifications by Can Baycay:
	///		Refactor: Code formatting.
	///		Add: Search field on top of the window.
	/// </summary>
	public class PlayerLoopWindow : EditorWindow
	{
		#region Initialization / Deinitialization

		[MenuItem(ExtenityMenu.Application + "Player Loop Visualizer", priority = ExtenityMenu.ApplicationPriorityEnd)]
		private static void Init()
		{
			// Get existing open window or if none, make a new one:
			var window = (PlayerLoopWindow)GetWindow(typeof(PlayerLoopWindow));
			window.Show();
		}

		private void OnEnable()
		{
			titleContent = new GUIContent("Player Loop");

			HasCustomPlayerLoop = EditorPrefs.GetBool("PlayerLoopWin_hasCustom", false);

			autoRepaintOnSceneChange = true;
		}

		private void OnDisable()
		{
			EditorPrefs.SetBool("PlayerLoopWin_hasCustom", HasCustomPlayerLoop);
		}

		#endregion

		#region GUI

		private static bool HasCustomPlayerLoop = false;
		private static bool IsProfilerActive = false;
		private static UnityEngine.LowLevel.PlayerLoopSystem CurrentPlayerLoop = new UnityEngine.LowLevel.PlayerLoopSystem();
		private static UnityEngine.LowLevel.PlayerLoopSystem NextPlayerLoop; // used for storing changes to the loop. Applied at the end of the GUI draw.
		private static bool HasUpdated = false;
		private Vector2 Scroll;
		private string SearchInput;

		private static string InfoBoxText =
@"The PlayerLoopSystem Struct represents a single system in the Player Loop. The loop is actually a tree-like structure, each System can store a list of subsystems (PlayerLoopSystem.subSystemList). In this visualization, we have drawn each top-level system as a fold out containing its subsystems.  This makes it super easy for us to inspect exactly what is happening in each main phase of a frame update.

We can plug in our own PlayerLoopSystem by adding it to the subsystem list for whichever update phase we want it to execute in. Click the 'Add/Remove Custom System button' to add two demo systems to the Update Phase. We can even remove whole subsystems from the update! to experiment with this click the [x] button next to a systems entry.";

		private void OnGUI()
		{
			GUILayout.Label("Player Loop Visualizer", EditorStyles.boldLabel);

			// Search bar
			GUILayout.BeginHorizontal(GUI.skin.FindStyle("Toolbar"));
			SearchInput = GUILayout.TextField(SearchInput, GUI.skin.FindStyle("ToolbarSeachTextField"));
			if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton")))
			{
				// Remove focus if cleared
				SearchInput = "";
				GUI.FocusControl(null);
			}
			GUILayout.EndHorizontal();

			// Check to see if we need to initialize the PlayerLoopSystem. 
			if (CurrentPlayerLoop.subSystemList == null)
			{
				if (HasCustomPlayerLoop)
				{
					// if were expecting a custom loop, use Generate Custom
					CurrentPlayerLoop = GenerateCustomLoop();
					UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(CurrentPlayerLoop);
				}
				else
				{
					// Otherwise grab the default loop
					CurrentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
				}
			}

			// Draw the entier list out in a scrollable area (it gets really big!)
			Scroll = EditorGUILayout.BeginScrollView(Scroll, GUIStyle.none, GUI.skin.verticalScrollbar);
			foreach (var loopSystem in CurrentPlayerLoop.subSystemList)
			{
				DrawSubsystemList(loopSystem, 0);

				if (HasUpdated)
				{
					HasUpdated = false;
					CurrentPlayerLoop = NextPlayerLoop;
					UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(CurrentPlayerLoop);
				}
			}
			EditorGUILayout.EndScrollView();

			// Draw out a documentation help box
			EditorGUILayout.HelpBox(InfoBoxText, MessageType.Info, true);

			// and finally draw our demo interaction buttons!
			GUILayout.BeginHorizontal(EditorStyles.helpBox);
			if (!HasCustomPlayerLoop)
			{
				if (GUILayout.Button("Add Custom System"))
				{
					HasCustomPlayerLoop = true;
					CurrentPlayerLoop = GenerateCustomLoop();
					UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(CurrentPlayerLoop);
				}
			}
			else
			{
				if (GUILayout.Button("Remove Custom System"))
				{
					HasCustomPlayerLoop = false;
					CurrentPlayerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
					UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(CurrentPlayerLoop);

					EditorApplication.QueuePlayerLoopUpdate();
				}
			}
			if (GUILayout.Button(IsProfilerActive ? "Disable Profiler" : "Enable Profiler"))
			{
				// simply toggle the profiler bool
				IsProfilerActive = !IsProfilerActive;
			}

			if (GUILayout.Button("Reset"))
			{
				// nulls the cached player loop system so the default system is grabbed again
				HasCustomPlayerLoop = false;
				CurrentPlayerLoop = new UnityEngine.LowLevel.PlayerLoopSystem();
			}
			if (GUILayout.Button("Open Docs"))
			{
				Application.OpenURL("https://docs.unity3d.com/2018.1/Documentation/ScriptReference/Experimental.LowLevel.PlayerLoopSystem.html");
			}
			GUILayout.EndHorizontal();
		}

		#endregion

		#region Custom Loop Example

		public struct CustomPlayerLoopStartUpdate
		{
			public static UnityEngine.LowLevel.PlayerLoopSystem GetNewSystem()
			{
				return new UnityEngine.LowLevel.PlayerLoopSystem()
				{
					type = typeof(CustomPlayerLoopStartUpdate),
					updateDelegate = UpdateFunction
				};
			}

			public static void UpdateFunction()
			{
				// TODO: something useful here!
				Debug.Log("Starting Update");
			}
		}

		public struct CustomPlayerLoopEndUpdate
		{
			public static UnityEngine.LowLevel.PlayerLoopSystem GetNewSystem()
			{
				return new UnityEngine.LowLevel.PlayerLoopSystem()
				{
					type = typeof(CustomPlayerLoopEndUpdate),
					updateDelegate = UpdateFunction
				};
			}

			public static void UpdateFunction()
			{
				// TODO: something useful here!
				Debug.Log("Ending Update");
			}
		}

		private UnityEngine.LowLevel.PlayerLoopSystem GenerateCustomLoop()
		{
			// Note: this also resets the loop to its defalt state first.
			var playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
			HasCustomPlayerLoop = true;

			// Grab the 4th subsystem - This is the man Update Phase
			var update = playerLoop.subSystemList[4];

			// convert the subsytem array to a List to make it easier to work with...
			var newList = new List<UnityEngine.LowLevel.PlayerLoopSystem>(update.subSystemList);

			// add a demo system to the start of it (implementation at the end of this file)
			var beginUpdateSystem = new UnityEngine.LowLevel.PlayerLoopSystem();
			beginUpdateSystem.type = typeof(CustomPlayerLoopStartUpdate); // Unity uses the name of the type here to identify the System - we would see this show up in the Profiler for example
			beginUpdateSystem.updateDelegate = CustomPlayerLoopStartUpdate.UpdateFunction; // we can plug a C# method into this delegate to control what actually happens when this System updates
			newList.Insert(0, beginUpdateSystem); // Finally lets insert it into the front!

			// Also lets put one on the end (implementation at the end of this file)
			newList.Add(CustomPlayerLoopEndUpdate.GetNewSystem()); // this time, lets use a small static helper method i added to the Systems type to generate the PlayerLoopSystem. this pattern is much cleaner :)

			// convert the list back to an array and plug it into the Update system.
			update.subSystemList = newList.ToArray();

			// dont forget to put our newly edited System back into the main player loop system!!
			playerLoop.subSystemList[4] = update;
			return playerLoop;
		}

		#endregion

		#region Subsystem List

		private Stack<string> PathStack = new Stack<string>();
		private Stack<UnityEngine.LowLevel.PlayerLoopSystem> SystemStack = new Stack<UnityEngine.LowLevel.PlayerLoopSystem>();

		public bool GetFoldout(string key)
		{
			return EditorPrefs.GetBool("PlayerLoopWin_Foldout_" + key, false);
		}

		public void SetFoldout(string key, bool value)
		{
			EditorPrefs.SetBool("PlayerLoopWin_Foldout_" + key, value);
		}

		private void DrawSubsystemList(UnityEngine.LowLevel.PlayerLoopSystem system, int increment = 1)
		{
			// here were using a stack to generate a path name for the PlayerLoopSystem were currently trying to draw
			// e.g Update.ScriptRunBehaviourUpdate. Unity uses these path names when storing profiler data on a step
			// that means  we can use these path names to retrieve profiler samples!
			if (PathStack.Count == 0)
			{
				// if this is a root object, add its name to the stack
				PathStack.Push(system.type.Name);
			}
			else
			{
				// otherwise add its name to its parents name...
				PathStack.Push(PathStack.Peek() + "." + system.type.Name);
			}

			using (new EditorGUI.IndentLevelScope(increment))
			{
				// if this System has Subsystems, draw a foldout
				var isHeader = system.subSystemList != null;
				if (isHeader)
				{
					var name = system.type.Name;
					var fullName = system.type.FullName;
					// check fold

					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					var fold = EditorGUILayout.Foldout(GetFoldout(fullName), name, true); // use the GetFoldout helper method to see if its open or closed
					EditorGUILayout.EndHorizontal();

					if (fold)
					{
						// if the fold is open, draw all the Subsytems~
						for (var i = 0; i < system.subSystemList.Length; i++)
						{
							var loopSystem = system.subSystemList[i];
							// store the current system Useful if we need to know the parent of a system later
							SystemStack.Push(system);
							DrawSubsystemList(loopSystem);
							SystemStack.Pop();
						}
					}

					SetFoldout(fullName, fold);
				}
				else
				{
					if (string.IsNullOrEmpty(SearchInput) ||
						system.type.Name.IndexOf(SearchInput, StringComparison.InvariantCultureIgnoreCase) >= 0 || // First search if the search word is included in the text because Liquid Metal only looks for the beginning of words.
					    LiquidMetalStringMatcher.Score(system.type.Name, SearchInput) > 0.8f // Then see if Liquid Metal is happy with the result. This allows searching by entering only some of the initial characters of words.
					)
					{
						// at the moment, all the defaut 'native' Systems update via a updateFunction (essentally, a pointer into the unmanaged C++ side of the engine.
						// So we can tell if a system is a custom one because it has a value in updateDelegate instead. So if this is a custom system, make note of that
						// so we can change how its drawn later
						var isCustom = system.updateDelegate != null;
						using (new EditorGUI.DisabledScope(isCustom))
						{
							EditorGUILayout.BeginHorizontal();
							GUILayout.Space((float)EditorGUI.indentLevel * 18f); // indent the entry nicley. We have to do this manually cos the flexible space at the end conflicts with GUI.Indent

							// draw the remove button...
							if (GUILayout.Button("x"))
							{
								RemoveSystem(system, SystemStack.Peek());
							}
							GUILayout.Label(system.type.Name); // draw the name out....

							// If the profiling mode is enabled, get the profiler sampler for this System and display its execution times!
							if (IsProfilerActive)
							{
								var sampler = Sampler.Get(PathStack.Peek());

								var info = "";
								if (sampler.GetRecorder().elapsedNanoseconds != 0)
								{
									info = (sampler.GetRecorder().elapsedNanoseconds / 1000000f) + " ms";
								}
								else
								{
									info = "0.000000 ms";
								}

								using (new EditorGUI.DisabledScope(true))
								{
									GUILayout.Label("[" + info + "]");
								}
							}

							GUILayout.FlexibleSpace();
							EditorGUILayout.EndHorizontal();
							//EditorGUILayout.LabelField(new GUIContent(/*"custom"*/));//, EditorGUIUtility.IconContent("cs Script Icon"));
						}
					}
				}
			}

			PathStack.Pop();
		}

		#endregion

		#region Subsystem Operations

		private void RemoveSystem(UnityEngine.LowLevel.PlayerLoopSystem target, UnityEngine.LowLevel.PlayerLoopSystem playerLoopSystem)
		{
			// LIMITATION assumes that systems are never stacked more than one level deep (e.g Update.CustomThing.CoolSystem will not work! one level too deep
			// Hell im not even sure if that works in general? it seems like it should but ive not tried it... still thought it was best to flag it up here...
			for (int i = 0; i < playerLoopSystem.subSystemList.Length; i++)
			{
				var system = playerLoopSystem.subSystemList[i];
				if (system.type == target.type)
				{
					// create a list of the subsystems, its easier to work with 
					var newList = new List<UnityEngine.LowLevel.PlayerLoopSystem>(playerLoopSystem.subSystemList);

					// remove the target
					newList.RemoveAt(i);
					playerLoopSystem.subSystemList = newList.ToArray();

					NextPlayerLoop = CurrentPlayerLoop; // copy the current loop...
														// and plug in our updated parent into it.
					for (int j = 0; j < CurrentPlayerLoop.subSystemList.Length; j++)
					{
						if (CurrentPlayerLoop.subSystemList[j].type == playerLoopSystem.type)
						{
							CurrentPlayerLoop.subSystemList[j] = playerLoopSystem;
						}
					}
					// then flag that it needs to be applied at the end of the GUI draw
					HasUpdated = true;
				}
			}
		}

		#endregion
	}

}

#endif
