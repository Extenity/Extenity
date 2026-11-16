#if PACKAGE_PHYSICS

using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.AnimationToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="DraggableObject"/>: only the settings of the selected dragging type are shown.
	/// </summary>
	[CustomEditor(typeof(DraggableObject))]
	[CanEditMultipleObjects]
	public sealed class DraggableObjectInspector : ObjectInspector<DraggableObject>
	{
		protected override void Configure(InspectorBuilder<DraggableObject> b)
		{
			b.Field(x => x.DistanceFromCamera).VisibleWhen(IsHovering);
			b.Field(x => x.DistanceFromHitPoint).VisibleWhen(IsSnapping);
			b.Field(x => x.RaycastMaxDistance).VisibleWhen(IsSnapping);
			b.Field(x => x.RaycastLayerMask).VisibleWhen(IsSnapping);
		}

		private static bool IsHovering(DraggableObject draggable) => draggable.DraggingType == ObjectDraggingType.HoverInFrontOfCamera;
		private static bool IsSnapping(DraggableObject draggable) => draggable.DraggingType == ObjectDraggingType.SnapWithRaycast;
	}

}

#endif
