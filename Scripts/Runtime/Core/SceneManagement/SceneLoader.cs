#if UNITY_5_3_OR_NEWER

using Extenity.GameObjectToolbox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.SceneManagementToolbox
{

	public class SceneLoader : MonoBehaviour
	{
		#region Configuration

		public enum SceneLoadTime
		{
			Custom,
			Awake,
			Start,
		}

		public string SceneName;
		public SceneLoadTime LoadTime = SceneLoadTime.Awake;
		public LoadSceneMode LoadMode = LoadSceneMode.Additive;
		public bool DestroyAfterDone = true;

		#endregion

		#region Initialization

		protected void Awake()
		{
			if (LoadTime == SceneLoadTime.Awake)
			{
				LoadScene();
			}
		}

		protected void Start()
		{
			if (LoadTime == SceneLoadTime.Start)
			{
				LoadScene();
			}
		}

		#endregion

		#region Deinitialization

		private void DestroySelfIfSetTo()
		{
			if (DestroyAfterDone)
			{
				GameObjectTools.DestroyComponentThenGameObjectIfNoneLeft(this);
			}
		}

		#endregion

		#region Load Scene

		public void LoadScene()
		{
			SceneManager.LoadScene(SceneName, LoadMode);

			DestroySelfIfSetTo();
		}

		#endregion
	}

}

#endif
