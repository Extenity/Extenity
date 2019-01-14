#define EnableNetworkDebugInput

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Extenity.DataToolbox;
using Extenity.DebugFlowTool.GraphPlotting;
using Extenity.FlowToolbox;
using Extenity.MessagingToolbox;
using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace BeyondNetworking
{

	/// <summary>
	/// Handles application's network connection from a higher level perspective. Users would only
	/// specify which network mode they would like to be in, like SinglePlayerSession, OnlineMenu, etc.
	/// Then the manager would automatically prepare the network library and connection behind the
	/// curtains.
	/// 
	/// One side note:
	/// NetworkConnectivityManager won't depend on network library's callbacks when processing
	/// desired modes. It will always poll the network state when deciding to process to next step.
	/// </summary>
	public class NetworkConnectivityManager : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks , IInRoomCallbacks, ILobbyCallbacks
	{
		#region Singleton

		private static NetworkConnectivityManager _Instance;
		private static NetworkConnectivityManager Instance
		{
			get
			{
				if (_Instance == null)
				{
					throw new Exception($"{nameof(NetworkConnectivityManager)} was not instantiated.");
				}
				return _Instance;
			}
		}

		private static bool IsInstanceAvailable { get { return _Instance; } }
		private static bool _WasInstantiatedBefore;

		#endregion

		#region Initialization

		protected void Awake()
		{
			if (_Instance)
				throw new InternalException(922817);
			_Instance = this;
			UpdateLogPrefix(null);
			if (_WasInstantiatedBefore)
			{
				// Instantiating NetworkConnectivityManager the second time will probably cause errors.
				// Because there are static fields in this class which should be reset properly.
				// Also when destroyed, the underlying networking system should be put in a proper state.
				// If you absolutely need to destroy it and create it again, then make sure it works
				// flawlessly before proceeding any further. Then remove this error log.
				Log.InternalError(1784111, this);
			}
			_WasInstantiatedBefore = true;

			// TODO: Temporary solution for a Photon bug. CollectibleSpawnInfo serialization in RpcSpawnCollectibles method throws an exception when using GpBinaryV18.
			PhotonNetwork.NetworkingClient.LoadBalancingPeer.SerializationProtocolType = SerializationProtocol.GpBinaryV16;

			PhotonNetwork.UseRpcMonoBehaviourCache = true;

			if (EnableNetworkStatisticsAtStart)
				NetworkStatisticsEnabled = true;
			if (EnableNetworkStatisticsGraphPlottingAtStart)
				NetworkStatisticsPlottingEnabled = true;

			//BackendAuthentication.OnAuthenticationCompleted.AddListener(OnAuthenticated);
			//if (BackendAuthentication.IsAuthenticated)
			//	OnAuthenticated();
		}

		protected void OnEnable()
		{
			PhotonNetwork.AddCallbackTarget(this);
		}

		//private void OnAuthenticated()
		//{
		//	BackendAuthentication.OnAuthenticationCompleted.RemoveListener(OnAuthenticated);

		//	if (DesiredMode == ConnectivityMode.Unspecified)
		//	{
		//		SetDesiredModeToOffline();
		//	}
		//}

		#endregion

		#region Deinitialization

		private static bool IsQuitting;

		//public override void OnDisable()
		//{
		//	base.OnDisable();
		//}

		protected void OnApplicationQuit()
		{
			IsQuitting = true;
		}

		protected void OnDestroy()
		{
			if (_Instance == this)
			{
				_Instance = null;
			}

			NetworkState = NetworkState.Unspecified;
			_DesiredMode = ConnectivityMode.Unspecified;
		}

		protected void OnDisable()
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		#endregion

		#region Update

#if EnableNetworkDebugInput

		private int CommandRepeats = 1;

		protected void Update()
		{
			// Make sure these lines will only be run if user presses a key while keeping Shift key down.
			if (!Input.anyKeyDown || !Extenity.InputToolbox.InputTools.GetKey_OnlyShift)
				return;

			var session = CurrentSession;

			for (int i = (int)KeyCode.Alpha1; i <= (int)KeyCode.Alpha9; i++)
			{
				if (Input.GetKeyDown((KeyCode)i))
				{
					var count = i - (int)KeyCode.Alpha0;
					Log.Info($"<b>DEBUG | Setting command repeats to '{count}'</b>", this);
					CommandRepeats = count;
				}
			}

			// Switch DesiredMode - Menu related
			if (Input.GetKeyDown(KeyCode.F1))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToOfflineMenu</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToOfflineMenu(new DefaultNetworkSession());
			}
			if (Input.GetKeyDown(KeyCode.F2))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToOnlineMenu</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToOnlineMenu(new DefaultNetworkSession());
			}
			if (Input.GetKeyDown(KeyCode.F3))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToGameFinder with default lobby</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToGameFinder(new DefaultNetworkSession(), new GameFinderConfiguration { Lobby = TypedLobby.Default });
			}
			if (Input.GetKeyDown(KeyCode.F4))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToGameFinder with custom lobby</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToGameFinder(new DefaultNetworkSession(), new GameFinderConfiguration { Lobby = new TypedLobby("TestLobby", LobbyType.Default) });
			}

			// Switch DesiredMode - Game related
			if (Input.GetKeyDown(KeyCode.F5))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToOfflineSession</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToOfflineSession(new DefaultNetworkSession());
			}
			if (Input.GetKeyDown(KeyCode.F6))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToSinglePlayerSession</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToSinglePlayerSession(new DefaultNetworkSession(), new SinglePlayerConfiguration { Lobby = TypedLobby.Default });
			}
			if (Input.GetKeyDown(KeyCode.F7))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToHostSession</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToHostSession(new DefaultNetworkSession(), new HostConfiguration { RoomName = "TestRoom" });
			}
			if (Input.GetKeyDown(KeyCode.F8))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToJoinSession</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToJoinSession(new DefaultNetworkSession(), new JoinConfiguration { RoomName = "TestRoom" });
			}
			if (Input.GetKeyDown(KeyCode.F9))
			{
				Log.Info("<b>DEBUG | Calling SetDesiredModeToRandomJoinSession</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					SetDesiredModeToRandomJoinSession(new DefaultNetworkSession(), new RandomJoinConfiguration { MatchingType = MatchmakingMode.FillRoom });
			}

			//if (Input.GetKeyDown(KeyCode.F9))
			//{
			//	Log.Info("<b>DEBUG | Leaving lobby</b>", this);
			//	if (CurrentSession.Controller == null)
			//		CurrentSession.Controller = new ProcessController(ConnectivityMode.Unspecified);
			//	StartCoroutine(InternalLeaveLobby(CurrentSession));
			//}

			if (Input.GetKeyDown(KeyCode.F10))
			{
				Log.Info("<b>DEBUG | Testing pending process discard mechanism</b>", this);
				SetDesiredModeToJoinSession(new DefaultNetworkSession(), new JoinConfiguration { RoomName = "TestRoom" });
				SetDesiredModeToJoinSession(new DefaultNetworkSession(), new JoinConfiguration { RoomName = "TestRoom" });
				SetDesiredModeToHostSession(new DefaultNetworkSession(), new HostConfiguration { RoomName = "TestRoom" });
				SetDesiredModeToSinglePlayerSession(new DefaultNetworkSession(), new SinglePlayerConfiguration { Lobby = TypedLobby.Default });
				SetDesiredModeToOfflineSession(new DefaultNetworkSession());
			}

			if (Input.GetKeyDown(KeyCode.F11))
			{
				Log.Info("<b>DEBUG | Restarting desired mode</b>", this);
				for (int i = 0; i < CommandRepeats; i++)
					RestartDesiredMode(new DefaultNetworkSession());
			}

			// Debug information
			if (Input.GetKeyDown(KeyCode.F12))
			{
				DebugLogNetworkState(session);
			}
		}

#endif

		#endregion

		#region Firewall Check

		// TODO:
		//Networking.InitializeFirewallCheck()

		#endregion

		#region Master Client

		public class MasterClientEvent : UnityEvent<bool> { }
		public static readonly MasterClientEvent OnMasterClientChanged = new MasterClientEvent();
		public static readonly ExtenityEvent OnBecameMasterClient = new ExtenityEvent();

		// TODO: 
		//private void CheckConsistencyOnDesiredModeSwitch_MasterClient()
		//{
		//	OnMasterClientChanged.EnsureNoCallbacksRegistered(nameof(OnMasterClientChanged));
		//	OnBecameMasterClient.EnsureNoCallbacksRegistered(nameof(OnBecameMasterClient));
		//}

		public static void RegisterToBecomingMasterClientIfRequired(ExtenityEvent.MethodDefinition action)
		{
			if (!PhotonNetwork.IsMasterClient)
			{
				OnBecameMasterClient.AddListener(action);
			}
		}

		public static void CallOrRegisterToBecomingMasterClient(ExtenityEvent.MethodDefinition action, ExtenityEvent.MethodDefinition immediateActionIfNotMaster = null)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				action();
			}
			else
			{
				immediateActionIfNotMaster?.Invoke();
				OnBecameMasterClient.AddListener(action);
			}
		}

		public void OnMasterClientSwitched(Player newMasterClient)
		{
			//var session = CurrentSession;

			var isMasterClient = PhotonNetwork.IsMasterClient;
			Log.Info($"Master Client switched to '{(isMasterClient ? "local" : "remote")}' player: '{newMasterClient}'", this);
			OnMasterClientChanged.Invoke(isMasterClient);

			if (isMasterClient)
			{
				OnBecameMasterClient.InvokeOneShotSafe();
			}
		}

		#endregion

		#region Status

		//[Inspect, ReadOnly]
		public static NetworkState NetworkState { get; private set; }

		public class NetworkStateEvent : UnityEvent<NetworkState> { }
		public static readonly NetworkStateEvent OnNetworkStateChanged = new NetworkStateEvent();

		private bool _RoomCreationFailedFlag => _RoomCreationFailedMessage != null;
		private string _RoomCreationFailedMessage = null;

		private bool _RoomJoiningFailedFlag => _RoomJoiningFailedMessage != null;
		private string _RoomJoiningFailedMessage = null;

		private void RefreshNetworkState(NetworkSession session, params NetworkState[] expectedStates)
		{
			if (OverkillLogging)
				DebugLogNetworkState(session);

			// Decide which state are we in.
			var oldState = NetworkState;
			var newState = GrabStateOfPhoton();

			if (expectedStates.Length != 0)
			{
				if (!expectedStates.Contains(newState))
				{
					Log.Warning($"New network state '{newState}' is different from the expected state(s) '{string.Join(", ", expectedStates.Select(item => item.ToString()))}'.", this);
				}
			}

			if (newState != oldState)
			{
				if (VerboseLogging)
					Log.Info($"Network state changed to '{newState}' (Previously was '{oldState}')", this);
				NetworkState = newState;

				OnNetworkStateChanged.Invoke(newState);
				session.OnNetworkStateChanged.Invoke(newState);
				if (newState == NetworkState.NotConnected)
				{
					session.OnDisconnected.Invoke();
				}
			}
		}

		private static NetworkState GrabStateOfPhoton()
		{
			var photonState = PhotonNetwork.NetworkClientState;
			switch (photonState)
			{
				// Cloud
				case ClientState.ConnectingToMasterserver: return NetworkState.ConnectingToCloud;
				case ClientState.ConnectingToNameServer: return NetworkState.ConnectingToCloud;
				case ClientState.ConnectedToNameServer: return NetworkState.ConnectingToCloud;
				case ClientState.Authenticating: return NetworkState.ConnectingToCloud;
				case ClientState.DisconnectingFromMasterserver: return NetworkState.DisconnectingFromCloud;
				case ClientState.DisconnectingFromNameServer: return NetworkState.DisconnectingFromCloud;
				case ClientState.Disconnecting: return NetworkState.DisconnectingFromCloud;
				case ClientState.PeerCreated: return NetworkState.NotConnected;
				case ClientState.Disconnected: return NetworkState.NotConnected;
				case ClientState.Authenticated: return NetworkState.Cloud;
				case ClientState.ConnectedToMasterserver: return NetworkState.Cloud;

				// Lobby
				case ClientState.JoiningLobby: return NetworkState.JoiningToLobby;
				case ClientState.JoinedLobby: return NetworkState.Lobby;

				// Room
				case ClientState.ConnectingToGameserver: return NetworkState.JoiningToRoom;
				case ClientState.ConnectedToGameserver: return NetworkState.JoiningToRoom; // This is an intermediate state while still connecting to a game. See ClientState.ConnectedToGameserver documentation.
				case ClientState.Joining: return NetworkState.JoiningToRoom;
				case ClientState.Leaving: return NetworkState.LeavingRoom;
				case ClientState.DisconnectingFromGameserver: return NetworkState.LeavingRoom;
				case ClientState.Joined: return NetworkState.Room;

				default:
					throw new ArgumentOutOfRangeException("PhotonNetwork.networkingPeer.State", photonState, "");
			}
		}

		#endregion

		#region Process

		//[Inspect, ReadOnly]
		public static bool IsCurrentlyProcessing { get { return CurrentSession != null && CurrentSession.Controller != null; } }

		/// <summary>
		/// A process requires currently ongoing process to be cancelled first before getting started.
		/// While waiting for ongoing process to be finished, this process will be kept in a pending state.
		/// 'PendingSession' field keeps track of currently pending process, so that a secondly launched
		/// pending process may cancel both the ongoing process and the previously pending process.
		/// </summary>
		public static NetworkSession PendingSession { get; private set; }
		/// <summary>
		/// Set at the start of desired mode process and lives until the desired mode is terminated,
		/// i.e disconnecting from server or changing to another desired mode.
		/// See also 'ProcessController' which lives until the desired mode process is completed.
		/// </summary>
		public static NetworkSession CurrentSession { get; private set; }

		//[Inspect, ReadOnly]
		private List<string> AllProcessSteps = new List<string>(20);
		//[Inspect, ReadOnly]
		private List<string> CurrentProcessSteps = new List<string>(20);

		//[Inspect, ReadOnly]
		private bool ProcessFailed;
		//[Inspect, ReadOnly]
		//private string ProcessFailMessage;

		public class ProcessStepEvent : UnityEvent<NetworkProcessStep, string> { }
		public static readonly ProcessStepEvent OnProcessStepChanged = new ProcessStepEvent();

		private bool InternalStartProcess(NetworkSession session, string initialMessage)
		{
			// Reset fail state
			ClearFailedFlag();

			// Change current process
			Log.Info(initialMessage, this);

			// Reset process steps
			CurrentProcessSteps.Clear();
			SetProcessStep(session, NetworkProcessStep.ProcessStarted, initialMessage);
			return true;
		}

		private void InternalEndProcess(NetworkSession session)
		{
			Debug.Assert(CurrentSession == session);

			if (!IsCurrentlyProcessing)
			{
				Log.CriticalError("Tried to end process while there is no active one.", this);
			}
			else
			{
				SetProcessStep(session, NetworkProcessStep.ProcessCompleted);

				session.Controller.InformFinish();
				session.Controller = null;
				session.OnFinished.Invoke(!ProcessFailed);

				//CurrentSession = null; No! Do not! See description of the field.
			}
		}

		private void SetProcessStep(NetworkSession session, NetworkProcessStep step, string message = null)
		{
			if (!IsCurrentlyProcessing)
			{
				Log.Warning("Added a process step while not currently processing.", this);
			}

			var fullMessage = !string.IsNullOrEmpty(message)
				? step + " | " + message
				: step.ToString();

			CurrentProcessSteps.Add(fullMessage);
			AllProcessSteps.Add(fullMessage);

			fullMessage = !string.IsNullOrEmpty(message)
				? $"Step '{step}' | {message}"
				: $"Step '{step}'";

			switch (step)
			{
				case NetworkProcessStep.Unknown:
				case NetworkProcessStep.InternalError:
				case NetworkProcessStep.ProcessFailed:
				case NetworkProcessStep.AuthenticationFailedInformation:
				case NetworkProcessStep.FailedToJoinLobby:
				case NetworkProcessStep.FailedToLeaveLobby:
				case NetworkProcessStep.JoinRoomFailedInformation:
				case NetworkProcessStep.RandomJoinFailedInformation:
				case NetworkProcessStep.FailedToLeaveRoom:
					Log.Error(fullMessage, this);
					break;
				case NetworkProcessStep.ProcessStarted:
				case NetworkProcessStep.ProcessCompleted:
				case NetworkProcessStep.SettingOffline:
				case NetworkProcessStep.SettingOnline:
				case NetworkProcessStep.AuthenticationResponseInformation:
				case NetworkProcessStep.ConnectedToMasterInformation:
				case NetworkProcessStep.ConnectedToPhotonInformation:
				case NetworkProcessStep.ConnectingToCloud:
				case NetworkProcessStep.AlreadyConnectedToCloud:
				case NetworkProcessStep.ConnectedToCloud:
				case NetworkProcessStep.CloudConnectionFailed:
				case NetworkProcessStep.Disconnecting:
				case NetworkProcessStep.Disconnected:
				case NetworkProcessStep.DisconnectedInformation:
				case NetworkProcessStep.JoiningToLobby:
				case NetworkProcessStep.JoinedLobbyInformation:
				case NetworkProcessStep.AlreadyInsideLobby:
				case NetworkProcessStep.ChangingLobby:
				case NetworkProcessStep.JoinedToLobby:
				case NetworkProcessStep.LeavingLobby:
				case NetworkProcessStep.LeftLobbyInformation:
				case NetworkProcessStep.AlreadyNotInLobby:
				case NetworkProcessStep.LeftLobby:
				case NetworkProcessStep.CreatingOfflineRoom:
				case NetworkProcessStep.CreatingSinglePlayerRoom:
				case NetworkProcessStep.CreatingHostRoom:
				case NetworkProcessStep.RoomCreatedInformation:
				case NetworkProcessStep.CreateRoomFailedInformation:
				case NetworkProcessStep.JoiningRoom:
				case NetworkProcessStep.JoinedRoomInformation:
				case NetworkProcessStep.LeavingRoom:
				case NetworkProcessStep.LeftRoom:
				case NetworkProcessStep.AlreadyNotInRoom:
				case NetworkProcessStep.LeftRoomInformation:
					if (VerboseLogging)
						Log.Info(fullMessage, this);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(step), step, "");
			}

			OnProcessStepChanged.Invoke(step, message);
		}

		private void ClearFailedFlag()
		{
			ProcessFailed = false;
			//ProcessFailMessage = null;
		}

		#endregion

		#region Desired Mode

		private static ConnectivityMode _DesiredMode;
		public static ConnectivityMode DesiredMode => _DesiredMode;

		public class ModeEvent : UnityEvent { }
		public static readonly ModeEvent OnProcessingDesiredMode = new ModeEvent();

		private GameFinderConfiguration DesiredGameFinderConfiguration;
		private SinglePlayerConfiguration DesiredSinglePlayerConfiguration;
		private HostConfiguration DesiredHostConfiguration;
		private JoinConfiguration DesiredJoinConfiguration;
		private RandomJoinConfiguration DesiredRandomJoinConfiguration;

		public static Coroutine RestartDesiredMode(NetworkSession session)
		{
			return Instance.ProcessDesiredMode(session);
		}

		public static Coroutine SetDesiredModeToOfflineMenu(NetworkSession session)
		{
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.OfflineMenu, session);
		}

		public static Coroutine SetDesiredModeToOnlineMenu(NetworkSession session)
		{
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.OnlineMenu, session);
		}

		public static Coroutine SetDesiredModeToGameFinder(NetworkSession session, GameFinderConfiguration configuration)
		{
			Instance.DesiredGameFinderConfiguration = configuration;
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.GameFinder, session);
		}

		public static Coroutine SetDesiredModeToOfflineSession(NetworkSession session)
		{
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.OfflineSession, session);
		}

		public static Coroutine SetDesiredModeToSinglePlayerSession(NetworkSession session, SinglePlayerConfiguration configuration)
		{
			Instance.DesiredSinglePlayerConfiguration = configuration;
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.SinglePlayerSession, session);
		}

		public static Coroutine SetDesiredModeToHostSession(NetworkSession session, HostConfiguration configuration)
		{
			Instance.DesiredHostConfiguration = configuration;
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.HostSession, session);
		}

		public static Coroutine SetDesiredModeToJoinSession(NetworkSession session, JoinConfiguration configuration)
		{
			Instance.DesiredJoinConfiguration = configuration;
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.JoinSession, session);
		}

		public static Coroutine SetDesiredModeToRandomJoinSession(NetworkSession session, RandomJoinConfiguration configuration)
		{
			Instance.DesiredRandomJoinConfiguration = configuration;
			return Instance.SetAndProcessDesiredMode(ConnectivityMode.RandomJoinSession, session);
		}

		private Coroutine SetAndProcessDesiredMode(ConnectivityMode mode, NetworkSession session)
		{
			_DesiredMode = mode;
			return ProcessDesiredMode(session);
		}

		private Coroutine ProcessDesiredMode(NetworkSession session)
		{
			session.Controller = new ProcessController(DesiredMode);
			return StartCoroutine(InternalProcessDesiredMode(session));
		}

		private IEnumerator InternalProcessDesiredMode(NetworkSession session)
		{
			if (string.IsNullOrEmpty(_GameVersion))
				throw new InternalException(7581051); // SetVersion must be called before any operation.
			if (session == null)
				throw new ArgumentNullException(nameof(session));

			// Cancel the ongoing process immediately as this method called. This must be the first line.
			if (IsCurrentlyProcessing)
			{
				//Debug.Assert(CurrentSession != null); This may or may not be null. NetworkSession will live throughout the entire time the desired mode is in use, not just the processing phase at the beginning.

				// This is where concurrency magic happens. We immediately set PendingProcessController
				// as this controller so that the next call to change desired mode (whether it is a call
				// 5 seconds from now or an instant call) will cancel this pending process first. There
				// will never be more than one pending process at the same time.
				if (PendingSession != null && !PendingSession.Controller.IsCancelled)
				{
					// Cancel currently pending process. We will override the pending process and take
					// it's place. Currently pending process will be terminated before even started.
					// This can happen with more than two rapid desired mode changes.
					PendingSession.Controller.Cancel();
				}
				PendingSession = session;

				// Flag the process as cancelled, if not cancelled already. We know that there is an active
				// ProcessController because of the initial 'if (IsCurrentlyProcessing)' check above.
				if (!CurrentSession.Controller.IsCancelled)
				{
					CurrentSession.Controller.Cancel();
				}

				var currentlyActiveControllerToBeTerminated = CurrentSession.Controller; // Cache this in case CurrentSession field gets updated somewhere else.

				// Wait for the process to finish. At this time, we are the pending process.
				while (!currentlyActiveControllerToBeTerminated.IsFinished && !session.Controller.IsCancelled)
				{
					if (OverkillLogging)
						Log.Info("---------- Waiting for previous process to finish...", this);
					yield return null;
				}

				// Meanwhile our pending process may get cancelled even before finishing
				// the cancellation request of previously active process.
				if (session.Controller.IsCancelled)
				{
					Log.Info($"Discarded a pending desired mode '{session.Controller.Mode}'.", this);
					yield break;
				}

				// Time to start the pending process.
				if (PendingSession != session)
				{
					Log.CriticalError($"Pending session expected to be '{session.ID}' but it was '{PendingSession.ID}'.", this);
					yield break;
				}
				CurrentSession = session;
				UpdateLogPrefix(session);
				PendingSession = null;
			}
			else
			{
				// We immediately set CurrentProcessController here at the start of this method.
				// See the comments above for detailed explanation.
				CurrentSession = session;
				UpdateLogPrefix(session);
				Debug.Assert(PendingSession == null);
			}

			var controller = session.Controller;

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }

			try
			{
				OnProcessingDesiredMode.Invoke();
			}
			catch (Exception exception)
			{
				Log.Exception(exception);
				controller.InformFail();
				yield break;
			}

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }

			switch (DesiredMode)
			{
				case ConnectivityMode.Unspecified:
					Log.CriticalError("Tried to process desired network connectivity mode while it was unspecified.", this);
					yield break;
				case ConnectivityMode.OfflineMenu:
					yield return StartCoroutine(DoStartOfflineMenu(session));
					break;
				case ConnectivityMode.OnlineMenu:
					yield return StartCoroutine(DoStartOnlineMenu(session));
					break;
				case ConnectivityMode.GameFinder:
					yield return StartCoroutine(DoStartGameFinder(DesiredGameFinderConfiguration, session));
					break;
				case ConnectivityMode.OfflineSession:
					yield return StartCoroutine(DoStartOfflineSession(session));
					break;
				case ConnectivityMode.SinglePlayerSession:
					yield return StartCoroutine(DoStartSinglePlayerSession(DesiredSinglePlayerConfiguration, session));
					break;
				case ConnectivityMode.HostSession:
					yield return StartCoroutine(DoStartHostSession(DesiredHostConfiguration, session));
					break;
				case ConnectivityMode.JoinSession:
					yield return StartCoroutine(DoStartJoinSession(DesiredJoinConfiguration, session));
					break;
				case ConnectivityMode.RandomJoinSession:
					yield return StartCoroutine(DoStartRandomJoinSession(DesiredRandomJoinConfiguration, session));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#endregion

		#region Mode - Offline Menu

		private IEnumerator DoStartOfflineMenu(NetworkSession session)
		{
			var controller = session.Controller;

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, "Starting offline menu");

			// Disconnect if already connected
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalDisconnect(session));
			}

			// Switch to offline if required
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				if (!PhotonNetwork.OfflineMode)
				{
					SetProcessStep(session, NetworkProcessStep.SettingOffline);

					PhotonNetwork.OfflineMode = true;
				}
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.NotConnected);
				FailProcess(session, "Failed to get into offline menu.");
				// TODO: Quit application. There is nothing more to do if we can't even launch in offline mode.
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Cloud);
			}
			InternalEndProcess(session);
		}

		#endregion

		#region Mode - Online Menu

		private IEnumerator DoStartOnlineMenu(NetworkSession session)
		{
			var controller = session.Controller;

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, "Starting online menu");

			// Leave if already in a room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalLeaveRoom(session));
			}

			// Leave if already in a lobby
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalLeaveLobby(session));
			}

			// Connect to cloud if required
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalConnectToCloud(session));
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.NotConnected);
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Cloud);
			}
			InternalEndProcess(session);
		}

		#endregion

		#region Mode - Game Finder

		private IEnumerator DoStartGameFinder([NotNull]GameFinderConfiguration configuration, NetworkSession session)
		{
			var controller = session.Controller;

			var error = configuration.ValidateAndFix();
			if (error != null)
			{
				Log.CriticalError(error, this);
				SetProcessStep(session, NetworkProcessStep.InternalError);
				yield break; // Nothing more to do here.
			}

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, "Starting game finder");

			// Leave if already in a room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalLeaveRoom(session));
			}

			// Join to lobby, if not joined already
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalJoinToLobby(configuration.Lobby, session));
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.Cloud, NetworkState.NotConnected);
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Lobby);
			}
			InternalEndProcess(session);
		}

		#endregion

		#region Mode - Offline Session

		private IEnumerator DoStartOfflineSession(NetworkSession session)
		{
			var controller = session.Controller;

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, "Starting offline session");

			// Disconnect if already connected
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalDisconnect(session));
			}

			// Switch to offline if required
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				if (!PhotonNetwork.OfflineMode)
				{
					SetProcessStep(session, NetworkProcessStep.SettingOffline);

					PhotonNetwork.OfflineMode = true;
					RefreshNetworkState(session, NetworkState.Cloud); // Photon instantly goes into Cloud state when offline mode is activated.
				}
			}

			// Create offline room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.CreatingOfflineRoom);

				var isRequestSent = PhotonNetwork.CreateRoom(null);

				if (!isRequestSent)
				{
					FailProcess(session, "Create room request could not be sent."); // The request is actually processed locally but intentionally used the same string to use less resources, since this condition is rare.
				}
				else
				{
					// Offline room creation is instantly done. No need to wait.

					OnClientListChanged.Invoke();
				}
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session);
				// TODO: Quit application. There is nothing more to do if we can't even launch in offline mode.
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Room);
			}
			InternalEndProcess(session);
		}

		#endregion

		#region Mode - Single Player Session

		private IEnumerator DoStartSinglePlayerSession([NotNull]SinglePlayerConfiguration configuration, NetworkSession session)
		{
			var controller = session.Controller;

			var error = configuration.ValidateAndFix();
			if (error != null)
			{
				Log.CriticalError(error, this);
				SetProcessStep(session, NetworkProcessStep.InternalError);
				yield break; // Nothing more to do here.
			}

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, "Starting single player session");

			// Leave if already in a room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalLeaveRoom(session));
			}

			// Join to lobby, if not joined already
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalJoinToLobby(configuration.Lobby, session));
			}

			// Create room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.CreatingSinglePlayerRoom);

				var roomOptions = new RoomOptions
				{
					CleanupCacheOnLeave = true,
					IsOpen = false,
					IsVisible = false,
					MaxPlayers = (byte)1,
				};
				var isRequestSent = PhotonNetwork.CreateRoom(configuration.RoomName, roomOptions, configuration.Lobby, null);

				if (!isRequestSent)
				{
					FailProcess(session, "Create room request could not be sent.");
				}
				else
				{
					RefreshNetworkState(session, NetworkState.JoiningToRoom);

					// Wait for room to be created
					// TODO: Check for timeout (if not already handled by Photon). See 726179.
					while (PhotonNetwork.NetworkClientState != ClientState.Joined)
					{
						if (OverkillLogging)
							Log.Info("---------- Waiting for the single player room to be created...", this);
						if (_RoomCreationFailedFlag)
						{
							FailProcess(session, _RoomCreationFailedMessage);
							_RoomCreationFailedMessage = null;
						}

						//if (controller.IsCancelled) yield break; Intentionally commented out. Photon is not happy breaking the connection process in the middle.
						if (ProcessFailed) break;
						yield return null;
					}
					if (controller.IsCancelled) yield break; // Make sure this comes just after the waiting loop above.

					OnClientListChanged.Invoke();
				}
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.Lobby, NetworkState.Cloud, NetworkState.NotConnected);
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Room);
			}
			InternalEndProcess(session);
		}

		#endregion

		#region Mode - Host

		private IEnumerator DoStartHostSession([NotNull]HostConfiguration configuration, NetworkSession session)
		{
			var controller = session.Controller;

			var error = configuration.ValidateAndFix();
			if (error != null)
			{
				Log.CriticalError(error, this);
				SetProcessStep(session, NetworkProcessStep.InternalError);
				yield break; // Nothing more to do here.
			}

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, $"Starting to host room '{configuration.RoomName}'");

			// Leave if already in a room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalLeaveRoom(session));
			}

			// Join to lobby, if not joined already
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalJoinToLobby(configuration.Lobby, session));
			}

			// Create room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.CreatingHostRoom);

				var isRequestSent = PhotonNetwork.CreateRoom(configuration.RoomName, configuration.RoomOptions, configuration.Lobby, configuration.ExpectedUserIDs);

				if (!isRequestSent)
				{
					FailProcess(session, "Create room request could not be sent.");
				}
				else
				{
					RefreshNetworkState(session, NetworkState.JoiningToRoom);

					// Wait for room to be created
					// TODO: Check for timeout (if not already handled by Photon). See 726179.
					while (PhotonNetwork.NetworkClientState != ClientState.Joined)
					{
						if (OverkillLogging)
							Log.Info("---------- Waiting for the host room to be created...", this);
						if (_RoomCreationFailedFlag)
						{
							FailProcess(session, _RoomCreationFailedMessage);
							_RoomCreationFailedMessage = null;
						}

						//if (controller.IsCancelled) yield break; Intentionally commented out. Photon is not happy breaking the connection process in the middle.
						if (ProcessFailed) break;
						yield return null;
					}
					if (controller.IsCancelled) yield break; // Make sure this comes just after the waiting loop above.

					OnClientListChanged.Invoke();
				}
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.Lobby, NetworkState.Cloud, NetworkState.NotConnected);
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Room);
			}
			InternalEndProcess(session);
		}

		public void OnCreatedRoom()
		{
			// No task is done here intentionally. Required tasks that have to happen when creating a room
			// will be done in desired mode processes, instead of this callback.

			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.RoomCreatedInformation);
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			// No task is done here intentionally. Required tasks that have to happen when creating a room
			// will be done in desired mode processes, instead of this callback.

			var session = CurrentSession;
			var fullMessage = $"Failed to create room. Error '{returnCode}' reason: " + message;

			SetProcessStep(session, NetworkProcessStep.CreateRoomFailedInformation, fullMessage);

			if (IsCurrentlyProcessing)
			{
				// When polling for room creation status, this is the only way to know that the request is failed.
				_RoomCreationFailedMessage = message;
			}
		}

		#endregion

		#region Mode - Join

		private IEnumerator DoStartJoinSession([NotNull]JoinConfiguration configuration, NetworkSession session)
		{
			var controller = session.Controller;

			var error = configuration.ValidateAndFix();
			if (error != null)
			{
				Log.CriticalError(error, this);
				SetProcessStep(session, NetworkProcessStep.InternalError);
				yield break; // Nothing more to do here.
			}

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, $"Joining to room '{configuration.RoomName}'");

			// Leave if already in a room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalLeaveRoom(session));
			}

			// Join to lobby, if not joined already
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalJoinToLobby(configuration.Lobby, session));
			}

			// Join to room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.JoiningRoom);

				var isRequestSent = PhotonNetwork.JoinRoom(configuration.RoomName, configuration.ExpectedUserIDs);

				if (!isRequestSent)
				{
					FailProcess(session, "Join room request could not be sent.");
				}
				else
				{
					RefreshNetworkState(session, NetworkState.JoiningToRoom);

					// Wait for joining the room
					// TODO: Check for timeout (if not already handled by Photon). See 726179.
					while (PhotonNetwork.NetworkClientState != ClientState.Joined)
					{
						if (OverkillLogging)
							Log.Info("---------- Waiting for joining to the room...", this);
						if (_RoomJoiningFailedFlag)
						{
							FailProcess(session, _RoomJoiningFailedMessage);
							_RoomJoiningFailedMessage = null;
						}

						//if (controller.IsCancelled) yield break; Intentionally commented out. Photon is not happy breaking the connection process in the middle.
						if (ProcessFailed) break;
						yield return null;
					}
					if (controller.IsCancelled) yield break; // Make sure this comes just after the waiting loop above.

					OnClientListChanged.Invoke();
				}
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.Lobby, NetworkState.Cloud, NetworkState.NotConnected);
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Room);
			}
			InternalEndProcess(session);
		}

		public void OnJoinedRoom()
		{
			// No task is done here intentionally. Required tasks that have to happen when creating a room
			// will be done in desired mode processes, instead of this callback.

			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.JoinedRoomInformation);
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			// No task is done here intentionally. Required tasks that have to happen when creating a room
			// will be done in desired mode processes, instead of this callback.

			var fullMessage = $"Failed to join room. Error '{returnCode}' reason: " + message;

			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.JoinRoomFailedInformation, fullMessage);

			if (IsCurrentlyProcessing)
			{
				// When polling for room joining status, this is the only way to know that the request is failed.
				_RoomJoiningFailedMessage = message;
			}
		}

		#endregion

		#region Mode - Random Join

		private IEnumerator DoStartRandomJoinSession([NotNull]RandomJoinConfiguration configuration, NetworkSession session)
		{
			var controller = session.Controller;

			var error = configuration.ValidateAndFix();
			if (error != null)
			{
				Log.CriticalError(error, this);
				SetProcessStep(session, NetworkProcessStep.InternalError);
				yield break; // Nothing more to do here.
			}

			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			InternalStartProcess(session, "Joining to a random room");

			// Leave if already in a room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalLeaveRoom(session));
			}

			// Join to lobby, if not joined already
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalJoinToLobby(configuration.Lobby, session));
			}

			// Join to room
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.JoiningRoom);

				var isRequestSent = PhotonNetwork.JoinRandomRoom(configuration.ExpectedCustomRoomProperties, configuration.ExpectedMaxPlayers, configuration.MatchingType, configuration.Lobby, configuration.SqlLobbyFilter, configuration.ExpectedUserIDs);

				if (!isRequestSent)
				{
					FailProcess(session, "Join random room request could not be sent.");
				}
				else
				{
					RefreshNetworkState(session, NetworkState.JoiningToRoom);

					// Wait for joining the room
					// TODO: Check for timeout (if not already handled by Photon). See 726179.
					while (PhotonNetwork.NetworkClientState != ClientState.Joined)
					{
						if (OverkillLogging)
							Log.Info("---------- Waiting for joining to a random room...", this);
						if (_RoomJoiningFailedFlag)
						{
							FailProcess(session, _RoomJoiningFailedMessage);
							_RoomJoiningFailedMessage = null;
						}

						//if (controller.IsCancelled) yield break; Intentionally commented out. Photon is not happy breaking the connection process in the middle.
						if (ProcessFailed) break;
						yield return null;
					}
					if (controller.IsCancelled) yield break; // Make sure this comes just after the waiting loop above.

					OnClientListChanged.Invoke();
				}
			}

			// Finalize
			if (controller.IsCancelled) { controller.InformFinish(); yield break; }
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.Lobby, NetworkState.Cloud, NetworkState.NotConnected);
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Room);
			}
			InternalEndProcess(session);
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			// No task is done here intentionally. Required tasks that have to happen when creating a room
			// will be done in desired mode processes, instead of this callback.

			var fullMessage = $"Failed to join random room. Error '{returnCode}' reason: " + message;

			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.RandomJoinFailedInformation, fullMessage);

			if (IsCurrentlyProcessing)
			{
				_RoomJoiningFailedMessage = message;
			}
		}

		#endregion

		#region Leave Room

		public bool AutomaticallyRemovePlayerCustomPropertiesWhenLeavingRoom = true;

		private IEnumerator InternalLeaveRoom(NetworkSession session)
		{
			var controller = session.Controller;

			var processWasNotFailedBefore = !ProcessFailed;

			if (!PhotonNetwork.InRoom)
			{
				// We are not in a room. Nothing to do here.
				SetProcessStep(session, NetworkProcessStep.AlreadyNotInRoom);
				yield break; // Nothing more to do here.
			}

			if (controller.IsCancelled) yield break;
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.LeavingRoom);

				var isRequestSent = PhotonNetwork.LeaveRoom();

				if (!isRequestSent)
				{
					FailProcess(session, "Leave room request could not be sent.");
				}
				else
				{
					if (PhotonNetwork.OfflineMode)
					{
						// No need to wait. Leaving the room is instantly done in offline mode.
					}
					else
					{
						RefreshNetworkState(session, NetworkState.LeavingRoom);

						// Wait for leaving the room
						// TODO: Check for timeout (if not already handled by Photon). See 726179.
						while (PhotonNetwork.InRoom || PhotonNetwork.NetworkClientState == ClientState.Leaving)
						{
							if (OverkillLogging)
								Log.Info("---------- Waiting for leaving the room...", this);

							//if (controller.IsCancelled) yield break; Intentionally commented out. Photon is not happy breaking the connection process in the middle.
							if (ProcessFailed) break;
							yield return null;
						}
						if (controller.IsCancelled) yield break; // Make sure this comes just after the waiting loop above.

						// After leaving the room, Photon will try to connect to the cloud. So wait for it too.
						RefreshNetworkState(session, NetworkState.ConnectingToCloud);

						// Wait for connection process.
						// TODO: Check for timeout (if not already handled by Photon). See 726179.
						while (!PhotonNetwork.IsConnectedAndReady)
						{
							if (OverkillLogging)
								Log.Info("---------- Waiting for cloud connection to be established after leaving the room...", this);
							//if (controller.IsCancelled) yield break; Intentionally commented out. Photon is not happy breaking the connection process in the middle.
							if (ProcessFailed) break;
							yield return null;
						}
						if (controller.IsCancelled) yield break; // Make sure this comes just after the waiting loop above.
					}
				}
			}

			// Finalize
			if (controller.IsCancelled) yield break;
			if (ProcessFailed)
			{
				// One last stand!
				// We were in a room at the start of this method and we only wanted to leave the room peacefully.
				// Somewhere in between, things got awful. But hopefully if we are not in a room after the shitstorm,
				// accept the fact that we are in a state where we are happy. So just move on. But only try to recover
				// if the process was not failed before calling this method. Otherwise there might be other more
				// important problems.
				// See 108715 for a similar approach.
				yield return null; // Not sure if this is required but just a precaution. Wait a bit for network library to settle down.
				if (!PhotonNetwork.InRoom && processWasNotFailedBefore)
				{
					ClearFailedFlag();
					// Let's not assume the network state is in good condition.
					RefreshNetworkState(session, NetworkState.Cloud, NetworkState.NotConnected);
					SetProcessStep(session, NetworkProcessStep.LeftRoom);
				}
				else
				{
					RefreshNetworkState(session, NetworkState.Cloud, NetworkState.NotConnected);
					SetProcessStep(session, NetworkProcessStep.FailedToLeaveRoom);
				}
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Cloud);
				SetProcessStep(session, NetworkProcessStep.LeftRoom);
			}
		}

		public void OnLeftRoom()
		{
			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.LeftRoomInformation);

			if (AutomaticallyRemovePlayerCustomPropertiesWhenLeavingRoom)
			{
				PhotonNetwork.RemovePlayerCustomProperties(null);
			}

			if (!IsQuitting)
			{
				if (OverkillLogging)
					DebugLogNetworkState(session);

				//ResetLocalPlayerPhotonViewID();

				OnClientListChanged.Invoke();
			}
		}

		#endregion

		#region Join to Lobby

		private IEnumerator InternalJoinToLobby(TypedLobby lobby, NetworkSession session)
		{
			var controller = session.Controller;

			// Connect to cloud if required
			if (controller.IsCancelled) yield break;
			if (!ProcessFailed)
			{
				yield return StartCoroutine(InternalConnectToCloud(session));
			}

			// Check if we are in a lobby, and make sure it's the lobby we want.
			if (controller.IsCancelled) yield break;
			if (!ProcessFailed)
			{
				if (PhotonNetwork.InLobby)
				{
					if (PhotonNetwork.CurrentLobby.Name == lobby.Name && PhotonNetwork.CurrentLobby.Type == lobby.Type)
					{
						// We are already in the lobby. Nothing to do here.
						SetProcessStep(session, NetworkProcessStep.AlreadyInsideLobby);
						yield break; // Nothing more to do here.
					}
					else
					{
						// We are in a different lobby. Leave current lobby and connect to the requested one.
						SetProcessStep(session, NetworkProcessStep.ChangingLobby);

						var isRequestSent = PhotonNetwork.LeaveLobby();

						if (!isRequestSent)
						{
							FailProcess(session, "Leave lobby request could not be sent.");
						}
						else
						{
							//RefreshNetworkState(session, NetworkState.LeavingLobby); See LeavingLobby for detailed explanation.

							// TODO: Check for timeout (if not already handled by Photon). See 726179.
							while (PhotonNetwork.InLobby)
							{
								if (OverkillLogging)
									Log.Info("---------- Waiting for leaving the lobby...", this);
								if (controller.IsCancelled) yield break;
								if (ProcessFailed) break;
								yield return null;
							}
						}
					}
				}
			}

			// Join to lobby
			if (controller.IsCancelled) yield break;
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.JoiningToLobby);

				var isRequestSent = PhotonNetwork.JoinLobby(lobby);

				if (!isRequestSent)
				{
					FailProcess(session, "Join lobby request could not be sent.");
				}
				else
				{
					RefreshNetworkState(session, NetworkState.JoiningToLobby);

					// Wait for joining the lobby
					// TODO: Check for timeout (if not already handled by Photon). See 726179.
					while (!PhotonNetwork.InLobby)
					{
						if (OverkillLogging)
							Log.Info("---------- Waiting for joining to lobby...", this);
						if (controller.IsCancelled) yield break;
						if (ProcessFailed) break;
						yield return null;
					}
				}
			}

			// Finalize
			if (controller.IsCancelled) yield break;
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.Cloud, NetworkState.NotConnected);
				SetProcessStep(session, NetworkProcessStep.FailedToJoinLobby);
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Lobby);
				SetProcessStep(session, NetworkProcessStep.JoinedToLobby, $"Joined to lobby '{PhotonNetwork.CurrentLobby.Name}'.");
			}
		}

		public void OnJoinedLobby()
		{
			// No task is done here intentionally. Required tasks that have to happen here
			// will be done in desired mode processes, instead of this callback.

			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.JoinedLobbyInformation);
		}

		#endregion

		#region Leave Lobby

		// TODO: Not sure if first we have to leave the room or not. Does leaving the lobby also makes the client leave the room?
		private IEnumerator InternalLeaveLobby(NetworkSession session)
		{
			var controller = session.Controller;

			var processWasNotFailedBefore = !ProcessFailed;

			if (!PhotonNetwork.InLobby)
			{
				// We are not in a lobby. Nothing to do here.
				SetProcessStep(session, NetworkProcessStep.AlreadyNotInLobby);
				yield break; // Nothing more to do here.
			}

			if (controller.IsCancelled) yield break;
			if (!ProcessFailed)
			{
				SetProcessStep(session, NetworkProcessStep.LeavingLobby);

				var isRequestSent = PhotonNetwork.LeaveLobby();

				if (!isRequestSent)
				{
					FailProcess(session, "Leave lobby request could not be sent.");
				}
				else
				{
					// TODO: See if that's the case.
					//if (PhotonNetwork.OfflineMode)
					//{
					//	// No need to wait. Leaving the lobby is instantly done in offline mode.
					//}
					//else
					{
						//RefreshNetworkState(session, NetworkState.LeavingLobby); See LeavingLobby for detailed explanation.

						// TODO: Check for timeout (if not already handled by Photon). See 726179.
						while (PhotonNetwork.InLobby)
						{
							if (OverkillLogging)
								Log.Info("---------- Waiting for leaving the lobby...", this);
							if (controller.IsCancelled) yield break;
							if (ProcessFailed) break;
							yield return null;
						}
					}
				}
			}

			// Finalize
			if (controller.IsCancelled) yield break;
			if (ProcessFailed)
			{
				// One last stand!
				// We were in a lobby at the start of this method and we only wanted to leave the lobby peacefully.
				// Somewhere in between, things got awful. But hopefully if we are not in a lobby after the shitstorm,
				// accept the fact that we are in a state where we are happy. So just move on. But only try to recover
				// if the process was not failed before calling this method. Otherwise there might be other more
				// important problems.
				// See 108715 for a similar approach.
				yield return null; // Not sure if this is required but just a precaution. Wait a bit for network library to settle down.
				if (!PhotonNetwork.InLobby && processWasNotFailedBefore)
				{
					ClearFailedFlag();
					// Let's not assume the network state is in good condition.
					RefreshNetworkState(session, NetworkState.Cloud, NetworkState.NotConnected);
					SetProcessStep(session, NetworkProcessStep.LeftLobby);
				}
				else
				{
					RefreshNetworkState(session, NetworkState.Cloud, NetworkState.NotConnected);
					SetProcessStep(session, NetworkProcessStep.FailedToLeaveLobby);
				}
			}
			else
			{
				RefreshNetworkState(session, NetworkState.Cloud);
				SetProcessStep(session, NetworkProcessStep.LeftLobby);
			}
		}

		public void OnLeftLobby()
		{
			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.LeftLobbyInformation);
		}

		#endregion

		#region Connect To Cloud

		private IEnumerator InternalConnectToCloud(NetworkSession session)
		{
			var controller = session.Controller;

			// Switch to online if required
			if (controller.IsCancelled) yield break;
			if (!ProcessFailed)
			{
				if (PhotonNetwork.OfflineMode)
				{
					SetProcessStep(session, NetworkProcessStep.SettingOnline);

					PhotonNetwork.OfflineMode = false;
					RefreshNetworkState(session, NetworkState.NotConnected);
				}
			}

			var alreadyConnected = false;

			if (controller.IsCancelled) yield break;
			if (!ProcessFailed)
			{
				alreadyConnected = PhotonNetwork.IsConnectedAndReady;
				if (!alreadyConnected)
				{
					SetProcessStep(session, NetworkProcessStep.ConnectingToCloud);

					var isRequestSent = PhotonNetwork.ConnectUsingSettings();

					if (!isRequestSent)
					{
						FailProcess(session, "Cloud connection request could not be sent");
					}
					else
					{
						RefreshNetworkState(session, NetworkState.ConnectingToCloud);

						// Wait for connection process.
						// TODO: Check for timeout (if not already handled by Photon). See 726179.
						while (PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterserver)
						{
							if (OverkillLogging)
								Log.Info("---------- Waiting for cloud connection to be established...", this);
							//if (controller.IsCancelled) yield break; Intentionally commented out. Photon is not happy breaking the connection process in the middle.
							if (ProcessFailed) break;
							yield return null;
						}
						if (controller.IsCancelled) yield break; // Make sure this comes just after the waiting loop above.
					}
				}
			}

			// Finalize
			if (controller.IsCancelled) yield break;
			if (ProcessFailed)
			{
				RefreshNetworkState(session, NetworkState.NotConnected);
				SetProcessStep(session, NetworkProcessStep.CloudConnectionFailed);
			}
			else
			{
				if (alreadyConnected)
				{
					//RefreshNetworkState(...); No connection attempt has been done in this method. So there is no need to refresh the network state. Who knows which state we are in right know. It can be anything.
					SetProcessStep(session, NetworkProcessStep.AlreadyConnectedToCloud);
				}
				else
				{
					RefreshNetworkState(session, NetworkState.Cloud);
					SetProcessStep(session, NetworkProcessStep.ConnectedToCloud);
				}
			}
		}

		public void OnConnectedToMaster()
		{
			// No task is done here intentionally. Required tasks that have to happen here
			// will be done in desired mode processes, instead of this callback.

			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.ConnectedToMasterInformation);
		}

		public void OnConnected()
		{
			// No task is done here intentionally. Required tasks that have to happen here
			// will be done in desired mode processes, instead of this callback.

			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.ConnectedToPhotonInformation);
		}

		#endregion

		#region Connection Fails and Disconnect / Reconnect

		/// <summary>
		/// Initiates the processing of current desired mode again. Does not matter if it's already
		/// in the middle of processing or not.
		/// </summary>
		[Obsolete("No such thing as 'Reconnect'. Set a desired mode instead. See if 'RestartDesiredMode' fits your needs.", true)]
		public static void Reconnect()
		{
		}

		[Obsolete("No such thing as 'Disconnect'. Set a desired mode instead.", true)]
		public static void Disconnect()
		{
		}

		private IEnumerator InternalDisconnect(NetworkSession session)
		{
			var controller = session.Controller;

			if (controller.IsCancelled) yield break;
			if (PhotonNetwork.IsConnected) // TODO: See if we should also check for IsConnecting state, so that it will be possible to break the connection process in the middle.
			{
				SetProcessStep(session, NetworkProcessStep.Disconnecting);
				Log.Info("Disconnecting...", this);
				var wasOffline = PhotonNetwork.OfflineMode;
				PhotonNetwork.Disconnect();

				if (wasOffline)
				{
					// No need to wait. Photon will instantly get to the Disconnected state.
					// The downside is we will skip the DisconnectingFromCloud state and that
					// may confuse network state listeners. Maybe we can implement something
					// like the old _CreatingRoomFlag that is set just before calling
					// PhotonNetwork.Disconnect above, but whatever.
				}
				else
				{
					RefreshNetworkState(session, NetworkState.DisconnectingFromCloud);

					while (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.Disconnecting)
					{
						if (OverkillLogging)
							Log.Info("---------- Waiting for connection to be closed...", this);
						if (controller.IsCancelled) yield break;
						if (ProcessFailed) break;
						yield return null;
					}
				}

				SetProcessStep(session, NetworkProcessStep.Disconnected);
				RefreshNetworkState(session, NetworkState.NotConnected);
			}
		}

		/// <param name="controller">Even though the current controller is well known, caller of this method must specify which controller will be marked as failed. This provides a more robust design to prevent concurrency problems.</param>
		private void FailProcess(NetworkSession session, string message = null)
		{
			if (!IsCurrentlyProcessing)
			{
				Log.CriticalError("Tried to fail process while there was none. Fail message: " + message, this);
				return;
			}

			SetProcessStep(session, NetworkProcessStep.ProcessFailed, message);

			session.Controller.InformFail();

			ProcessFailed = true;
			//ProcessFailMessage = message;
		}

		public void OnDisconnected(DisconnectCause cause)
		{
			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.DisconnectedInformation);

			if (OverkillLogging)
				DebugLogNetworkState(session);

			switch (cause)
			{
				// Process failure
				case DisconnectCause.ExceptionOnConnect:
				case DisconnectCause.Exception:
				case DisconnectCause.InvalidAuthentication:
				case DisconnectCause.CustomAuthenticationFailed:
				case DisconnectCause.MaxCcuReached:
				case DisconnectCause.InvalidRegion:
				case DisconnectCause.OperationNotAllowedInCurrentState:
				case DisconnectCause.AuthenticationTicketExpired:
				// Connection lost
				case DisconnectCause.None:
				case DisconnectCause.ClientTimeout:
				case DisconnectCause.ServerTimeout:
				case DisconnectCause.DisconnectByServerReasonUnknown:
					{
						if (IsCurrentlyProcessing)
						{
							FailProcess(session, $"Disconnected (Error: {cause})");
						}
					}
					break;

				// Connection closed by request
				case DisconnectCause.DisconnectByServerLogic:
				case DisconnectCause.DisconnectByClientLogic:
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(cause), cause, null);
			}

			OnClientListChanged.Invoke();
		}

		#endregion

		#region Connected Clients

		public class ClientListEvent : UnityEvent { }
		public static readonly ClientListEvent OnClientListChanged = new ClientListEvent();
		public class ClientConnectedEvent : UnityEvent<Player> { }
		public static readonly ClientConnectedEvent OnClientConnected = new ClientConnectedEvent();
		public class ClientDisconnectedEvent : UnityEvent<Player> { }
		public static readonly ClientDisconnectedEvent OnClientDisconnected = new ClientDisconnectedEvent();

		public void OnPlayerEnteredRoom(Player newPlayer)
		{
			//var session = CurrentSession;
			Log.Info($"Player '{newPlayer.NickName}' connected", this);

			OnClientConnected.Invoke(newPlayer);
			OnClientListChanged.Invoke();
		}

		public void OnPlayerLeftRoom(Player otherPlayer)
		{
			//var session = CurrentSession;
			Log.Info($"Player '{otherPlayer.NickName}' disconnected", this);

			OnClientDisconnected.Invoke(otherPlayer);
			OnClientListChanged.Invoke();
		}

		#endregion

		#region Lobby Statistics

		public class RoomListEvent : UnityEvent<List<RoomInfo>> { }
		public static readonly RoomListEvent OnRoomListChanged = new RoomListEvent();
		public class LobbyStatisticsEvent : UnityEvent<List<TypedLobbyInfo>> { }
		public static readonly LobbyStatisticsEvent OnLobbyStatisticsChanged = new LobbyStatisticsEvent();

		public static int OnlinePlayerCount { get { return PhotonNetwork.CountOfPlayers; } }
		public static int OnlinePlayerCountInRooms { get { return PhotonNetwork.CountOfPlayersInRooms; } }
		public static int OnlinePlayerCountInMenus { get { return PhotonNetwork.CountOfPlayersOnMaster; } }
		public static int OnlineRoomCount { get { return PhotonNetwork.CountOfRooms; } }

		public static List<RoomInfo> RoomList;
		public static List<TypedLobbyInfo> LobbyStatistics;

		public void OnRoomListUpdate(List<RoomInfo> roomList)
		{
			//var session = CurrentSession;
			Log.Info("OnReceivedRoomListUpdate", this);

			RoomList = roomList;

			OnRoomListChanged.Invoke(roomList);
		}

		public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
		{
			//var session = CurrentSession;
			Log.Info("OnLobbyStatisticsUpdate", this);

			LobbyStatistics = lobbyStatistics;

			foreach (var lobby in lobbyStatistics)
			{
				Log.Info(lobby.ToHumanReadableString(), this);
			}

			OnLobbyStatisticsChanged.Invoke(lobbyStatistics);
		}

		#endregion

		#region Player Properties

		// This is handled inside player object. Processing it here has some pros and cons but decided it's better we use it inside player object.
		//public override void OnPlayerPropertiesUpdate(Player player, Hashtable changedProps)
		//{
		//	Log($"OnPhotonPlayerPropertiesChanged    player: '{player}'     updatedProps: '{changedProps.ToJoinedString()}'");
		//}

		/*
		/// <summary>
		/// Resets local player's photon view ID. It should be set to zero before another connection attempt.
		/// </summary>
		private void ResetLocalPlayerPhotonViewID()
		{
			if (!PlayerManager.IsInstanceAvailable)
			{
				LogError("Tried to reset local player view ID but manager was not instantiated.");
				return;
			}
			if (!PlayerManager.Instance.IsLocalPlayerSet)
			{
				LogError("Tried to reset local player view ID but player was not instantiated.");
				return;
			}

			var localPlayerPhotonView = PlayerManager.Instance.LocalPlayer.photonView;
			if (localPlayerPhotonView.viewID != 0)
			{
				Log("Local player view ID reset");

				localPlayerPhotonView.viewID = 0;
			}
		}
		*/

		#endregion

		#region Player Activity

		// TODO: See if this is abandoned in PUN2 or just renamed.

		//public class PlayerActivityEvent : UnityEvent<Player> { }
		//public static readonly PlayerActivityEvent OnPlayerActivityChanged = new PlayerActivityEvent();

		//public override void OnPhotonPlayerActivityChanged(Player otherPlayer)
		//{
		//	var session = CurrentSession;
		//	Log($"OnPhotonPlayerActivityChanged    otherPlayer: '{otherPlayer}'", session);

		//	OnPlayerActivityChanged.Invoke(otherPlayer);
		//}

		#endregion

		#region Friends

		public class FriendListEvent : UnityEvent<List<FriendInfo>> { }
		public static readonly FriendListEvent OnFriendListChanged = new FriendListEvent();

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			//var session = CurrentSession;
			Log.Info("OnUpdatedFriendList", this);

			OnFriendListChanged.Invoke(friendList);
		}

		#endregion

		#region Stats - Data

		public static int TotalSentPacketCount;
		public static int TotalSentPacketBytes;
		public static int SentPacketBytesPerSecond;
		public static float SentKBsPerSecond => SentPacketBytesPerSecond / 1000f;
		private static int _TotalSentPacketBytesInPreviousMeasurement;
		public static int SentPacketCountPerSecond;
		private static int _TotalSentPacketCountInPreviousMeasurement;

		public static int TotalReceivedPacketCount;
		public static int TotalReceivedPacketBytes;
		public static int ReceivedPacketBytesPerSecond;
		public static float ReceivedKBsPerSecond => ReceivedPacketBytesPerSecond / 1000f;
		private static int _TotalReceivedPacketBytesInPreviousMeasurement;
		public static int ReceivedPacketCountPerSecond;
		private static int _TotalReceivedPacketCountInPreviousMeasurement;

		#endregion

		#region Stats - Controller

		[Header("Stats")]
		public bool EnableNetworkStatisticsAtStart = false;
		public bool EnableNetworkStatisticsGraphPlottingAtStart = false;

		public static readonly UnityEvent OnNetworkStatsRefresh = new UnityEvent();

		private static bool _NetworkStatisticsEnabled;
		public static bool NetworkStatisticsEnabled
		{
			get { return _NetworkStatisticsEnabled; }
			set
			{
				if (_NetworkStatisticsEnabled == value)
					return;
				if (_NetworkStatisticsEnabled != PhotonNetwork.NetworkStatisticsEnabled)
					throw new InternalException(751952); // PhotonNetwork.NetworkStatisticsEnabled should not be used elsewhere.
				if (!IsInstanceAvailable)
					throw new InternalException(851952); // This is only available in play mode. (It requires FastInvokes)
				_NetworkStatisticsEnabled = value;

				ResetStatInternals();

				if (value)
				{
					Instance.FastInvokeRepeating(InternalUpdateNetworkStats, 1, 1, true);
				}
				else
				{
					Instance.CancelFastInvoke(InternalUpdateNetworkStats);
				}

				InternalRefreshNetworkStatsGraph();
				OnNetworkStatsRefresh.Invoke(); // This will initialize UIs.

				PhotonNetwork.NetworkStatisticsEnabled = value;
			}
		}

		private static void ResetStatInternals()
		{
			TotalSentPacketBytes = 0;
			TotalSentPacketCount = 0;
			SentPacketBytesPerSecond = 0;
			SentPacketCountPerSecond = 0;
			_TotalSentPacketBytesInPreviousMeasurement = 0;
			_TotalSentPacketCountInPreviousMeasurement = 0;
			TotalReceivedPacketBytes = 0;
			TotalReceivedPacketCount = 0;
			ReceivedPacketBytesPerSecond = 0;
			ReceivedPacketCountPerSecond = 0;
			_TotalReceivedPacketBytesInPreviousMeasurement = 0;
			_TotalReceivedPacketCountInPreviousMeasurement = 0;
		}

		private static void InternalUpdateNetworkStats()
		{
			var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
			var IncomingTrafficStats = peer.TrafficStatsIncoming;
			var OutgoingTrafficStats = peer.TrafficStatsOutgoing;

			TotalSentPacketBytes = OutgoingTrafficStats.TotalPacketBytes;
			TotalSentPacketCount = OutgoingTrafficStats.TotalPacketCount;
			SentPacketBytesPerSecond = TotalSentPacketBytes - _TotalSentPacketBytesInPreviousMeasurement;
			_TotalSentPacketBytesInPreviousMeasurement = TotalSentPacketBytes;
			SentPacketCountPerSecond = TotalSentPacketCount - _TotalSentPacketCountInPreviousMeasurement;
			_TotalSentPacketCountInPreviousMeasurement = TotalSentPacketCount;

			TotalReceivedPacketBytes = IncomingTrafficStats.TotalPacketBytes;
			TotalReceivedPacketCount = IncomingTrafficStats.TotalPacketCount;
			ReceivedPacketBytesPerSecond = TotalReceivedPacketBytes - _TotalReceivedPacketBytesInPreviousMeasurement;
			_TotalReceivedPacketBytesInPreviousMeasurement = TotalReceivedPacketBytes;
			ReceivedPacketCountPerSecond = TotalReceivedPacketCount - _TotalReceivedPacketCountInPreviousMeasurement;
			_TotalReceivedPacketCountInPreviousMeasurement = TotalReceivedPacketCount;

			InternalRefreshNetworkStatsGraph();
			OnNetworkStatsRefresh.Invoke();
		}

		#endregion

		#region Stats - Graph

		public static bool NetworkStatisticsPlottingEnabled = false;

		private static Graph NetworkStatisticsGraph;
		private static Channel[] NetworkStatisticsGraphChannels;

		private static void InternalRefreshNetworkStatsGraph()
		{
			var shouldDraw = NetworkStatisticsEnabled && NetworkStatisticsPlottingEnabled;

			if (shouldDraw)
			{
				// Create if not exists.
				if (NetworkStatisticsGraph == null)
				{
					Graph.SetupGraphWithXYChannels(true, ref NetworkStatisticsGraph, "Network Send/Receive Delta", Instance.gameObject,
						ValueAxisRangeConfiguration.CreateAdaptive(), ref NetworkStatisticsGraphChannels,
						true, true, "Sent", "Received", Color.red, Color.green);
				}
				else // Do not draw at the first call. This will probably be 0.
				{
					// Send values to graph
					var time = Time.unscaledTime;
					var frame = Time.frameCount;
					NetworkStatisticsGraphChannels[0].Sample(SentPacketBytesPerSecond, time, frame);
					NetworkStatisticsGraphChannels[1].Sample(ReceivedPacketBytesPerSecond, time, frame);
				}
			}
			else
			{
				// Destroy if exists.
				if (NetworkStatisticsGraph != null)
				{
					// Close channels
					foreach (var channel in NetworkStatisticsGraphChannels)
					{
						channel.Close();
					}
					NetworkStatisticsGraphChannels = null;
					// Close graph
					NetworkStatisticsGraph.Close();
					NetworkStatisticsGraph = null;
				}
			}
		}

		#endregion

		#region Simulation

		// TODO:

		[Header("Network Simulation")]
		public float SimulationPacketDropChance = 0f; // Value between 0-1
		public int SimulationNetworkLatency = 0; // msec

		#endregion

		#region Version

		private static string _GameVersion;
		private static string GameVersion
		{
			get
			{
				if (string.IsNullOrEmpty(_GameVersion))
				{
					// Game version is not set yet. Make sure it's set before calling the getter.
					throw new InternalException(17659823);
				}
				return _GameVersion;
			}
		}

		public static void SetVersion(string value)
		{
			if (!string.IsNullOrEmpty(_GameVersion))
			{
				Log.InternalError(27659823);
				// TODO: SECURITY: Possible tampering attempt.
				return;
			}

			_GameVersion = value;
			PhotonNetwork.GameVersion = GameVersion;
		}

		#endregion

		#region Log

		public bool VerboseLogging = false; // Check this at the location of method call for performance reasons.
		public bool OverkillLogging = false; // Check this at the location of method call for performance reasons.

		private void UpdateLogPrefix(NetworkSession session)
		{
			Log.RegisterPrefix(this, "NetConMan-" + (session != null ? session.ID.ToString() : "NA"));
		}

		public void DebugLogNetworkState(NetworkSession session)
		{
			Log.Info($@"Network state: {GrabStateOfPhoton()}
			NetworkClientState : {PhotonNetwork.NetworkClientState}
			Server : {PhotonNetwork.Server}
			IsConnected : {PhotonNetwork.IsConnected}
			IsConnectedAndReady : {PhotonNetwork.IsConnectedAndReady}
			AutomaticallySyncScene : {PhotonNetwork.AutomaticallySyncScene}
			InRoom : {PhotonNetwork.InRoom}
			InLobby : {PhotonNetwork.InLobby}
			OfflineMode : {PhotonNetwork.OfflineMode}
			ConnectMethod : {PhotonNetwork.ConnectMethod}", this);
		}

		#endregion



		// TODO: What to do with these below?

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.AuthenticationFailedInformation, debugMessage);

			if (VerboseLogging)
				Log.Info($"Custom authentication failed: '{debugMessage}'", this);
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
			var session = CurrentSession;
			SetProcessStep(session, NetworkProcessStep.AuthenticationResponseInformation);

			if (VerboseLogging)
				Log.Info($"Custom authentication response: '{data.ToJoinedString()}'", this);
		}

		public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			//var session = CurrentSession;
			Log.Info($"OnPhotonCustomRoomPropertiesChanged    propertiesThatChanged: '{propertiesThatChanged.ToJoinedString()}'", this);
		}

		public void OnWebRpcResponse(OperationResponse response)
		{
			//var session = CurrentSession;
			Log.Info($"OnWebRpcResponse    response: '{response}'", this);
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
		}

		public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
		{
		}
	}

}
