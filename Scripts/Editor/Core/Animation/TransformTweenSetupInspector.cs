using DG.Tweening;
using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.AnimationToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="TransformTweenSetup"/>: each ease has a button beside it that switches that part
	/// of the animation off, and the four space options are two rows of check boxes.
	/// </summary>
	[CustomPropertyDrawer(typeof(TransformTweenSetup))]
	public sealed class TransformTweenSetupInspector : NestedInspector<TransformTweenSetup>
	{
		internal const string DoNotAnimateLabel = "Do Not Animate";
		private const float DoNotAnimateWidth = 110f;

		protected override void Configure(InspectorBuilder<TransformTweenSetup> b)
		{
			b.Field(x => x.Location).Required("Location is required.");

			b.Row(r =>
			{
				r.Field(x => x.MovementEase).Label("Movement").Grow();
				r.Button(DoNotAnimateLabel, setup => setup.RemoveMovementAnimation())
				 .Size(ButtonSize.Small).Width(DoNotAnimateWidth)
				 .EnabledWhen(setup => setup.MovementEase != Ease.Unset);
			});
			b.Row(r =>
			{
				r.Field(x => x.RotationEase).Label("Rotation").Grow();
				r.Button(DoNotAnimateLabel, setup => setup.RemoveRotationAnimation())
				 .Size(ButtonSize.Small).Width(DoNotAnimateWidth)
				 .EnabledWhen(setup => setup.RotationEase != Ease.Unset);
			});
			b.Row(r =>
			{
				r.Field(x => x.ScaleEase).Label("Scale").Grow();
				r.Button(DoNotAnimateLabel, setup => setup.RemoveScaleAnimation())
				 .Size(ButtonSize.Small).Width(DoNotAnimateWidth)
				 .EnabledWhen(setup => setup.ScaleEase != Ease.Unset);
			});

			b.Row(r =>
			{
				r.Field(x => x.UseLocalPosition).ToggleLeft().Grow();
				r.Field(x => x.MoveInLocal).ToggleLeft().Grow();
			});
			b.Row(r =>
			{
				r.Field(x => x.UseLocalRotation).ToggleLeft().Grow();
				r.Field(x => x.RotateInLocal).ToggleLeft().Grow();
			});
		}
	}

}
