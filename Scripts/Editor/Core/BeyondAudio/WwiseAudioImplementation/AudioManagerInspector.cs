#if BeyondAudioUsesWwiseAudio

using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[CustomEditor(typeof(AudioManager))]
	public class AudioManagerInspector : ExtenityEditorBase<AudioManager>
	{
		protected override void OnEnableDerived()
		{
		}

		protected override void OnDisableDerived()
		{
		}

		protected override void OnBeforeDefaultInspectorGUI()
		{
		}

		protected override void OnAfterDefaultInspectorGUI()
		{
		}
	}

}

#endif
