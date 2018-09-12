using UnityEngine;

namespace PhotonTools
{

	public class ProcessController
	{
		/// <summary>
		/// States that the process is cancelled. It may be cancelled by user or by another
		/// launched process, since both processes can't be alive at the same time.
		/// </summary>
		public bool IsCancelled { get; private set; }
		/// <summary>
		/// States that the process is finished any no work will be done anymore, without
		/// stating if the process is successful or not.
		/// </summary>
		public bool IsFinished { get; private set; }
		/// <summary>
		/// States that the process is failed. The process may still continue after failure.
		/// Use 'IsFinished' to see if it's still alive.
		/// </summary>
		public bool IsFailed { get; private set; }

		public readonly ConnectivityMode Mode;

		public ProcessController(ConnectivityMode mode)
		{
			Mode = mode;
		}

		public void Cancel()
		{
			if (IsCancelled)
			{
				Debug.LogWarning($"Tried to cancel process '{Mode}' more than once.");
				return;
			}
			IsCancelled = true;
		}

		internal void InformFinish()
		{
			if (IsFinished)
			{
				Debug.LogWarning($"Tried to finish process '{Mode}' more than once.");
				return;
			}
			IsFinished = true;
		}

		internal void InformFail()
		{
			IsFailed = true;
		}
	}

}
