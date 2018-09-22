#if BeyondAudioUsesUnityAudio

using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;

namespace Extenity.BeyondAudio.Editor
{

	[CustomPropertyDrawer(typeof(AudioEvent))]
	public class AudioEventPropertyDrawer : ExtenityPropertyDrawerBase<AudioEvent>
	{
	}

}

#endif
