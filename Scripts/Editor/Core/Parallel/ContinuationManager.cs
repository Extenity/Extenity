using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;

namespace Extenity.ParallelToolbox.Editor
{

	/// <summary>
	/// Provides a simulated multithreading manager for invoking callbacks on the main Unity thread.
	/// </summary>
	public static class ContinuationManager
	{
		/// <summary>
		/// Provides a class for storing callback information.
		/// </summary>
		private class Job
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="Job"/> class.
			/// </summary>
			/// <param name="checkCompleted">The check completed callback function.</param>
			/// <param name="completionAction">The completion callback.</param>
			public Job(Func<bool> checkCompleted, Action completionAction)
			{
				this.CheckCompleted = checkCompleted;
				this.CompletionAction = completionAction;
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="Job"/> class.
			/// </summary>
			/// <param name="completionAction">The completion callback.</param>
			public Job(Action completionAction)
			{
				this.CompletionAction = completionAction;
			}

			/// <summary>
			/// Gets the check completed callback used to determine if the job has completed.
			/// </summary>
			public Func<bool> CheckCompleted { get; private set; }

			/// <summary>
			/// Gets the completion callback.
			/// </summary>
			public Action CompletionAction { get; private set; }
		}

		/// <summary>
		/// The jobs list the holds the queued jobs to be later invoked.
		/// </summary>
		private static readonly List<Job> jobQueue = new List<Job>();

		/// <summary>
		/// Gets or sets a value indicating whether <see cref="Update"/> is called automatically.
		/// </summary>                              
		public static bool AutoUpdate
		{
			get
			{
				return autoUpdate;
			}

			set
			{
				autoUpdate = value;
				lock (jobQueue)
				{
					if (autoUpdate && !updateHooked)
					{
						EditorApplication.update += Update;
						updateHooked = true;
					}

					if (!autoUpdate && updateHooked)
					{
						EditorApplication.update -= Update;
						updateHooked = false;
					}
				}
			}
		}

		/// <summary>
		/// The update hooked flag used to determine weather or not the editor update method has been hooked into.
		/// </summary>
		private static bool updateHooked;

		/// <summary>
		/// Queues a callback that will be run on the next update if the completed callback returns true.
		/// </summary>
		/// <param name="completed">The CheckCompleted.</param>
		/// <param name="continueWith">The continue with.</param>
		public static void Run(Func<bool> completed, Action continueWith)
		{
			// lock the jobs list
			lock (jobQueue)
			{
				// queue the job on the jobs list
				jobQueue.Add(new Job(completed, continueWith));
			}
		}

		/// <summary>
		/// Queues a callback that will be run on the next call to <see cref="Update"/>.
		/// </summary>
		/// <param name="continueWith">The continue with.</param>
		public static void Run(Action continueWith)
		{
			// lock the jobs list
			lock (jobQueue)
			{
				// queue the job on the jobs list
				jobQueue.Add(new Job(null, continueWith));
			}
		}

		/// <summary>
		/// Invoke all queued jobs if they have completed.
		/// </summary>
		public static void Update()
		{
			// check if we have been initialized by checking if the main thread has been set
			if (MainThread == null)
			{
				throw new ThreadStateException("Not Initialized!");
			}

			// if we are not on the main thread throw an exception otherwise any code running in the callbacks
			// might access unity api's that are not thread safe and cause an exception. So we throw a 
			// thread exception before atempting to invoke any callbacks.
			if (Thread.CurrentThread != MainThread)
			{
				throw new ThreadStateException("Can only be called from the main unity thread.");
			}

			// locking the jobs queue here means that any other calls to Update, Run or RunSynchronously will
			// be blocked until all jobs have been processed.
			lock (jobQueue)
			{
				// process each job in the queue
				var i = 0;
				while (i < jobQueue.Count)
				{
					var job = jobQueue[i];
					if (job.CheckCompleted == null || job.CheckCompleted())
					{
						job.CompletionAction();
						jobQueue.RemoveAt(i);
						continue;
					}

					i++;
				}
			}
		}

		/// <summary>
		/// A reference to the main unity thread.
		/// </summary>
		private static Thread MainThread;

		/// <summary>
		/// The automatic update flag used by the <see cref="AutoUpdate"/> property.
		/// </summary>
		private static bool autoUpdate;

		/// <summary>
		/// Called bu unity automatically and sets auto update to true.
		/// </summary>
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			if (MainThread != null)
			{
				return;
			}

			MainThread = Thread.CurrentThread;
			AutoUpdate = true;
		}

		/// <summary>
		/// Runs a callback synchronously.
		/// </summary>
		/// <param name="callback">The callback to be invoked.</param>
		/// <returns></returns>
		/// <exception cref="ThreadStateException">Thrown if the <see cref="Initialize"/> method has not been previously called.</exception>
		public static void RunSynchronously(Action callback)
		{
			RunSynchronously(new Func<object>(() =>
			{
				callback();
				return null;
			}));
		}

		/// <summary>
		/// Runs a callback synchronously.
		/// </summary>
		/// <typeparam name="T">The return type.</typeparam>
		/// <param name="callback">The callback to be invoked.</param>
		/// <returns></returns>
		/// <exception cref="ThreadStateException">Thrown if the <see cref="Initialize"/> method has not been previously called.</exception>
		public static T RunSynchronously<T>(Func<T> callback)
		{
			// setup a default return value
			T result = default(T);

			// check if we have been initialized by checking if the main thread has been set
			if (MainThread == null)
			{
				throw new ThreadStateException("Not Initialized!");
			}

			// if we are already on the main thread then just invoke directly
			if (Thread.CurrentThread == MainThread)
			{
				return callback();
			}

			// otherwise queue up a job
			var completed = false;
			Run(() => !completed, () =>
			{
				result = callback();
				completed = true;
			});

			// wait for job to complete by putting the thread to sleep
			// based on the previous check this code should only get executed if
			// called from a thread other then the main unity thread
			while (!completed)
			{
				Thread.Sleep(1);
			}

			// return result
			return result;
		}
	}

}
