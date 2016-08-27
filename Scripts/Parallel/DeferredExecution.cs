using System;
using System.ComponentModel;

namespace Extenity.Parallel
{

	public static class DeferredExecution
	{

		public static DeferredExecutionState ExecuteInBackground(DoWorkEventHandler doWork)
		{
			var currentWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
			var result = new DeferredExecutionState(currentWorker);

			currentWorker.RunWorkerCompleted += (sender, args) =>
			{
				currentWorker.Dispose();
				if (args.Error != null)
				{
					result.FinishedWithError(args.Error.Message);
				}
				else
				{
					result.FinishedSuccessfully();
				}
			};

			currentWorker.ProgressChanged += (sender, args) => result.UpdateProgress((string)args.UserState, args.ProgressPercentage / 100f);

			currentWorker.DoWork += doWork;
			currentWorker.RunWorkerAsync();

			return result;
		}

	}

	public class DeferredExecutionState
	{
		private readonly BackgroundWorker BackgroundWorker;

		public DeferredExecutionState(BackgroundWorker backgroundWorker)
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

		private bool _Successful;
		public bool Successful
		{
			get { lock (this) { return _Successful; } }
			private set { lock (this) { _Successful = value; } }
		}

		private string _ErrorMessage;
		public string ErrorMessage
		{
			get { lock (this) { return _ErrorMessage; } }
			private set { lock (this) { _ErrorMessage = value; } }
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

		//public class FailedEvent : UnityEvent<string> { }
		//public FailedEvent OnFailed = new FailedEvent();
		public delegate void FailedDelegate(string errorMessage);
		public event FailedDelegate OnFailed;

		//public class ProgressEvent : UnityEvent<string, float> { }
		//public ProgressEvent OnProgress = new ProgressEvent();
		public delegate void ProgressDelegate(string status, float progress);
		public event ProgressDelegate OnProgress;

		#endregion

		#region Internal State Updates

		internal void UpdateProgress(string status, float progress)
		{
			lock (this)
			{
				Status = status;
				Progress = progress;
				if (OnProgress != null)
					OnProgress(status, progress);
			}
		}

		internal void FinishedSuccessfully()
		{
			lock (this)
			{
				Successful = true;
				Finished = true;
				ErrorMessage = null;
				if (OnCompleted != null)
					OnCompleted();
			}
		}

		internal void FinishedWithError(string error)
		{
			lock (this)
			{
				ErrorMessage = error;
				Successful = false;
				Finished = true;
				if (OnFailed != null)
					OnFailed(error);
			}
		}

		#endregion

		#region Commands

		public void Cancel()
		{
			BackgroundWorker.CancelAsync();
		}

		#endregion
	}

}
