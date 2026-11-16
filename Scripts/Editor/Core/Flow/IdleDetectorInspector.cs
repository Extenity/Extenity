#if ExtenityFastInvoke

using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.FlowToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="IdleDetector"/>: whether it currently detects idling, kept up to date.
	/// </summary>
	[CustomEditor(typeof(IdleDetector))]
	[CanEditMultipleObjects]
	public sealed class IdleDetectorInspector : ObjectInspector<IdleDetector>
	{
		protected override void Configure(InspectorBuilder<IdleDetector> b)
		{
			b.Live("Is Idle", detector => detector.IsIdle).After(x => x.RequiredTimeToGoIntoIdle);
		}
	}

}

#endif
