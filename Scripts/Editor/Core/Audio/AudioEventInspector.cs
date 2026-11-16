#if ExtenityAudio

using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.Audio.Editor
{

	/// <summary>
	/// The inspector of <see cref="AudioEvent"/>: only the clips of the selected event type are shown, and the
	/// randomness option only when there is more than one clip to choose from.
	/// </summary>
	[CustomPropertyDrawer(typeof(AudioEvent))]
	public sealed class AudioEventInspector : NestedInspector<AudioEvent>
	{
		protected override void Configure(InspectorBuilder<AudioEvent> b)
		{
			b.Title(audioEvent => audioEvent.Name);
			b.Field(x => x.Clips).VisibleWhen(audioEvent => audioEvent.Type == AudioEventType.Regular);
			b.Field(x => x.WeightedGroups).VisibleWhen(audioEvent => audioEvent.Type == AudioEventType.WeightedGroups);
			b.Field(x => x.EnsureNonrecurringRandomness).VisibleWhen(HasMoreThanOneClip);
		}

		/// <summary>
		/// The value is a copy made from serialized data, so its clip count (not serialized) is calculated here.
		/// </summary>
		internal static bool HasMoreThanOneClip(AudioEvent audioEvent)
		{
			audioEvent.CalculateInternals();
			return audioEvent.ClipCount > 1;
		}
	}

}

#endif
