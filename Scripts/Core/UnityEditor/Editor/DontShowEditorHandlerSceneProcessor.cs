using Extenity.GameObjectToolbox;
using Extenity.GameObjectToolbox.Editor;
using UnityEditor.Build;
#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;
#endif
using UnityEngine.SceneManagement;

namespace Extenity.UnityEditorToolbox.Editor
{

	public class DontShowEditorHandlerSceneProcessor :
#if UNITY_2018_1_OR_NEWER
		IProcessSceneWithReport
#else
		IProcessScene
#endif
	{
		public int callbackOrder { get { return -1000; } }

#if UNITY_2018_1_OR_NEWER
		public void OnProcessScene(Scene scene, BuildReport report)
#else
		public void OnProcessScene(Scene scene)
#endif
		{
			scene.DestroyAllComponents<DontShowEditorHandler>(true, false, true);
		}
	}

}
