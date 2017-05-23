using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	public class TickCounter : MonoBehaviour
	{
		#region Singleton

		[RuntimeInitializeOnLoadMethod]
		private static void CreateInstance()
		{
			var instance = new GameObject("_TickCounter").AddComponent<TickCounter>();
			instance.gameObject.hideFlags = HideFlags.HideAndDontSave;
			DontDestroyOnLoad(instance.gameObject);
		}

		#endregion

		#region Deinitialization

		// This object stays in scene after quitting play mode in editor. So we manually destroy it here.
		private void OnApplicationQuit()
		{
			Destroy(gameObject);
		}

		#endregion

		#region Update

		private void FixedUpdate()
		{
			_Ticks++;
		}

		#endregion

		private static int _Ticks;
		public static int Ticks
		{
			get
			{
				return _Ticks;
			}
		}
	}

}
