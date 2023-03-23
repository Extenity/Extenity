#if UNITY

#define InitializationTrackerLogging
//#define InitializationTrackerVerboseLogging

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Extenity.DataToolbox;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Extenity.DebugToolbox
{

	// TODO: Keeping a list of all active and erroneous Trackers. Successfully finalized Trackers should be removed from the list.
	// TODO: Editor UI.

	#region Initialization Step

	public enum InitializationStepType : byte
	{
		ObjectInstantiation,
		MethodCall,
		CheckpointReach,
	}

	public struct InitializationStep
	{
		public readonly InitializationStepType StepType;
		/// <summary>
		/// Used for ObjectInstantiation
		/// </summary>
		public readonly Type Type;
		/// <summary>
		/// Used for MethodCall, CheckpointReach
		/// </summary>
		public readonly string Tag;

		public static InitializationStep ObjectInstantiation(Type typeOfObjectExpectedToBeInitialized)
		{
			if (typeOfObjectExpectedToBeInitialized == null)
				throw new ArgumentNullException();
			return new InitializationStep(InitializationStepType.ObjectInstantiation, typeOfObjectExpectedToBeInitialized, null);
		}

		public static InitializationStep MethodCall(string nameOfMethodExpectedToBeCalled)
		{
			if (string.IsNullOrWhiteSpace(nameOfMethodExpectedToBeCalled))
				throw new ArgumentNullException();
			return new InitializationStep(InitializationStepType.MethodCall, null, nameOfMethodExpectedToBeCalled);
		}

		public static InitializationStep CheckpointReach(string nameOfCheckpointExpectedToBeReached)
		{
			if (string.IsNullOrWhiteSpace(nameOfCheckpointExpectedToBeReached))
				throw new ArgumentNullException();
			return new InitializationStep(InitializationStepType.CheckpointReach, null, nameOfCheckpointExpectedToBeReached);
		}

		private InitializationStep(InitializationStepType stepType, Type type, string tag)
		{
			StepType = stepType;
			Type = type;
			Tag = tag;
		}

		public string ToLogFriendlyString()
		{
			string context;
			switch (StepType)
			{
				case InitializationStepType.ObjectInstantiation: context = Type.ToString(); break;
				case InitializationStepType.MethodCall: context = Tag; break;
				case InitializationStepType.CheckpointReach: context = Tag; break;
				default: throw new ArgumentOutOfRangeException();
			}
			return StepType + " of " + context;
		}
	}

	#endregion

	/// <summary>
	/// Tells if a system's initialization process completed as expected or not, by tracking object instantiation order
	/// and method call order. See 'Initialize' where you tell the Tracker which objects and methods are expected.
	/// Then call 'InformObjectInstantiation' and 'InformMethodCall' methods at the right places in you code.
	/// The Tracker then tells you if the application successfully instantiated expected objects and made necessary calls
	/// to methods, and tells if these are happened in the expected order.
	///
	/// Only the first encountered failure will be logged to prevent bloat. It will be logged with Fatal which
	/// logs as an exception but won't break the execution. It will also be sent to Unity Cloud Diagnostics.
	/// 
	/// Inspecting the Tracker Editor tool is also a great way to explore what went wrong in an initialization process.
	/// 
	/// A Tracker object is not reusable and meant to be used only once. 'ActiveTrackers' will keep record of all Trackers
	/// currently in process, or completed but failed ones. Successfully completed trackers will be released from memory.
	/// </summary>
	public class InitializationTracker
	{
		#region Initialization

		public InitializationTracker(InitializationStep[] expectedInitializationStepsInOrder)
		{
			if (expectedInitializationStepsInOrder.IsNullOrEmpty())
			{
				// Don't have anything to do here. Just mark it as completed.
				Finalize(true);
				return;
			}

			ExpectedInitializationStepsInOrder = expectedInitializationStepsInOrder;

			InitializeHistory(expectedInitializationStepsInOrder.Length); // Capacity is the count of expected steps. See 98157.
		}

		#endregion

		#region Finalization

		public bool IsFinalized { get; private set; }
		public bool IsCancelled { get; private set; }

		private void Finalize(bool succeeded)
		{
			LogVerbose($"{nameof(Finalize)} called with succeeded '{succeeded}'.");
			if (IsFinalized)
			{
				// Finalization should only be called once.
				Log.InternalError(17541957);
				return;
			}

			IsFinalized = true;

			// Release resources if succeeded. If not, keep the history so that it can be logged whenever it's needed.
			if (succeeded)
			{
				ExpectedInitializationStepsInOrder = null;
				DeinitializeHistory();
			}
		}

		/// <summary>
		/// Gracefully cancels the initialization process without raising any alarms. No matter if the process
		/// was proceeding okay or proceeding with errors, the Tracker is assumed to be finalized successfully.
		/// Tracker Editor will not display the process.
		///
		/// Note that this will only cancel an ongoing process. An already finalized process will be visible
		/// in Tracker Editor and will tell what went wrong in case it was finalized with failures.
		/// </summary>
		public void CancelIfNotFinalized()
		{
			LogVerbose($"{nameof(CancelIfNotFinalized)} called while finalized '{IsFinalized}'.");
			if (IsFinalized)
				return;
			IsCancelled = true;
			Finalize(true);
		}

		/// <summary>
		/// Call this when you know something went wrong in the process and the Tracker should know it too.
		/// Tracker will no longer track events. Tracker Editor will display the failed process.
		/// </summary>
		public void GiveUpIfNotFinalized()
		{
			LogVerbose($"{nameof(GiveUpIfNotFinalized)} called while finalized '{IsFinalized}'.");
			if (IsFinalized)
				return;
			Finalize(false);
		}

		#endregion

		#region Expected Initialization Steps

		private InitializationStep[] ExpectedInitializationStepsInOrder;

		#endregion

		#region Initialization History

		private struct InitializationHistoryEntry
		{
			public readonly InitializationStepType StepType;

			// Object related
			public readonly Type Type;
			public readonly MonoBehaviour Component;
			public bool StillExists => Component;

			// Method and Checkpoint related
			public readonly string Tag;

			public InitializationHistoryEntry(InitializationStepType stepType, Type type, MonoBehaviour component, string tag)
			{
				StepType = stepType;
				Type = type;
				Component = component;
				Tag = tag;
			}
		}

		private List<InitializationHistoryEntry> InitializationHistoryInOrder;

		private void InitializeHistory(int capacity)
		{
			InitializationHistoryInOrder = new List<InitializationHistoryEntry>(capacity);
		}

		private void DeinitializeHistory()
		{
			if (InitializationHistoryInOrder != null)
			{
				InitializationHistoryInOrder.Clear();
				InitializationHistoryInOrder = null;
			}
		}

		private bool IsInstantiated(Type type)
		{
			for (int i = 0; i < InitializationHistoryInOrder.Count; i++)
			{
				var entry = InitializationHistoryInOrder[i];
				if (entry.StepType == InitializationStepType.ObjectInstantiation && entry.Type == type)
					return true;
			}
			return false;
		}

		private bool IsMethodCalled(string methodName)
		{
			for (int i = 0; i < InitializationHistoryInOrder.Count; i++)
			{
				var entry = InitializationHistoryInOrder[i];
				if (entry.StepType == InitializationStepType.MethodCall && entry.Tag == methodName)
					return true;
			}
			return false;
		}

		private bool IsCheckpointReached(string checkpointName)
		{
			for (int i = 0; i < InitializationHistoryInOrder.Count; i++)
			{
				var entry = InitializationHistoryInOrder[i];
				if (entry.StepType == InitializationStepType.CheckpointReach && entry.Tag == checkpointName)
					return true;
			}
			return false;
		}

		/// <summary>
		/// The initialization process will be finalized when there are enough history entries.
		/// Enough means the count of expected steps. If everything went right, the process
		/// will be finalized successfully. If something went wrong, the process will be finalized
		/// with failure.
		/// 
		/// See 98157.
		/// </summary>
		private void FinalizeIfHistoryIsFull()
		{
			if (InitializationHistoryInOrder.Count >= ExpectedInitializationStepsInOrder.Length)
			{
				Finalize(!IsFailed);
			}
		}

		#endregion

		#region Failure

		public bool IsFailed { get; private set; }

		private void NoteFailure(string message, Object context = null)
		{
			LogVerbose($"{nameof(NoteFailure)} called with message '{message}'.");
			if (IsFailed)
				return; // Ignore consecutive failures.
			IsFailed = true;
			Log.FatalWithContext(context, message);
		}

		#endregion

		#region Inform

		/// <summary>
		/// Simplified, log-only version of it's Inform_ variant.
		/// Useful when you just need to see what's happening, without processing.
		/// </summary>
		public void LogObjectInstantiation(MonoBehaviour component)
		{
			if (IsFinalized)
			{
				LogVerbose($"(LOGONLY)(FINALIZED) {nameof(LogObjectInstantiation)} called with component '{component}'.");
				return; // Do absolutely nothing if the process is finalized, for optimization's sake.
			}
			LogInfo($"(LOGONLY) {nameof(LogObjectInstantiation)} called with component '{component}'.");
		}

		/// <summary>
		/// You should inform the Tracker when an expected object is instantiated.
		/// Feel free to call it in at the most suitable time, like Awake, Start,
		/// OnEnable, network instantiation method or maybe the first Update.
		/// </summary>
		public void InformObjectInstantiation(MonoBehaviour component)
		{
			if (IsFinalized)
			{
				LogVerbose($"(FINALIZED) {nameof(InformObjectInstantiation)} called with component '{component}'.");
				return; // Do absolutely nothing if the process is finalized, for optimization's sake.
			}
			LogInfo($"{nameof(InformObjectInstantiation)} called with component '{component}'.");
			if (!component)
				throw new ArgumentNullException(nameof(component));
			var type = component.GetType();

			// Check if it was already instantiated before
			if (IsInstantiated(type))
			{
				NoteFailure($"Component '{type}' was already instantiated before.", component);
				//return; Nope. Add it to the list one more time. The list can contain more than one entries for the same object.
			}

			// Check if the object instantiation is expected
			{
				var index = InitializationHistoryInOrder.Count;
				Debug.Assert(index < ExpectedInitializationStepsInOrder.Length); // See 98157.
				var expectedEntry = ExpectedInitializationStepsInOrder[index];
				if (expectedEntry.StepType != InitializationStepType.ObjectInstantiation ||
					expectedEntry.Type != type)
				{
					NoteFailure($"Component '{type}' is instantiated while expecting '{expectedEntry.ToLogFriendlyString()}'", component);
				}
			}

			// Add it to the instantiation history
			InitializationHistoryInOrder.Add(new InitializationHistoryEntry(InitializationStepType.ObjectInstantiation, type, component, null));
			FinalizeIfHistoryIsFull();
		}

		/// <summary>
		/// Simplified, log-only version of it's Inform_ variant.
		/// Useful when you just need to see what's happening, without processing.
		/// </summary>
		public void LogMethodCall(string methodName, MonoBehaviour component = null)
		{
			if (IsFinalized)
			{
				LogVerbose($"(LOGONLY)(FINALIZED) {nameof(InformMethodCall)} called with method '{methodName}' and component '{component}'.");
				return; // Do absolutely nothing if the process is finalized, for optimization's sake.
			}
			LogInfo($"(LOGONLY) {nameof(InformMethodCall)} called with method '{methodName}' and component '{component}'.");
		}

		/// <summary>
		/// You should inform the Tracker when an expected method is called.
		/// Better call it at the first line of that method so any thrown
		/// exception somewhere in the middle won't prevent informing the
		/// Tracker.
		///
		/// If you also need to make sure the method execution must reach
		/// to the end without failure, use an 'InformCheckpoint' at the end
		/// of the method.
		///
		/// Remember to use 'nameof' to make life easier.
		/// </summary>
		public void InformMethodCall(string methodName, MonoBehaviour component = null)
		{
			if (IsFinalized)
			{
				LogVerbose($"(FINALIZED) {nameof(InformMethodCall)} called with method '{methodName}' and component '{component}'.");
				return; // Do absolutely nothing if the process is finalized, for optimization's sake.
			}
			LogInfo($"{nameof(InformMethodCall)} called with method '{methodName}' and component '{component}'.");
			if (string.IsNullOrEmpty(methodName))
				throw new ArgumentNullException(nameof(methodName));

			var componentAndMethodName = component
				? component.GetType().Name + ":" + methodName
				: methodName;

			// Check if the method was already called before
			if (IsMethodCalled(componentAndMethodName))
			{
				NoteFailure($"Method '{componentAndMethodName}' was already called before.", component);
				//return; Nope. Add it to the list one more time. The list can contain more than one entries for the same method.
			}

			// Check if the method call is expected
			{
				var index = InitializationHistoryInOrder.Count;
				Debug.Assert(index < ExpectedInitializationStepsInOrder.Length); // See 98157.
				var expectedEntry = ExpectedInitializationStepsInOrder[index];
				if (expectedEntry.StepType != InitializationStepType.MethodCall ||
					expectedEntry.Tag != componentAndMethodName)
				{
					NoteFailure($"Method '{componentAndMethodName}' is called while expecting '{expectedEntry.ToLogFriendlyString()}'", component);
				}
			}

			// Add it to the instantiation history
			InitializationHistoryInOrder.Add(new InitializationHistoryEntry(InitializationStepType.MethodCall, null, null, componentAndMethodName));
			FinalizeIfHistoryIsFull();
		}

		/// <summary>
		/// Simplified, log-only version of it's Inform_ variant.
		/// Useful when you just need to see what's happening, without processing.
		/// </summary>
		public void LogCheckpointReached(string checkpointName, Object context = null)
		{
			if (IsFinalized)
			{
				LogVerbose($"(LOGONLY)(FINALIZED) {nameof(InformCheckpointReached)} called with checkpoint '{checkpointName}'.");
				return; // Do absolutely nothing if the process is finalized, for optimization's sake.
			}
			LogInfo($"(LOGONLY) {nameof(InformCheckpointReached)} called with checkpoint '{checkpointName}'.");
		}

		/// <summary>
		/// You should inform the Tracker when an expected checkpoint is reached.
		///
		/// Remember to use 'nameof' if feasible to make life easier.
		/// </summary>
		public void InformCheckpointReached(string checkpointName, Object context = null)
		{
			if (IsFinalized)
			{
				LogVerbose($"(FINALIZED) {nameof(InformCheckpointReached)} called with checkpoint '{checkpointName}'.");
				return; // Do absolutely nothing if the process is finalized, for optimization's sake.
			}
			LogInfo($"{nameof(InformCheckpointReached)} called with checkpoint '{checkpointName}'.");
			if (string.IsNullOrEmpty(checkpointName))
				throw new ArgumentNullException(nameof(checkpointName));

			// Check if the checkpoint was already reached before
			if (IsCheckpointReached(checkpointName))
			{
				NoteFailure($"Checkpoint '{checkpointName}' was already reached before.", context);
				//return; Nope. Add it to the list one more time. The list can contain more than one entries for the same checkpoint.
			}

			// Check if the checkpoint reach is expected
			{
				var index = InitializationHistoryInOrder.Count;
				Debug.Assert(index < ExpectedInitializationStepsInOrder.Length); // See 98157.
				var expectedEntry = ExpectedInitializationStepsInOrder[index];
				if (expectedEntry.StepType != InitializationStepType.CheckpointReach ||
					expectedEntry.Tag != checkpointName)
				{
					NoteFailure($"Checkpoint '{checkpointName}' is reached while expecting '{expectedEntry.ToLogFriendlyString()}'", context);
				}
			}

			// Add it to the instantiation history
			InitializationHistoryInOrder.Add(new InitializationHistoryEntry(InitializationStepType.CheckpointReach, null, null, checkpointName));
			FinalizeIfHistoryIsFull();
		}

		#endregion

		#region Log

		private Logger Log = new("InitTrack");

		[Conditional("InitializationTrackerLogging")]
		private void LogInfo(string message)
		{
			Log.Info(message);
		}

		[Conditional("InitializationTrackerVerboseLogging")]
		private void LogVerbose(string message)
		{
			Log.Verbose(message);
		}

		#endregion
	}

}

#endif
