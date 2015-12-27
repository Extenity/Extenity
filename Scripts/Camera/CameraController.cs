using UnityEngine;
using UnityEngine.EventSystems;

namespace Extenity.CameraManagement
{

	public abstract class CameraController : MonoBehaviour
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

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Camera

		public Camera Camera;

		#endregion

		#region User Controls

		public abstract bool IsAxisActive { get; set; }
		public abstract bool IsMouseActive { get; set; }

		#endregion

		#region GUI

		private EventSystem CachedEventSystem;

		public bool IsGUIActive
		{
			get
			{
				if (CachedEventSystem == null)
				{
					CachedEventSystem = FindObjectOfType<EventSystem>();
				}
				if (CachedEventSystem == null)
				{
					Debug.LogErrorFormat(this, "Scene should have an EventSystem for camera controller on game object '{0}' to work.", gameObject.name);
					return false;
				}
				return CachedEventSystem.currentSelectedGameObject != null;
			}
		}

		#endregion
	}

}
