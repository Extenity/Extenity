#if UNITY

using Extenity.MessagingToolbox;
using UnityEngine;

namespace Extenity.ScreenToolbox
{

	/// <summary>
	/// Tracks screen state changes and emits OnScreenModified event.
	/// Works in both Editor and Runtime.
	/// </summary>
	public static class ScreenTracker
	{
		#region Initialization

		static ScreenTracker()
		{
			GetInitialState();

			RegisterIntoApplicationLoop();
		}

		private static void GetInitialState()
		{
			Info.CollectInfoFromUnity();
			Info.CopyTo(PreviousInfo);
		}

		private static void RegisterIntoApplicationLoop()
		{
#if UNITY_EDITOR
			// Tried to make it work in Editor but Unity makes things too complicated
			// by not telling the actual game screen's resolution with Screen.width
			// and Screen.height when working in Edit mode. So disabled until finding
			// a more robust way to get screen size. See 11653662.
			// if (!Application.isPlaying)
			// {
			// 	UnityEditor.EditorApplication.update -= CustomUpdate;
			// 	UnityEditor.EditorApplication.update += CustomUpdate;
			// 	Log.Info("Initialized ScreenTracker in Editor");
			// 	return;
			// }
			// else
			// {
			// 	UnityEditor.EditorApplication.update -= CustomUpdate;
			// }
			//
			// This should be removed if the feature above is implemented in future.
			if (!Application.isPlaying)
			{
				// Skip initialization in Edit mode.
				return;
			}
#endif

			Loop.DeregisterPreUpdate(CustomUpdate);
			Loop.RegisterPreUpdate(CustomUpdate);
			// Log.Info("Initialized ScreenTracker");
		}

		#endregion

		#region Update

		private static void CustomUpdate()
		{
			Info.CollectInfoFromUnity();

			if (!Info.Equals(PreviousInfo))
			{
				Info.CopyTo(PreviousInfo);

				// Emitting is not done instantly. Wait for one more frame when a change is detected.
				// Unity tends to partially modify screen state which are spread to multiple frames.
				// So we wait for state changes to come to a stop. Then we emit the modification event
				// to merge all modifications into one event.
				EmitModifiedEventInNextFrame = true;
				return;
			}

			if (EmitModifiedEventInNextFrame)
			{
				EmitModifiedEventInNextFrame = false;
				OnScreenModified.InvokeSafe(Info);
			}
		}

		#endregion

		#region Events

		public class ScreenModifiedEvent : ExtenityEvent<ScreenInfo> { }

		public static readonly ScreenModifiedEvent OnScreenModified = new ScreenModifiedEvent();

		private static bool EmitModifiedEventInNextFrame;

		#endregion

		#region Data

		public static readonly ScreenInfo Info = new ScreenInfo();

		// Note that it's strictly made private. There is the EmitModifiedEventInNextFrame
		// system that merges more than one ScreenInfos spread across frames. So PreviousInfo
		// won't be telling the state at the time when previous OnScreenModified event emitted.
		private static readonly ScreenInfo PreviousInfo = new ScreenInfo();

		#endregion
	}

}

#endif
