using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Extenity.ParallelToolbox
{

	/// <summary>
	/// CoroutineTask is an alternative interface for Unity's coroutine API with additional features.
	/// 
	/// * Stopping and pausing without any need for changes in coroutine code like lot's of "if"s.
	/// * Coroutine status can be checked outside of the coroutine
	/// * Callback when finished execution
	/// * Coroutines can be started without the need for a MonoBehaviour
	/// * Coroutines can be used inside Unity Editor. See 'EditorCoroutineTask' for details.
	/// * Coroutines can be used in threads. See 'EditorCoroutineTask' for details.
	/// 
	/// </summary>
	/// <example>
	/// Nested coroutines:
	/// 
	/// <code>
	/// CoroutineTask task;
	/// 
	/// void Start()
	/// {
	/// 	task = CoroutineTask.Create(MainCoroutine());
	/// 	task.OnFinished += TaskOnFinished;
	/// }
	/// 
	/// void Update()
	/// {
	/// 	if (Input.anyKeyDown)
	/// 	{
	/// 		Debug.Log("-------- Stop");
	/// 		task.Stop();
	/// 	}
	/// }
	/// 
	/// IEnumerator MainCoroutine()
	/// {
	/// 	Debug.Log("Main start  -  Waiting 5 seconds");
	/// 	yield return new WaitForSeconds(5f);
	/// 	yield return task.StartNested(SubCoroutine());
	/// 	Debug.Log("Main end");
	/// }
	/// 
	/// IEnumerator SubCoroutine()
	/// {
	/// 	Debug.Log("Sub start  -  Waiting 5 seconds");
	/// 	yield return new WaitForSeconds(5f);
	/// 	Debug.Log("Sub end");
	/// }
	/// 
	/// private void TaskOnFinished(bool manuallyStopped)
	/// {
	/// 	Debug.Log("Task finished. Manually stopped: " + manuallyStopped);
	/// 	task = null;
	/// }
	/// </code>
	/// </example>
	public class CoroutineTask
	{
		#region Initialization

		private CoroutineTask()
		{
		}

		/// <summary>
		/// Creates an uninitialized task. Initialize() should be called before any operations.
		/// Alternatively other constructors can be used to initialize automatically.
		/// </summary>
		public static CoroutineTask Create()
		{
			return new CoroutineTask();
		}

		public static CoroutineTask Create(IEnumerator coroutine, bool startImmediately = true)
		{
			var task = new CoroutineTask();
			task.Initialize(coroutine, startImmediately);
			return task;
		}

		public static CoroutineTask Create(IEnumerator coroutine, Finished onFinished, bool startImmediately = true)
		{
			var task = new CoroutineTask();
			task.Initialize(coroutine, onFinished, startImmediately);
			return task;
		}

		public void Initialize(IEnumerator coroutine, bool startImmediately = true)
		{
			BaseCoroutine = coroutine;
			if (startImmediately)
				Start();
		}

		public void Initialize(IEnumerator coroutine, Finished onFinished, bool startImmediately = true)
		{
			OnFinished += onFinished;
			Initialize(coroutine, startImmediately);
		}

		#endregion

		#region Events

		public delegate void Finished(bool manuallyStopped);
		public event Finished OnFinished;

		#endregion

		#region State

		/// <summary>
		/// Tells if the task was initialized and ready to be started.
		/// </summary>
		public bool IsInitialized { get { return BaseCoroutine != null; } }

		/// <summary>
		/// Tells if this task was started or waiting for it's start. Once a task starts, 
		/// IsLaunched will be set to true and will never be set to false again.
		/// </summary>
		public bool IsLaunched { get; private set; }

		/// <summary>
		/// Tells if the task started to run and not finished yet. This will be set to false
		/// once the task finishes or manually stopped with Stop().
		/// </summary>
		public bool IsRunning { get; private set; }

		/// <summary>
		/// Tells if the task currently paused. Task pause state can be changed anytime
		/// regardless of the launch and running states.
		/// </summary>
		public bool IsPaused { get; private set; }

		/// <summary>
		/// Tells if the task was manually stopped. This will be set when Stop() called
		/// and will never be set to false again.
		/// </summary>
		public bool IsManuallyStopped { get; private set; }

		#endregion

		#region Start / Stop / Pause / Resume

		//public Coroutine Coroutine;

		/// <summary>
		/// Starts execution of task. Most of the time you will want to use 
		/// 'startImmediately' parameter in constructor to start tasks. Though it's
		/// possible to start the task manually anytime.
		/// 
		/// Start() should not be called more than once. Otherwise it will throw an error.
		/// </summary>
		public Coroutine Start()
		{
			_InternalStart();
			//Coroutine = CoroutineTaskManager.Instance.StartCoroutine(CoroutineEmulator());
			return CoroutineTaskManager.Instance.StartCoroutine(CoroutineEmulator());
		}

		public void _InternalStart()
		{
			if (!IsInitialized)
				throw new Exception("Task was not initialized.");
			if (IsLaunched)
				throw new Exception("Task was already launched.");

			IsLaunched = true;
			IsRunning = true;
		}

		/// <summary>
		/// Stops execution of the task at next coroutine yield. Sets IsManuallyStopped to true.
		/// Use IsManuallyStopped to check if task was finished via Stop() or use OnFinished
		/// event which will tell if manually stopped via it's 'manuallyStopped' parameter.
		/// 
		/// Stop() can be called more than once. Multiple calls will be discarded silently.
		/// </summary>
		public void Stop()
		{
			IsManuallyStopped = true; // Set manually stopped flag regardless of running state

			if (!IsRunning)
				return;
			IsRunning = false;

			//// Call OnFinished immediately if user stops manually. Otherwise there will be a delay 
			//// while waiting for the last iterator before ending wrapper loop.
			//var callback = OnFinished;
			//if (callback != null)
			//	callback(IsManuallyStopped);
		}

		/// <summary>
		/// Pauses the execution of the task at next coroutine yield. Task can be paused anytime
		/// regardless of launch and running states.
		/// </summary>
		public void Pause()
		{
			IsPaused = true;
		}

		/// <summary>
		/// Resumes the execution of the task. See Pause() for more info.
		/// </summary>
		public void Resume()
		{
			IsPaused = false;
		}

		#endregion

		#region Nested Coroutines

		/// <summary>
		/// Makes it possible to use stop and pause features work with nested coroutines. 
		/// See class description for an example.
		/// </summary>
		public IEnumerator StartNested(IEnumerator coroutine)
		{
			CoroutineStack.Push(coroutine);
			return _DummyEnumerator();
		}

		private IEnumerator _DummyEnumerator()
		{
			yield break;
		}

		#endregion

		#region Coroutine Emulator

		private IEnumerator BaseCoroutine;
		private Stack<IEnumerator> CoroutineStack;

		public IEnumerator CoroutineEmulator()
		{
			if (CoroutineStack == null)
			{
				CoroutineStack = new Stack<IEnumerator>(5);
			}
			else
			{
				CoroutineStack.Clear();
			}
			CoroutineStack.Push(BaseCoroutine);

			while (IsRunning)
			{
				if (IsPaused)
				{
					yield return null;
				}
				else
				{
					var enumerator = CoroutineStack.Peek();

					//Debug.Log("                                           movenext " + enumerator + "        list count: " + CoroutineStack.Count);
					if (enumerator != null && enumerator.MoveNext())
					{
						//Debug.Log("                                           yes");
						yield return enumerator.Current;
					}
					else
					{
						//Debug.Log("                                           no");
						CoroutineStack.Pop();
						if (CoroutineStack.Count == 0)
						{
							IsRunning = false;
						}
					}
				}
			}

			//if (!IsManuallyStopped) // OnFinished will be handled differently if user stops the task manually. See Stop() for more info.
			{
				var callback = OnFinished;
				if (callback != null)
					callback(IsManuallyStopped);
			}
		}

		#endregion
	}

	internal class CoroutineTaskManager : MonoBehaviour
	{
		#region Singleton

		private static CoroutineTaskManager _Instance;
		internal static CoroutineTaskManager Instance
		{
			get
			{
				if (!_Instance)
				{
					var go = new GameObject("_CoroutineTaskManager");
					go.hideFlags = HideFlags.HideAndDontSave;
					DontDestroyOnLoad(go);
					_Instance = go.AddComponent<CoroutineTaskManager>();
				}
				return _Instance;
			}
		}

		#endregion
	}

}
