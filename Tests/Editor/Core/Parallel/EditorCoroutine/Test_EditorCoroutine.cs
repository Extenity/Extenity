using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Extenity;
using Extenity.ParallelToolbox.Editor;
using Extenity.Testing;
using UnityEngine.TestTools;

namespace ExtenityTests.ParallelToolbox.Editor
{
	internal class DummyEditorWindow : EditorWindow
	{

	}

	[TestFixture]
	public class Test_EditorCoroutine : ExtenityTestBase
	{
		const float waitTime = 1.0f; //wait time in seconds

		IEnumerator ExecuteRoutineYieldingArbitraryEnumerator(IEnumerator enumerator)
		{
			Log.Info("PreExecution");
			yield return enumerator;
			Log.Info("PostExecution");
		}

		IEnumerator ExecuteRoutineWithWaitForSeconds()
		{
			Log.Info("PreExecution");
			yield return new WaitForSeconds(waitTime);
			Log.Info("PostExecution");
		}

		IEnumerator ExecuteNestedOwnerlessRoutinesWithWaitForSeconds()
		{
			Log.Info("Outer PreExecution");
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(ExecuteRoutineWithWaitForSeconds());
			Log.Info("Outer PostExecution");
		}

		[UnityTest]
		public IEnumerator Coroutine_LogsStepsAtExpectedTimes()
		{
			var currentWindow = EditorWindow.GetWindow<DummyEditorWindow>();

			currentWindow.StartCoroutine(ExecuteRoutineYieldingArbitraryEnumerator(null));
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "PreExecution"));
			yield return null;
			AssertExpectLog((LogType.Log, "PostExecution"));

			currentWindow.Close();
		}

		[UnityTest]
		public IEnumerator Coroutine_WaitsForSpecifiedNumberOfSeconds()
		{
			yield return new EnterPlayMode(); //both enter/exit play mode cause domain reload

			var currentWindow = EditorWindow.GetWindow<DummyEditorWindow>();
			try
			{
				currentWindow.StartCoroutine(ExecuteRoutineWithWaitForSeconds());
				yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

				double targetTime = EditorApplication.timeSinceStartup + waitTime;
				AssertExpectLog((LogType.Log, "PreExecution"));

				while (targetTime > EditorApplication.timeSinceStartup)
				{
					AssertExpectNoLogs();
					yield return null; //wait until target time is reached
				}
				// This is not cool but just allow a couple of seconds for the sake of timing differences.
				// Wait until a log appears in 2 seconds. Without this wait, the test randomly fails.
				for (int i = 1; i <= 10; i++)
				{
					if (!Logs.Contains(((LogType.Log), "PostExecution")))
					{
						Log.Info("# extra waiting " + i);
						yield return new WaitForSecondsRealtime(0.2f);
					}
				}

				AssertExpectLog((LogType.Log, "PostExecution"));
			}
			finally
			{
				Log.Info("# Closing the test window.");
				currentWindow.Close();
			}

			yield return new ExitPlayMode();
		}

		[UnityTest]
		public IEnumerator CoroutineWithArbitraryObject_StopsExecutionIfObjectIsCollected()
		{
			object obj = new object();
			EditorCoroutineUtility.StartCoroutine(ExecuteRoutineWithWaitForSeconds(), obj);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "PreExecution"));

			double targetTime = EditorApplication.timeSinceStartup + waitTime;
			while (targetTime + 0.5f > EditorApplication.timeSinceStartup) // Add a little more time to make sure the wait time gets passed.
			{
				if (obj != null && EditorApplication.timeSinceStartup > targetTime - (waitTime * 0.5f))
				{
					obj = null;
					GC.Collect(); //Halfway through the wait, collect the owner object
				}
				yield return null; //wait until target time is reached
			}

			AssertExpectNoLogs();
		}

		[UnityTest]
		public IEnumerator CoroutineWithArbitraryUnityEngineObject_StopsExecutionIfObjectIsCollected()
		{
			GameObject gameObject = new GameObject("TEST");
			EditorCoroutineUtility.StartCoroutine(ExecuteRoutineWithWaitForSeconds(), gameObject);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "PreExecution"));

			double targetTime = EditorApplication.timeSinceStartup + waitTime;
			while (targetTime + 0.5f > EditorApplication.timeSinceStartup) // Add a little more time to make sure the wait time gets passed.
			{
				if (gameObject != null && EditorApplication.timeSinceStartup > targetTime - (waitTime * 0.5f))
				{
					UnityEngine.Object.DestroyImmediate(gameObject);
					gameObject = null; //Immediately destroy the gameObject
				}
				yield return null; //wait until target time is reached
			}

			AssertExpectNoLogs();
		}

		[UnityTest]
		public IEnumerator NestedCoroutinesWithoutOwner_WaitForSpecificNumberOfSeconds()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ExecuteNestedOwnerlessRoutinesWithWaitForSeconds());
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Outer PreExecution"));
			yield return null; //schedule inner routine
			AssertExpectLog((LogType.Log, "PreExecution"));

			double targetTime = EditorApplication.timeSinceStartup + waitTime;
			while (targetTime > EditorApplication.timeSinceStartup)
			{
				AssertExpectNoLogs();
				yield return null; //wait until target time is reached
			}
			// This is not cool but just allow a couple of frames for the sake of timing differences.
			// Wait until a log appears in 5 frames. Without this wait, the test randomly fails.
			for (int i = 0; i < 5 && Logs.Count == 0; i++)
			{
				Log.Info("# extra waiting");
				yield return null;
			}

			AssertExpectLog((LogType.Log, "PostExecution"));
			yield return null; //exit inner coroutine
			AssertExpectLog((LogType.Log, "Outer PostExecution"));
		}

		private IEnumerator NestedIEnumeratorRoutine()
		{
			Log.Info("Start of nesting");
			yield return ExecuteRoutineYieldingArbitraryEnumerator(ExecuteRoutineYieldingArbitraryEnumerator(null));
			Log.Info("End of nesting");
		}

		[UnityTest]
		public IEnumerator CoroutineWithoutOwner_YieldingIEnumerator()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(NestedIEnumeratorRoutine());
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Start of nesting"));
			yield return null; //yield 1st nested IEnumerator
			AssertExpectLog((LogType.Log, "PreExecution"));
			yield return null; //yield 2nd nested IEnumerator
			AssertExpectLog((LogType.Log, "PreExecution"));
			yield return null; //execute 2nd IEnumerator
			AssertExpectLog((LogType.Log, "PostExecution"),
							(LogType.Log, "PostExecution"),
							(LogType.Log, "End of nesting"));
		}

		#region Tests - Nested

		[UnityTest]
		public IEnumerator RecursiveNestedCoroutines_Ownerless()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(-1, 5, 1));
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Log, "Nested 5 end"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 end"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 end"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 end"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 1 end"));
		}

		[UnityTest]
		public IEnumerator RecursiveYieldedCoroutines_Ownerless()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveYieldedCoroutine(-1, 5, 1));
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Log, "Nested 5 end"),
							(LogType.Log, "Nested 4 end"),
							(LogType.Log, "Nested 3 end"),
							(LogType.Log, "Nested 2 end"),
							(LogType.Log, "Nested 1 end"));
		}

		#endregion

		#region Tests - Exception - RoutineThrowingGUIException

		private IEnumerator RoutineThrowingGUIException()
		{
			Log.Info("PreException");
			yield return null;
			GUIUtility.ExitGUI();
			yield return null;
			Log.Info("PostException");
		}

		[UnityTest]
		public IEnumerator Exception_Simple_DoesNotHandleExitGUIException()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(RoutineThrowingGUIException());
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "PreException"));
			yield return null;
			AssertExpectLog((LogType.Exception, "ExitGUIException: Exception of type 'UnityEngine.ExitGUIException' was thrown."));
		}

		#endregion

		#region Tests - Exception - StackStopsWhenExceptionThrownInLeaf

		[UnityTest]
		public IEnumerator Exception_StackStopsWhenExceptionThrownInLeaf_RecursiveNestedCoroutines_Ownerless()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(5, 100, 1));
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Exception, "Exception: Nested 5 throws"));

			// The original (IMHO buggy) implementation logs these. The fixed one does not.
			//yield return null;
			//yield return null;
			//AssertExpectLog((LogType.Log, "Nested 4 end"));
			//yield return null;
			//AssertExpectLog((LogType.Log, "Nested 3 end"));
			//yield return null;
			//AssertExpectLog((LogType.Log, "Nested 2 end"));
			//yield return null;
			//AssertExpectLog((LogType.Log, "Nested 1 end"));
		}

		[UnityTest]
		public IEnumerator Exception_StackStopsWhenExceptionThrownInLeaf_RecursiveYieldedCoroutines_Ownerless()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveYieldedCoroutine(5, 100, 1));
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Exception, "Exception: Nested 5 throws"));

			// The original (IMHO buggy) implementation logs these. The fixed one does not.
			//yield return null;
			//yield return null;
			//AssertExpectLogs((LogType.Log, "Nested 4 end"),
			//				(LogType.Log, "Nested 3 end"),
			//				(LogType.Log, "Nested 2 end"),
			//				(LogType.Log, "Nested 1 end"));
		}

		#endregion

		#region Tests - Exception Catching - Simple

		private IEnumerator SimpleThrowingRoutine()
		{
			Log.Info("Routine start");
			Throw("Routine throws");
			Log.Info("Routine end"); // Nope, not happening because of the throw above.
			yield break;
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_Simple()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(SimpleThrowingRoutine(), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Routine start"),
							(LogType.Log, "Caught the exception (Main): Routine throws"));
		}

		#endregion

		#region Tests - Exception Catching - Second Nested

		private IEnumerator FirstRoutine()
		{
			Log.Info("First start");
			yield return EditorCoroutineUtility.StartCoroutineOwnerless(SecondRoutine(), OnExceptionAndCatch_Depth1);
			Log.Info("First end");
		}

		private IEnumerator SecondRoutine()
		{
			Log.Info("Second start");
			Throw("Second routine throws");
			Log.Info("Second end"); // Nope, not happening because of the throw above.
			yield break;
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_SecondNested()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(FirstRoutine(), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "First start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Second start"),
							(LogType.Log, "Caught the exception (Depth 1): Second routine throws"));
			yield return null;
			AssertExpectLog((LogType.Log, "First end"));
		}

		#endregion

		#region Tests - Exception Catching - Recursive

		[UnityTest]
		public IEnumerator ExceptionHandling_InMainCall_RecursiveNestedCoroutines_Ownerless_Depth1()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(1, 100, 1), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"),
							(LogType.Log, "Caught the exception (Main): Nested 1 throws"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_InMainCall_RecursiveNestedCoroutines_Ownerless_Depth2()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(2, 100, 1), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"),
							(LogType.Log, "Caught the exception (Main): Nested 2 throws"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_InMainCall_RecursiveNestedCoroutines_Ownerless_Depth5()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(5, 100, 1), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Log, "Caught the exception (Main): Nested 5 throws"));
		}

		// --------------------------------------------------------------------------------------------------------

		[UnityTest]
		public IEnumerator ExceptionHandling_InMainCall_RecursiveYieldedCoroutines_Ownerless_Depth1()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveYieldedCoroutine(1, 100, 1), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"),
							(LogType.Log, "Caught the exception (Main): Nested 1 throws"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_InMainCall_RecursiveYieldedCoroutines_Ownerless_Depth2()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveYieldedCoroutine(2, 100, 1), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"),
							(LogType.Log, "Caught the exception (Main): Nested 2 throws"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_InMainCall_RecursiveYieldedCoroutines_Ownerless_Depth5()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveYieldedCoroutine(5, 100, 1), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Log, "Caught the exception (Main): Nested 5 throws"));
		}

		// --------------------------------------------------------------------------------------------------------

		[UnityTest]
		public IEnumerator ExceptionHandling_InNestedCall_RecursiveNestedCoroutines_Ownerless_Depth1()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(1, 100, 1, new Func<Exception, bool>[] { OnExceptionAndCatch_Depth1, OnExceptionAndCatch_Depth2, OnExceptionAndCatch_Depth3, OnExceptionAndCatch_Depth4, OnExceptionAndCatch_Depth5 }), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"),
							(LogType.Log, "Caught the exception (Main): Nested 1 throws"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_InNestedCall_RecursiveNestedCoroutines_Ownerless_Depth2()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(2, 100, 1, new Func<Exception, bool>[] { OnExceptionAndCatch_Depth1, OnExceptionAndCatch_Depth2, OnExceptionAndCatch_Depth3, OnExceptionAndCatch_Depth4, OnExceptionAndCatch_Depth5 }), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"),
							(LogType.Log, "Caught the exception (Depth 1): Nested 2 throws"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 1 end"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_InNestedCall_RecursiveNestedCoroutines_Ownerless_Depth5()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(5, 100, 1, new Func<Exception, bool>[] { OnExceptionAndCatch_Depth1, OnExceptionAndCatch_Depth2, OnExceptionAndCatch_Depth3, OnExceptionAndCatch_Depth4, OnExceptionAndCatch_Depth5 }), OnExceptionAndCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Log, "Caught the exception (Depth 4): Nested 5 throws"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 end"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 end"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 end"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 1 end"));
		}

		// --------------------------------------------------------------------------------------------------------

		[UnityTest]
		public IEnumerator ExceptionHandling_PassExceptionInNestedCall_RecursiveNestedCoroutines_Ownerless_Depth1()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(1, 100, 1, new Func<Exception, bool>[] { OnExceptionButDontCatch_Depth1, OnExceptionButDontCatch_Depth2, OnExceptionButDontCatch_Depth3, OnExceptionButDontCatch_Depth4, OnExceptionButDontCatch_Depth5 }), OnExceptionButDontCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"),
							(LogType.Log, "Passed the exception (Main): Nested 1 throws"),
							(LogType.Exception, "Exception: Nested 1 throws"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_PassExceptionInNestedCall_RecursiveNestedCoroutines_Ownerless_Depth2()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(2, 100, 1, new Func<Exception, bool>[] { OnExceptionButDontCatch_Depth1, OnExceptionButDontCatch_Depth2, OnExceptionButDontCatch_Depth3, OnExceptionButDontCatch_Depth4, OnExceptionButDontCatch_Depth5 }), OnExceptionButDontCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"),
							(LogType.Log, "Passed the exception (Depth 1): Nested 2 throws"),
							(LogType.Log, "Passed the exception (Main): Nested 2 throws"),
							(LogType.Exception, "Exception: Nested 2 throws"));
		}

		[UnityTest]
		public IEnumerator ExceptionHandling_PassExceptionInNestedCall_RecursiveNestedCoroutines_Ownerless_Depth()
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(5, 100, 1, new Func<Exception, bool>[] { OnExceptionButDontCatch_Depth1, OnExceptionButDontCatch_Depth2, OnExceptionButDontCatch_Depth3, OnExceptionButDontCatch_Depth4, OnExceptionButDontCatch_Depth5 }), OnExceptionButDontCatch_Main);
			yield return null; AssertExpectNoLogs(); yield return null; // We start to get the logs 2 frames after.

			AssertExpectLog((LogType.Log, "Nested 1 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 2 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 3 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 4 start"));
			yield return null;
			AssertExpectLog((LogType.Log, "Nested 5 start"),
							(LogType.Log, "Passed the exception (Depth 4): Nested 5 throws"),
							(LogType.Log, "Passed the exception (Depth 3): Nested 5 throws"),
							(LogType.Log, "Passed the exception (Depth 2): Nested 5 throws"),
							(LogType.Log, "Passed the exception (Depth 1): Nested 5 throws"),
							(LogType.Log, "Passed the exception (Main): Nested 5 throws"),
							(LogType.Exception, "Exception: Nested 5 throws"));
		}

		#endregion

		#region Tools - Common Coroutines

		private IEnumerator ThrowingRecursiveNestedCoroutine(int throwAtRecursion, int maxDepth, int currentDepth, Func<Exception, bool>[] registerExceptionCatchersInDepths = null)
		{
			Log.Info("Nested " + currentDepth + " start");
			if (currentDepth == throwAtRecursion)
			{
				throw new Exception("Nested " + throwAtRecursion + " throws");
			}
			if (currentDepth < maxDepth)
			{
				var onException = registerExceptionCatchersInDepths != null && currentDepth <= registerExceptionCatchersInDepths.Length
						? registerExceptionCatchersInDepths[currentDepth - 1]
						: null;
				if (onException != null)
				{
					Log.Info("# Registering catcher : " + onException.Method.Name);
				}
				yield return EditorCoroutineUtility.StartCoroutineOwnerless(ThrowingRecursiveNestedCoroutine(throwAtRecursion, maxDepth, currentDepth + 1, registerExceptionCatchersInDepths), onException);
			}
			Log.Info("Nested " + currentDepth + " end");
		}

		private IEnumerator ThrowingRecursiveYieldedCoroutine(int throwAtRecursion, int maxDepth, int currentDepth)
		{
			Log.Info("Nested " + currentDepth + " start");
			if (currentDepth == throwAtRecursion)
			{
				throw new Exception("Nested " + throwAtRecursion + " throws");
			}
			if (currentDepth < maxDepth)
			{
				yield return ThrowingRecursiveYieldedCoroutine(throwAtRecursion, maxDepth, currentDepth + 1);
			}
			Log.Info("Nested " + currentDepth + " end");
		}

		#endregion

		#region Tools - Exception Handling

		private bool OnExceptionButDontCatch_Main(Exception exception)
		{
			Log.Info("Passed the exception (Main): " + exception.Message);
			return false;
		}

		private bool OnExceptionButDontCatch_Depth1(Exception exception)
		{
			Log.Info("Passed the exception (Depth 1): " + exception.Message);
			return false;
		}

		private bool OnExceptionButDontCatch_Depth2(Exception exception)
		{
			Log.Info("Passed the exception (Depth 2): " + exception.Message);
			return false;
		}

		private bool OnExceptionButDontCatch_Depth3(Exception exception)
		{
			Log.Info("Passed the exception (Depth 3): " + exception.Message);
			return false;
		}

		private bool OnExceptionButDontCatch_Depth4(Exception exception)
		{
			Log.Info("Passed the exception (Depth 4): " + exception.Message);
			return false;
		}

		private bool OnExceptionButDontCatch_Depth5(Exception exception)
		{
			Log.Info("Passed the exception (Depth 5): " + exception.Message);
			return false;
		}

		private bool OnExceptionAndCatch_Main(Exception exception)
		{
			Log.Info("Caught the exception (Main): " + exception.Message);
			return true;
		}

		private bool OnExceptionAndCatch_Depth1(Exception exception)
		{
			Log.Info("Caught the exception (Depth 1): " + exception.Message);
			return true;
		}

		private bool OnExceptionAndCatch_Depth2(Exception exception)
		{
			Log.Info("Caught the exception (Depth 2): " + exception.Message);
			return true;
		}

		private bool OnExceptionAndCatch_Depth3(Exception exception)
		{
			Log.Info("Caught the exception (Depth 3): " + exception.Message);
			return true;
		}

		private bool OnExceptionAndCatch_Depth4(Exception exception)
		{
			Log.Info("Caught the exception (Depth 4): " + exception.Message);
			return true;
		}

		private bool OnExceptionAndCatch_Depth5(Exception exception)
		{
			Log.Info("Caught the exception (Depth 5): " + exception.Message);
			return true;
		}

		#endregion
	}
}
