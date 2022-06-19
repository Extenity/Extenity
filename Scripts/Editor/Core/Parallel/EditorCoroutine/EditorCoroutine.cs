//#define EditorCoroutineDebugging

using System;
using System.Collections;
using System.Collections.Generic;
using Extenity.ReflectionToolbox;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if EditorCoroutineDebugging
using System.Linq;
using System.Reflection;
#endif

namespace Extenity.ParallelToolbox.Editor
{
	/// <summary>
	/// A handle to an EditorCoroutine, can be passed to <see cref="EditorCoroutineUtility">EditorCoroutineUtility</see> methods to control lifetime.
	/// </summary>
	public class EditorCoroutine
	{
		private struct YieldProcessor
		{
			internal enum DataType : byte
			{
				None = 0,
				WaitForSeconds = 1,
				EditorCoroutine = 2,
				AsyncOP = 3,
			}
			internal struct ProcessorData
			{
				public DataType type;
				public double targetTime;
				public object current;
			}

			internal ProcessorData data;

			public void Set(object yield)
			{
				if (yield == data.current)
					return;

				var type = yield.GetType();
				var dataType = DataType.None;
				double targetTime = -1;
				if (type == typeof(WaitForSeconds))
				{
					targetTime = EditorApplication.timeSinceStartup + (float)ReflectionTools.GetFieldValue(((WaitForSeconds)yield), "m_Seconds");
					dataType = DataType.WaitForSeconds;
				}
				// else if (type == typeof(WaitForSecondsRealtime)) Tried to support WaitForSecondsRealtime but there was another error with it. Activate EditorCoroutineDebugging and write tests to see what's going on.
				// {
				// 	targetTime = EditorApplication.timeSinceStartup + ((WaitForSecondsRealtime)yield).waitTime;
				// 	dataType = DataType.WaitForSeconds;
				// }
				// else if (type == typeof(EditorWaitForSeconds)) Not needed anymore. Use WaitForSeconds instead.
				// {
				// 	targetTime = EditorApplication.timeSinceStartup + (yield as EditorWaitForSeconds).WaitTime;
				// 	dataType = DataType.WaitForSeconds;
				// }
				else if (type == typeof(EditorCoroutine))
				{
					dataType = DataType.EditorCoroutine;
				}
				else if (type == typeof(AsyncOperation))
				{
					dataType = DataType.AsyncOP;
				}

				data = new ProcessorData { current = yield, targetTime = targetTime, type = dataType };
			}

#if EditorCoroutineDebugging
			public bool MoveNext(EditorCoroutine editorCoroutine, IEnumerator enumerator)
#else
			public bool MoveNext(IEnumerator enumerator)
#endif
			{
				bool advance = false;
				switch (data.type)
				{
					case DataType.WaitForSeconds:
						advance = data.targetTime <= EditorApplication.timeSinceStartup;
						break;
					case DataType.EditorCoroutine:
						advance = (data.current as EditorCoroutine).m_IsDone;
						break;
					case DataType.AsyncOP:
						advance = (data.current as AsyncOperation).isDone;
						break;
					default:
						advance = data.current == enumerator.Current; //a IEnumerator or a plain object was passed to the implementation
						break;
				}

				if (advance)
				{
					data = default(ProcessorData);
#if EditorCoroutineDebugging
					LogStatus("DECIDED TO ADVANCE", editorCoroutine);
					var more = enumerator.MoveNext();
					LogStatus($"AFTER THE ADVANCE ({(more ? "More" : "No more")})", editorCoroutine);
					return more;
#else
					return enumerator.MoveNext();
#endif
				}

#if EditorCoroutineDebugging
				LogStatus("DECIDED TO SKIP", editorCoroutine);
#endif
				return true;
			}
		}

		internal WeakReference m_Owner;
		IEnumerator m_Routine;
		YieldProcessor m_Processor;

		EditorCoroutine m_ParentCoroutine;
		static EditorCoroutine CurrentlyProcessingCoroutine;

		bool m_IsDone;

		Func<Exception, bool> m_OnException;
		/// <summary>
		/// Invokes the exception catcher if specified by user. Then the exception catcher should
		/// return 'true' if it wants to catch the exception and allow the coroutine execution
		/// to continue. Otherwise it returns 'false' which then the coroutine is no longer continued
		/// and the exception is handed one level above in the coroutine callstack. Then the same
		/// exception catching operation is applied on that coroutine.
		///
		/// If there is no exception catcher in this coroutine, this method returns 'false', which
		/// is the same as handing the exception up in callstack.
		/// </summary>
		private bool InvokeOnException(Exception exception)
		{
			try
			{
				if (m_OnException != null)
				{
					return m_OnException(exception);
				}
			}
			catch (Exception callbackException)
			{
				// The exception thrown in callback should not break the
				// execution. Otherwise we would miss the real exception.
				// So we log the callback exception as a separate line, then
				// move on to the serious one that happened in coroutine.
				Debug.LogException(callbackException);
			}
			return false;
		}

		internal EditorCoroutine(IEnumerator routine, Func<Exception, bool> onException)
		{
			m_Owner = null;
			m_Routine = routine;
			m_OnException = onException;
			if (CurrentlyProcessingCoroutine != null)
			{
				m_ParentCoroutine = CurrentlyProcessingCoroutine;
			}
			Register(this);
			LogStatus("CREATED OWNERLESS", this);
			RegisterToUpdate();
		}

		internal EditorCoroutine(IEnumerator routine, object owner, Func<Exception, bool> onException)
		{
			m_Processor = new YieldProcessor();
			m_Owner = new WeakReference(owner);
			m_Routine = routine;
			m_OnException = onException;
			if (CurrentlyProcessingCoroutine != null)
			{
				m_ParentCoroutine = CurrentlyProcessingCoroutine;
			}
			Register(this);
			LogStatus("CREATED WITH OWNER", this);
			RegisterToUpdate();
		}

		#region Update

		private void RegisterToUpdate()
		{
			if (m_IsDone)
				throw new Exception();
			EditorApplication.update += MoveNext;
		}

		private void DeregisterFromUpdate()
		{
			if (!m_IsDone)
				throw new Exception();
			EditorApplication.update -= MoveNext;
		}

		#endregion

		#region MoveNext Calls

		private static int MoveCalls = 0;
		private static bool IsInMove;

		private void MoveNext()
		{
			if (m_IsDone)
			{
				throw new Exception("Called MoveNext on a finalized coroutine.");
			}
			if (IsInMove)
			{
				throw new Exception("Recursively called MoveNext.");
			}

			MoveCalls++;
			IsInMove = true;

			try
			{
				if (m_Owner != null &&
				    (
					    !m_Owner.IsAlive ||
					    (m_Owner.Target is Object targetAsUnityObject && targetAsUnityObject == null)
				    )
				   )
				{
					LogVerbose("Finalizing coroutine because the owner was destroyed.");
					m_IsDone = true;
					DeregisterFromUpdate();
					return;
				}

				bool notDone = ProcessIEnumeratorRecursive(m_Routine);
				m_IsDone = !notDone;

				if (m_IsDone)
				{
					DeregisterFromUpdate();
				}
			}
			finally
			{
				IsInMove = false;
			}
		}

		#endregion

		static Stack<IEnumerator> kIEnumeratorProcessingStack = new Stack<IEnumerator>(32);
		private bool ProcessIEnumeratorRecursive(IEnumerator enumerator)
		{
			var root = enumerator;
			while (enumerator.Current as IEnumerator != null)
			{
				LogVerbose("Stack A");
				kIEnumeratorProcessingStack.Push(enumerator);
				enumerator = enumerator.Current as IEnumerator;
			}

			//process leaf
			m_Processor.Set(enumerator.Current);
			bool result;
			try
			{
				CurrentlyProcessingCoroutine = this;
				LogStatus("BEFORE", this);
#if EditorCoroutineDebugging
				result = m_Processor.MoveNext(this, enumerator);
#else
				result = m_Processor.MoveNext(enumerator);
#endif
			}
			catch (Exception exception)
			{
				LogStatus("AT EXCEPTION CATCH", this);

				//Debug.LogException(exception);

				var iteratedCoroutine = this;
				do
				{
					// Sadly the execution of this coroutine was broken by the exception. This coroutine won't continue
					// its execution. But its PARENT coroutine may continue if THIS coroutine handles the exception.
					iteratedCoroutine.m_IsDone = true;
					iteratedCoroutine.DeregisterFromUpdate();

					var exceptionHandled = iteratedCoroutine.InvokeOnException(exception);
					if (exceptionHandled)
					{
						// Yes, this coroutine handled the exception. Parent coroutine may continue from where its left off.
						return false;
					}
					if (iteratedCoroutine.m_ParentCoroutine != null)
					{
						// Nope, this coroutine did not handle the exception. So the parent coroutine should handle it, if
						// it wishes so. Execution of the parent coroutine also will be broken. But if it handles the exception,
						// the grandparent coroutine will continue the execution.
						iteratedCoroutine = iteratedCoroutine.m_ParentCoroutine;
					}
					else
					{
						// Nope, this coroutine did not handle the exception and there is no parent that would handle it.
						// We cannot do anything about that.
						iteratedCoroutine = null;
					}
				}
				while (iteratedCoroutine != null);

				// Nope, no coroutine did handle the exception. We can do nothing about that, but throw it off to Unity.
				throw;
			}
			finally
			{
				CurrentlyProcessingCoroutine = null;
			}

			while (kIEnumeratorProcessingStack.Count > 1)
			{
				LogVerbose("Stack B");
				if (!result)
				{
					result = kIEnumeratorProcessingStack.Pop().MoveNext();
				}
				else
					kIEnumeratorProcessingStack.Clear();
			}

			if (kIEnumeratorProcessingStack.Count > 0 && !result && root == kIEnumeratorProcessingStack.Pop())
			{
				LogVerbose("Stack C");
				result = root.MoveNext();
			}

			LogStatus($"AFTER ({(result ? "Not finished yet" : "Finished")})", this);

			return result;
		}

		internal void Stop()
		{
			m_Owner = null;
			m_Routine = null;
			m_IsDone = true;
			DeregisterFromUpdate();
		}

		#region All Coroutines

		/// <summary>
		/// The list of all existing EditorCoroutines.
		/// 
		/// Caution! Do not modify the contents.
		/// </summary>
		public static readonly List<WeakReference> AllEditorCoroutines = new List<WeakReference>();

		/// <summary>
		/// Caution! This will stop all ongoing EditorCoroutines and will probably cause
		/// serious damage to the systems depend on it. Use it at your own risk and do not
		/// ever use it if you are not sure what it does.
		/// </summary>
		public static void StopAllEditorCoroutines()
		{
			RegistryCleanup();
			foreach (var entry in AllEditorCoroutines)
			{
				if (entry.IsAlive)
				{
					var editorCoroutine = (EditorCoroutine)entry.Target;
					editorCoroutine.Stop();
				}
			}
		}

		public static void EnsureNoRunningEditorCoroutines()
		{
			RegistryCleanup();
			if (AllEditorCoroutines.Count > 0)
			{
				DumpAllEditorCoroutines();
				throw new Exception($"There are '{AllEditorCoroutines.Count}' running editor coroutines.");
			}
		}

		private static void Register(EditorCoroutine editorCoroutine)
		{
			RegistryCleanup();
			AllEditorCoroutines.Add(new WeakReference(editorCoroutine));
		}

		private static void RegistryCleanup()
		{
			for (int i = AllEditorCoroutines.Count - 1; i >= 0; i--)
			{
				if (AllEditorCoroutines[i].IsAlive)
				{
					var coroutine = (EditorCoroutine)AllEditorCoroutines[i].Target;
					if (!coroutine.m_IsDone)
						continue;
				}
				AllEditorCoroutines.RemoveAt(i);
			}
		}

		#endregion

		#region Debug

#if EditorCoroutineDebugging

		#region Reflection

		private void TryLogFields()
		{
			try
			{
				var fields = m_Routine.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy).Select(member => member.Name).ToList();
				Log.Info("Fields: " + string.Join(", ", fields));
			}
			catch { }
		}

		private int TryGetCurrentDepth()
		{
			try
			{
				return (int)m_Routine.GetType().GetField("currentDepth").GetValue(m_Routine);
			}
			catch { }
			return -2;
		}

		#endregion

		#region Log

		public static void LogVerbose(string message)
		{
			Log.Info(message);
		}

		public static void LogStatus(string operation, EditorCoroutine editorCoroutine)
		{
			var id = editorCoroutine?.ID.ToString() ?? "#";
			var depth = editorCoroutine?.TryGetCurrentDepth().ToString() ?? "#";
			var ownership = editorCoroutine?.m_ParentCoroutine != null ? "Child" : "Root";
			var inMoveMark = IsInMove ? "InMove" : "Paused";
			var doneMark = editorCoroutine?.m_IsDone == true ? "Done" : "NotDoneYet";
			Log.Info($"#\t\t ID {id}   Depth {depth}   Move {MoveCalls}   {ownership}   {inMoveMark}/{doneMark} \t\t {operation} \t\t {editorCoroutine?.m_Processor.data.type}");
		}

		public static void DumpAllEditorCoroutines()
		{
			foreach (var entry in AllEditorCoroutines)
			{
				if (entry.IsAlive)
				{
					var editorCoroutine = (EditorCoroutine)entry.Target;
					LogStatus("DUMP", editorCoroutine);
				}
			}
		}

		#endregion

#else

		[System.Diagnostics.Conditional("EditorCoroutineDebugging")]
		public static void DumpAllEditorCoroutines() { }

		[System.Diagnostics.Conditional("EditorCoroutineDebugging")]
		public static void LogVerbose(string message) { }

		[System.Diagnostics.Conditional("EditorCoroutineDebugging")]
		public static void LogStatus(string operation, EditorCoroutine editorCoroutine) { }

#endif

		#endregion
	}
}
