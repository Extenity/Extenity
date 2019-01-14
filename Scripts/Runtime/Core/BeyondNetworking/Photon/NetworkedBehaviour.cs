using Photon.Pun;
using UnityEngine;

namespace BeyondNetworking
{

	public abstract class NetworkedBehaviour : MonoBehaviour
	{
		#region Initialization

		protected void InitializeNetworkedObjectInstantiation()
		{
			Log.Info($"Instantiating networked object '{GetType().Name}'", this);

			InitializeAssignedNetworkSession();
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region PhotonView

		private PhotonView _PhotonView;
		public PhotonView PhotonView
		{
			get
			{
				if (_PhotonView == null)
				{
					_PhotonView = gameObject.GetComponent<PhotonView>();
				}
				return _PhotonView;
			}
		}

		#endregion

		#region Assigned Network Session

		private NetworkSession _AssignedNetworkSession;
		protected NetworkSession AssignedNetworkSession
		{
			get
			{
				if (_AssignedNetworkSession == null)
				{
					throw new InternalException(5375987); // Tried to get before initialized.
				}
				return _AssignedNetworkSession;
			}
		}

		protected void InitializeAssignedNetworkSession()
		{
			_AssignedNetworkSession = NetworkConnectivityManager.CurrentSession;
			if (_AssignedNetworkSession == null)
			{
				throw new InternalException(4375987); // Tried to initialize while there is no ongoing session.
			}
		}

		// TODO: Implement this for all RPCs in application.
		/*
		/// <summary>
		/// Use this in the first line of every PunRPC. Full usage is:
		/// if (IsRPCInvalid) return;
		/// </summary>
		protected bool IsRPCInvalid
		{
			get
			{
				if (NetworkConnectivityManager.CurrentSession != AssignedNetworkSession)
				{
					var previousMethod = DebugReflection.PreviousMethodName;
					Log.Warning($"Called '{previousMethod}' out of its network session.");
					return true;
				}
				return false;
			}
		}
		*/

		#endregion
	}

}
