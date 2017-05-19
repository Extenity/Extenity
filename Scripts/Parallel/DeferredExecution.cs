using System;
using System.ComponentModel;

namespace Extenity.ParallelToolbox
{

	public static class DeferredExecution
	{

		public static DeferredExecutionController Setup(DoWorkEventHandler doWork)
		{
			var currentWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			var controller = new DeferredExecutionController(currentWorker);

			currentWorker.RunWorkerCompleted += (sender, args) =>
			{
				currentWorker.Dispose();
				if (args.Error != null)
				{
					controller.InformFailed(args.Error);
				}
				else if (args.Cancelled)
				{
					controller.InformCancelled();
				}
				else
				{
					controller.InformCompleted();
				}
			};

			currentWorker.ProgressChanged += (sender, args) => controller.InformUpdateProgress((string)args.UserState, args.ProgressPercentage / 100f);

			currentWorker.DoWork += doWork;

			return controller;
		}

	}

	public class DeferredExecutionController
	{
		private readonly BackgroundWorker BackgroundWorker;

		public DeferredExecutionController(BackgroundWorker backgroundWorker)
		{
			BackgroundWorker = backgroundWorker;
		}

		#region Status

		private bool _Finished;
		public bool Finished
		{
			get { lock (this) { return _Finished; } }
			private set { lock (this) { _Finished = value; } }
		}

		private bool _Cancelled;
		public bool Cancelled
		{
			get { lock (this) { return _Cancelled; } }
			private set { lock (this) { _Cancelled = value; } }
		}

		private bool _Successful;
		public bool Successful
		{
			get { lock (this) { return _Successful; } }
			private set { lock (this) { _Successful = value; } }
		}

		private Exception _Exception;
		public Exception Exception
		{
			get { lock (this) { return _Exception; } }
			private set { lock (this) { _Exception = value; } }
		}

		private string _Status;
		public string Status
		{
			get { lock (this) { return _Status; } }
			private set { lock (this) { _Status = value; } }
		}

		private float _Progress;
		public float Progress
		{
			get { lock (this) { return _Progress; } }
			private set { lock (this) { _Progress = value; } }
		}

		#endregion

		#region Events

		public event Action OnCompleted;

		public delegate void FailedDelegate(Exception exception);
		public event FailedDelegate OnFailed;

		public event Action OnCancelled;

		public delegate void ProgressDelegate(string status, float progress);
		public event ProgressDelegate OnProgress;

		#endregion

		#region Internal State Updates

		internal void InformUpdateProgress(string status, float progress)
		{
			lock (this)
			{
				Status = status;
				Progress = progress;
				if (OnProgress != null)
					OnProgress(status, progress);
			}
		}

		internal void InformCompleted()
		{
			lock (this)
			{
				Successful = true;
				Finished = true;
				Exception = null;
				if (OnCompleted != null)
					OnCompleted();
			}
		}

		internal void InformFailed(Exception exception)
		{
			lock (this)
			{
				Successful = false;
				Finished = true;
				Exception = exception;
				if (OnFailed != null)
					OnFailed(exception);
			}
		}

		internal void InformCancelled()
		{
			lock (this)
			{
				Successful = false;
				Finished = true;
				Exception = null;
				Cancelled = true;
				if (OnCancelled != null)
					OnCancelled();
			}
		}

		#endregion

		#region Commands

		public void RunAsync()
		{
			BackgroundWorker.RunWorkerAsync();
		}

		public void Cancel()
		{
			BackgroundWorker.CancelAsync();
		}

		#endregion
	}

}
