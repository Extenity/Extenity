using Photon.Pun;
using UnityEngine;

namespace BeyondNetworking
{

	public class NetworkedBehaviour : MonoBehaviour
	{
		#region Initialization

		//protected void Awake()
		//{
		//}

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
	}

}
