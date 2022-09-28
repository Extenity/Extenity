#if ExtenityAudio

using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;

namespace Extenity.Audio.Editor
{

	[CustomPropertyDrawer(typeof(AudioEvent))]
	public class AudioEventPropertyDrawer : ExtenityPropertyDrawerBase<AudioEvent>
	{
	}

}

#endif
