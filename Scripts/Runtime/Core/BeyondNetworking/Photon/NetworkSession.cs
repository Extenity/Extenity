#if BeyondNetworkingUsesPhoton

using Extenity.DebugToolbox;
using UnityEngine.Events;

namespace BeyondNetworking
{

	public abstract class NetworkSession
	{
		#region ID

		private static int _LastGeneratedID = 0;

		public readonly int ID = ++_LastGeneratedID;

		#endregion

		#region Process Controller

		/// <summary>
		/// Set at the start of desired mode process and lives until the desired mode process is completed.
		/// </summary>
		public ProcessController Controller;

		#endregion

		#region Initialization Tracker

		private InitializationTracker _InitializationTracker;
		public InitializationTracker InitializationTracker
		{
			get { return _InitializationTracker; }
			set
			{
				_InitializationTracker = value;
				SetToCancelInitializationTrackerOnDisconnect();
			}
		}

		private void SetToCancelInitializationTrackerOnDisconnect()
		{
			if (InitializationTracker != null)
			{
				OnDisconnected.AddListener(InitializationTracker.CancelIfNotFinalized);
			}
		}

		#endregion

		#region Events

		public class NetworkStateEvent : UnityEvent<NetworkState> { }
		public readonly NetworkStateEvent OnNetworkStateChanged = new NetworkStateEvent();

		public readonly UnityEvent OnDisconnected = new UnityEvent();

		public class FinishEvent : UnityEvent<bool> { }
		public readonly FinishEvent OnFinished = new FinishEvent();

		#endregion
	}

}

#endif
