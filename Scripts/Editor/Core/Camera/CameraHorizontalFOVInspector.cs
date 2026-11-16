using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.CameraToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="CameraHorizontalFOV"/>: the camera is required, and a changed field of view is
	/// applied to the camera on its next update.
	/// </summary>
	[CustomEditor(typeof(CameraHorizontalFOV))]
	[CanEditMultipleObjects]
	public sealed class CameraHorizontalFOVInspector : ObjectInspector<CameraHorizontalFOV>
	{
		protected override void Configure(InspectorBuilder<CameraHorizontalFOV> b)
		{
			b.Field(x => x.Camera).Required("Camera is required.");
			b.Field(x => x.HorizontalFOV).OnChanged(component => component.Invalidate());
		}
	}

}
