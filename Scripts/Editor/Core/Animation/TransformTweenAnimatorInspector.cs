using Extenity.InspectorToolbox.Editor;
using UnityEditor;

namespace Extenity.AnimationToolbox.Editor
{

	/// <summary>
	/// The inspector of <see cref="TransformTweenAnimator"/>: the animation under its own title and a row of
	/// playback buttons at the bottom. Play needs Play Mode, where the tweens run; Jump also works in Edit Mode
	/// and can be undone.
	/// </summary>
	[CustomEditor(typeof(TransformTweenAnimator))]
	[CanEditMultipleObjects]
	public sealed class TransformTweenAnimatorInspector : ObjectInspector<TransformTweenAnimator>
	{
		protected override void Configure(InspectorBuilder<TransformTweenAnimator> b)
		{
			b.Group("Animation", GroupStyle.Title, g => g.Fields(x => x.Animation));
			b.Field(x => x.Animation).Inline();

			b.Group("Playback", GroupStyle.Title, g => g.Row(r =>
			{
				r.Button("Play", animator => animator.Play()).Size(ButtonSize.Large).PlayModeOnly().NoUndo()
				 .EnabledWhen(animator => TransformTweenInspectorTools.HasLocation(animator.Animation));
				r.Button("Jump", animator => TransformTweenInspectorTools.Jump(animator, animator.Animation)).Size(ButtonSize.Large).NoUndo()
				 .EnabledWhen(animator => TransformTweenInspectorTools.HasLocation(animator.Animation));
				r.Button("Stop", animator => animator.Stop()).Size(ButtonSize.Large).NoUndo();
			})).AtBottom();
		}
	}

	/// <summary>
	/// The inspector of <see cref="TwoWayTransformTweenAnimator"/>: both animations under their own titles and a
	/// row of playback buttons at the bottom.
	/// </summary>
	[CustomEditor(typeof(TwoWayTransformTweenAnimator))]
	[CanEditMultipleObjects]
	public sealed class TwoWayTransformTweenAnimatorInspector : ObjectInspector<TwoWayTransformTweenAnimator>
	{
		protected override void Configure(InspectorBuilder<TwoWayTransformTweenAnimator> b)
		{
			b.Group("Animation - To B", GroupStyle.Title, g => g.Fields(x => x.AnimationToB));
			b.Field(x => x.AnimationToB).Inline();
			b.Group("Animation - To A", GroupStyle.Title, g => g.Fields(x => x.AnimationToA));
			b.Field(x => x.AnimationToA).Inline();

			b.Group("Playback", GroupStyle.Title, g =>
			{
				g.Row(r =>
				{
					r.Button("Play to B", animator => animator.PlayToB()).Size(ButtonSize.Large).PlayModeOnly().NoUndo()
					 .EnabledWhen(animator => TransformTweenInspectorTools.HasLocation(animator.AnimationToB));
					r.Button("Jump to B", animator => TransformTweenInspectorTools.Jump(animator, animator.AnimationToB)).Size(ButtonSize.Large).NoUndo()
					 .EnabledWhen(animator => TransformTweenInspectorTools.HasLocation(animator.AnimationToB));
				});
				g.Row(r =>
				{
					r.Button("Play to A", animator => animator.PlayToA()).Size(ButtonSize.Large).PlayModeOnly().NoUndo()
					 .EnabledWhen(animator => TransformTweenInspectorTools.HasLocation(animator.AnimationToA));
					r.Button("Jump to A", animator => TransformTweenInspectorTools.Jump(animator, animator.AnimationToA)).Size(ButtonSize.Large).NoUndo()
					 .EnabledWhen(animator => TransformTweenInspectorTools.HasLocation(animator.AnimationToA));
				});
				g.Button("Stop", animator => animator.Stop()).Size(ButtonSize.Large).NoUndo();
			}).AtBottom();
		}
	}

	internal static class TransformTweenInspectorTools
	{
		internal static bool HasLocation(TransformTweenSetup setup)
		{
			return setup != null && setup.Location;
		}

		/// <summary>
		/// Puts the animated transform at the end of the animation at once, as one undo step on that transform.
		/// </summary>
		internal static void Jump(TransformTweenAnimatorBase animator, TransformTweenSetup setup)
		{
			if (!animator.Transform)
				throw new System.InvalidOperationException($"'{animator.name}' has no transform to animate.");
			if (setup == null || !setup.Location)
				throw new System.InvalidOperationException($"The animation of '{animator.name}' has no location to jump to.");
			Undo.RecordObject(animator.Transform, "Jump");
			animator.Jump(setup);
		}
	}

}
