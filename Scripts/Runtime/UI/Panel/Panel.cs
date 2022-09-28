#if ExtenityScreenManagement

using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Extenity.DataToolbox;
using Extenity.FlowToolbox;
using Extenity.GameObjectToolbox;
using Sirenix.OdinInspector;
using UnityEngine;
using static Unity.Mathematics.math;

// TODO: Implement fade animation. See 118548391.

namespace Extenity.UIToolbox
{

	public enum PanelVisibility
	{
		Unspecified = 0,

		Invisible = 2,
		BecomingVisible = 3,
		Visible = 4,
		BecomingInvisible = 5,
	}

	[HideMonoScript]
	public class Panel : MonoBehaviour
	{
		#region Initialization

		private void Awake()
		{
			RegisterToParentPanel();
		}

		#endregion

		#region Deinitialization

		private void OnDestroy()
		{
			DeregisterFromParentPanel();
		}

		#endregion

		#region Animation Controls

		private float _AnimationLengthOfBecomingVisible
		{
			get
			{
				var max = 0f;
				if (EnableScaleInAnimation)
				{
					max = ScaleInDuration + ScaleInDelay;
				}
				// This is how it's going to be when implementing Fade or other types of animations. See 118548391.
				// if (EnableFadeInAnimation)
				// {
				// 	max = max(max, FadeInDuration + FadeInDelay);
				// }
				return max;
			}
		}

		private float _AnimationLengthOfBecomingInvisible
		{
			get
			{
				var max = 0f;
				if (EnableScaleOutAnimation)
				{
					max = ScaleOutDuration + ScaleOutDelay;
				}
				// This is how it's going to be when implementing Fade or other types of animations. See 118548391.
				// if (EnableFadeInAnimation)
				// {
				// 	max = max(max, FadeOutDuration + FadeOutDelay);
				// }
				return max;
			}
		}

		[ShowInInspector, ReadOnly, TitleGroup("Animation Controls")]
		public float AnimationLengthOfBecomingVisible
		{
			get
			{
				if (!enabled)
					return 0; // That makes the length to be counted as zero which helps parents to calculate delays correctly.

				return _AnimationLengthOfBecomingVisible;
			}
		}

		[ShowInInspector, ReadOnly, TitleGroup("Animation Controls")]
		public float AnimationLengthOfBecomingInvisible
		{
			get
			{
				if (!enabled)
					return 0; // That makes the length to be counted as zero which helps parents to calculate delays correctly.

				return _AnimationLengthOfBecomingInvisible;
			}
		}

		[ShowInInspector, ReadOnly, TitleGroup("Animation Controls")]
		public float AnimationLengthOfBecomingVisibleIncludingChildren
		{
			get
			{
				if (!enabled)
					return 0; // That makes the length to be counted as zero which helps parents to calculate delays correctly.

				var maxFound = _AnimationLengthOfBecomingVisible;

				// Also aggregate animation length of children
				for (var i = 0; i < Children.Count; i++)
				{
					if (Children[i])
					{
						maxFound = max(maxFound, Children[i].AnimationLengthOfBecomingVisibleIncludingChildren); // Recursive call
					}
				}

				return maxFound;
			}
		}

		[ShowInInspector, ReadOnly, TitleGroup("Animation Controls")]
		public float AnimationLengthOfBecomingInvisibleIncludingChildren
		{
			get
			{
				if (!enabled)
					return 0; // That makes the length to be counted as zero which helps parents to calculate delays correctly.

				var maxFound = _AnimationLengthOfBecomingInvisible;

				// Also aggregate animation length of children
				for (var i = 0; i < Children.Count; i++)
				{
					if (Children[i])
					{
						maxFound = max(maxFound, Children[i].AnimationLengthOfBecomingInvisibleIncludingChildren); // Recursive call
					}
				}

				return maxFound;
			}
		}

		[Button(ButtonSizes.Medium), HorizontalGroup("Animation Controls/Buttons")]
		[EnableIf("@UnityEngine.Application.isPlaying")]
		public void BecomeVisible()
		{
			if (!enabled)
				return; // Ignore this request if the component is not enabled.
			if (VisibilityStatus == PanelVisibility.Visible ||
			    VisibilityStatus == PanelVisibility.BecomingVisible)
				return; // Ignore consecutive requests of the same type.

			if (EnableVerboseLogging)
				LogVerbose("Panel becoming visible: " + gameObject.name);

			CancelProcessInvokes();
			ChangeVisibilityStatus(PanelVisibility.BecomingVisible); // Be extremely cautious about where to call these status changes. See 1175684391.
			ProcessBeforeBecomingVisibleAnimations();
			PlayScaleInAnimation();
			DelayOrCallProcessingAfterBecomingVisibleAnimations();

			// Trigger the children AFTER triggering this Panel to become VISIBLE.
			for (var i = 0; i < Children.Count; i++)
			{
				if (Children[i])
				{
					Children[i].BecomeVisible();
				}
			}
		}

		[Button(ButtonSizes.Medium), HorizontalGroup("Animation Controls/Buttons")]
		[EnableIf("@UnityEngine.Application.isPlaying")]
		public void BecomeInvisible()
		{
			if (!enabled)
				return; // Ignore this request if the component is not enabled.
			if (VisibilityStatus == PanelVisibility.Invisible ||
			    VisibilityStatus == PanelVisibility.BecomingInvisible)
				return; // Ignore consecutive requests of the same type.

			if (EnableVerboseLogging)
				LogVerbose("Panel becoming invisible: " + gameObject.name);

			// Trigger the children BEFORE triggering this Panel to become INVISIBLE.
			for (var i = 0; i < Children.Count; i++)
			{
				if (Children[i])
				{
					Children[i].BecomeInvisible();
				}
			}

			CancelProcessInvokes();
			ChangeVisibilityStatus(PanelVisibility.BecomingInvisible); // Be extremely cautious about where to call these status changes. See 1175684391.
			ProcessBeforeBecomingInvisibleAnimations();
			PlayScaleOutAnimation();
			DelayOrCallProcessingAfterBecomingInvisibleAnimations();
		}

		public void ChangeVisibility(bool visible)
		{
			if (visible)
			{
				BecomeVisible();
			}
			else
			{
				BecomeInvisible();
			}
		}

		#endregion

		#region Scale Animation General

		private void StopAllScaleAnimations()
		{
			StopScaleInAnimation();
			StopScaleOutAnimation();
		}

		#endregion

		#region Scale In Animation

		[Title("Becoming Visible Animation - Scale In")]
		public bool EnableScaleInAnimation = false;

		[ShowIf(nameof(EnableScaleInAnimation))]
		[ChildGameObjectsOnly]
		[Required]
		public RectTransform ScaleInTransform;

		[EnableIf(nameof(EnableScaleInAnimation))]
		public float ScaleInDuration = 0.25f;
		[EnableIf(nameof(EnableScaleInAnimation))]
		public float ScaleInDelay = 0f;
		[EnableIf(nameof(EnableScaleInAnimation))]
		public Ease ScaleInEase = Ease.OutBack;
		[EnableIf(nameof(EnableScaleInAnimation))]
		public bool ForceInitialScaleOfScaleInAnimation = true;
		[EnableIf(nameof(EnableScaleInAnimation))]
		[EnableIf(nameof(ForceInitialScaleOfScaleInAnimation))]
#if UNITY_EDITOR
		[InfoBox("Note that " + nameof(InitialScaleOfScaleInAnimation) + " and " + nameof(EndingScaleOfScaleOutAnimation) + " does not match, both of which define what scale should the panel squeezed into when became invisible. That's alright if the panel is instantly hidden after animations. But can become a problem if the panel stays visible as scaled out.",
		         InfoMessageType.Warning,
		         VisibleIf = nameof(NeedToShowUnmatchedScaledOutValueWarning))]
#endif
		public Vector3 InitialScaleOfScaleInAnimation = new Vector3(0, 0, 0);
		// [EnableIf(nameof(EnableScaleInAnimation))]
		// public Vector3 EndingScaleOfScaleInAnimation = new Vector3(1, 1, 1); Nope! All UI elements are enforced to have a scale of one. Otherwise Unity handles non-uniform scaling awfully.

		private TweenerCore<Vector3, Vector3, VectorOptions> ScaleInTween;

		private void PlayScaleInAnimation()
		{
			if (!EnableScaleInAnimation)
				return;

			StopAllScaleAnimations();

			if (ForceInitialScaleOfScaleInAnimation)
			{
				ScaleInTransform.localScale = InitialScaleOfScaleInAnimation;
			}

			ScaleInTween = DOTween.To(() => ScaleInTransform.localScale,
			                          value => ScaleInTransform.localScale = value,
			                          new Vector3(1, 1, 1),
			                          ScaleInDuration)
			                      .SetDelay(ScaleInDelay)
			                      .SetEase(ScaleInEase)
			                      .SetUpdate(UpdateType.Late, true);
		}

		private void StopScaleInAnimation()
		{
			if (ScaleInTween != null && ScaleInTween.IsActive() && ScaleInTween.IsPlaying())
			{
				ScaleInTween.Kill(false);
			}
		}

		#endregion

		#region Scale Out Animation

		[Title("Becoming Invisible Animation - Scale Out")]
		public bool EnableScaleOutAnimation = false;

		[ShowIf(nameof(EnableScaleOutAnimation))]
		[ChildGameObjectsOnly]
		[Required]
		public RectTransform ScaleOutTransform;

		[EnableIf(nameof(EnableScaleOutAnimation))]
		public float ScaleOutDuration = 0.25f;
		[EnableIf(nameof(EnableScaleOutAnimation))]
		public float ScaleOutDelay = 0f;
		[EnableIf(nameof(EnableScaleOutAnimation))]
		public Ease ScaleOutEase = Ease.OutCubic;
		[EnableIf(nameof(EnableScaleOutAnimation))]
		public bool ForceInitialScaleOfScaleOutAnimation = false;
		[EnableIf(nameof(EnableScaleOutAnimation))]
		[EnableIf(nameof(ForceInitialScaleOfScaleOutAnimation))]
		public Vector3 InitialScaleOfScaleOutAnimation = new Vector3(1, 1, 1);
		[EnableIf(nameof(EnableScaleOutAnimation))]
		public Vector3 EndingScaleOfScaleOutAnimation = new Vector3(0, 0, 0);

		private TweenerCore<Vector3, Vector3, VectorOptions> ScaleOutTween;

		private void PlayScaleOutAnimation()
		{
			if (!EnableScaleOutAnimation)
				return;

			StopAllScaleAnimations();

			if (ForceInitialScaleOfScaleOutAnimation)
			{
				ScaleOutTransform.localScale = InitialScaleOfScaleOutAnimation;
			}

			ScaleOutTween = DOTween.To(() => ScaleOutTransform.localScale,
			                           value => ScaleOutTransform.localScale = value,
			                           EndingScaleOfScaleOutAnimation,
			                           ScaleOutDuration)
			                       .SetDelay(ScaleOutDelay)
			                       .SetEase(ScaleOutEase)
			                       .SetUpdate(UpdateType.Late, true);
		}

		private void StopScaleOutAnimation()
		{
			if (ScaleOutTween != null && ScaleOutTween.IsActive() && ScaleOutTween.IsPlaying())
			{
				ScaleOutTween.Kill(false);
			}
		}

		#endregion

		#region Visibility Operations

		private void DelayOrCallProcessingAfterBecomingInvisibleAnimations()
		{
			var delay = AnimationLengthOfBecomingInvisibleIncludingChildren;
			if (delay > 0f)
			{
				this.FastInvoke(ProcessAfterBecomingInvisibleAnimations, delay, true);
			}
			else
			{
				ProcessAfterBecomingInvisibleAnimations();
			}
		}

		private void DelayOrCallProcessingAfterBecomingVisibleAnimations()
		{
			var delay = AnimationLengthOfBecomingVisibleIncludingChildren;
			if (delay > 0f)
			{
				this.FastInvoke(ProcessAfterBecomingVisibleAnimations, delay, true);
			}
			else
			{
				ProcessAfterBecomingVisibleAnimations();
			}
		}

		private void CancelProcessInvokes()
		{
			this.CancelFastInvoke(ProcessAfterBecomingVisibleAnimations);
			this.CancelFastInvoke(ProcessAfterBecomingInvisibleAnimations);
		}

		private void ProcessBeforeBecomingInvisibleAnimations()
		{
			ManageObjectsBeforeBecomingInvisibleAnimationStarts();
		}

		private void ProcessBeforeBecomingVisibleAnimations()
		{
			ManageObjectsBeforeBecomingVisibleAnimationStarts();
		}

		private void ProcessAfterBecomingVisibleAnimations()
		{
			CancelProcessInvokes();
			ManageObjectsAfterBecomingVisibleAnimationFinishes();
			ChangeVisibilityStatus(PanelVisibility.Visible); // Be extremely cautious about where to call these status changes. See 1175684391.
		}

		private void ProcessAfterBecomingInvisibleAnimations()
		{
			CancelProcessInvokes();
			ManageObjectsAfterBecomingInvisibleAnimationFinishes();
			ChangeVisibilityStatus(PanelVisibility.Invisible); // Be extremely cautious about where to call these status changes. See 1175684391.
		}

		#endregion

		#region Auto Managed Objects

		[TitleGroup("Auto Managed Objects")]
		[Tooltip("The linked " + nameof(Canvas) + " will also be enabled/disabled when the panel changes visibility. It will be enabled BEFORE becoming visible animations STARTS playing and disabled AFTER the becoming invisible animations FINISHES playing.")]
		public Canvas AutoEnabledCanvas;

		[TitleGroup("Auto Managed Objects")]
		[Tooltip("The '" + nameof(CanvasGroup.interactable) + "' property of linked " + nameof(CanvasGroup) + " will also be enabled/disabled when the panel changes visibility. It will be enabled AFTER becoming visible animations FINISHES playing and disabled BEFORE the becoming invisible animations STARTS playing.")]
		public CanvasGroup AutoEnabledCanvasGroup;

		private void ManageObjectsBeforeBecomingVisibleAnimationStarts()
		{
			if (AutoEnabledCanvas)
			{
				AutoEnabledCanvas.enabled = true;
			}
			if (AutoEnabledCanvasGroup)
			{
				AutoEnabledCanvasGroup.blocksRaycasts = true;
			}
		}

		private void ManageObjectsAfterBecomingInvisibleAnimationFinishes()
		{
			if (AutoEnabledCanvas)
			{
				AutoEnabledCanvas.enabled = false;
			}
			if (AutoEnabledCanvasGroup)
			{
				AutoEnabledCanvasGroup.blocksRaycasts = false;
			}
		}

		private void ManageObjectsAfterBecomingVisibleAnimationFinishes()
		{
			if (AutoEnabledCanvasGroup)
			{
				AutoEnabledCanvasGroup.interactable = true;
				AutoEnabledCanvasGroup.blocksRaycasts = true; // Not required but ensures consistency.
			}
		}

		private void ManageObjectsBeforeBecomingInvisibleAnimationStarts()
		{
			if (AutoEnabledCanvasGroup)
			{
				AutoEnabledCanvasGroup.interactable = false;
				AutoEnabledCanvasGroup.blocksRaycasts = true; // Not required but ensures consistency.
			}
		}

		#endregion

		#region Children Panels

		[Title("Parenting")]
		public bool DoNotLinkToParent = false;

		/// <remarks>
		/// Use it as Readonly.
		/// </remarks>
		[NonSerialized]
		[ShowInInspector, ReadOnly]
		public Panel Parent;

		/// <remarks>
		/// Use it as Readonly.
		/// </remarks>
		[NonSerialized]
		[ShowInInspector, ReadOnly]
		public readonly List<Panel> Children = new List<Panel>();

		private void RegisterToParentPanel()
		{
			if (!DoNotLinkToParent)
			{
				Parent = gameObject.GetComponentInParent<Panel>(false, true);
				if (Parent)
				{
					Parent.Register(this);
				}
			}
		}

		private void DeregisterFromParentPanel()
		{
			if (Parent)
			{
				Parent.Deregister(this);
			}
		}

		/// <remarks>
		/// It's safe to register the component more than once.
		/// </remarks>
		private void Register(Panel component)
		{
			if (!component)
				return;

			if (EnableVerboseLogging)
				LogVerbose($"Registering {nameof(Panel)}: " + component.gameObject.name);

			// Removing nulls here will ensure proper memory management that prevents the list to get filled with destroyed object links.
			Children.AddUniqueNullCheckedAndRemoveNulls(component);
		}

		/// <remarks>
		/// It's safe to deregister the component more than once.
		/// </remarks>
		private void Deregister(Panel component)
		{
			if (!component)
				return;

			if (EnableVerboseLogging)
				LogVerbose($"Deregistering {nameof(Panel)}: " + component.gameObject.name);

			Children.Remove(component);
		}

		#endregion

		#region Registered PanelMonoBehaviours

		[NonSerialized]
		private List<PanelMonoBehaviour> RegisteredPanelMonoBehaviours;

		public void Register(PanelMonoBehaviour panel)
		{
			if (RegisteredPanelMonoBehaviours == null)
			{
				RegisteredPanelMonoBehaviours = new List<PanelMonoBehaviour>();
			}

			RegisteredPanelMonoBehaviours.AddUniqueNullCheckedAndRemoveNulls(panel);
		}

		public void Deregister(PanelMonoBehaviour panel)
		{
			if (RegisteredPanelMonoBehaviours != null)
			{
				RegisteredPanelMonoBehaviours.RemoveNullChecked(panel);
			}
		}

		#endregion

		#region Visibility

		[NonSerialized]
		[ShowInInspector, ReadOnly]
		[TitleGroup("Status")]
		public PanelVisibility VisibilityStatus;

		[TitleGroup("Status")]
		[ShowInInspector, ReadOnly]
		public bool IsVisible => VisibilityStatus == PanelVisibility.Visible;
		[TitleGroup("Status")]
		[ShowInInspector, ReadOnly]
		public bool IsVisibleOrBecomingVisible => VisibilityStatus == PanelVisibility.Visible || VisibilityStatus == PanelVisibility.BecomingVisible;
		[TitleGroup("Status")]
		[ShowInInspector, ReadOnly]
		public bool IsInvisibleOrBecomingInvisible => VisibilityStatus == PanelVisibility.Invisible || VisibilityStatus == PanelVisibility.BecomingInvisible;

		private void ChangeVisibilityStatus(PanelVisibility newStatus)
		{
			VisibilityStatus = newStatus;
			InvokeVisibilityStatusChangeEvents();
		}

		private void InvokeVisibilityStatusChangeEvents()
		{
			if (RegisteredPanelMonoBehaviours == null || RegisteredPanelMonoBehaviours.Count == 0)
				return; // No one to invoke.

			// Copy the current state to prevents any side effects if the status is modified in the middle of the process.
			var visibilityStatus = VisibilityStatus;

			// Copy the list just before invoking the callbacks to prevent side effects of modifying the list in the middle
			// of the process.
			var panels = New.List<PanelMonoBehaviour>();
			RegisteredPanelMonoBehaviours.CopyTo(panels);

			for (var i = 0; i < panels.Count; i++)
			{
				var panel = panels[i];

				try // Every callback is called inside its own catch block to ensure all panels to be processed.
				{
					switch (visibilityStatus)
					{
						case PanelVisibility.Unspecified:
							break;

						case PanelVisibility.Invisible:
							panel.OnAfterBecameInvisible();
							break;

						case PanelVisibility.BecomingVisible:
							panel.OnBeforeBecomingVisible();
							break;

						case PanelVisibility.Visible:
							panel.OnAfterBecameVisible();
							break;

						case PanelVisibility.BecomingInvisible:
							panel.OnBeforeBecomingInvisible();
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception exception)
				{
					Log.Exception(exception);
				}
			}

			Release.ListUnsafe(panels);
		}

		#endregion

		#region Log

		[Title("Log")]
		public bool EnableVerboseLogging = false;

		private void LogVerbose(string message)
		{
			Log.Verbose(message, this);
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		private bool NeedToShowUnmatchedScaledOutValueWarning =>
			EnableScaleInAnimation &&
			EnableScaleOutAnimation &&
			InitialScaleOfScaleInAnimation != EndingScaleOfScaleOutAnimation;

		private void OnValidate()
		{
			EnsurePanelScriptsDontHaveOnEnabledOrOnDisabled();
		}

#endif

		#endregion

		#region Editor - Ensure

#if UNITY_EDITOR

		private void EnsurePanelScriptsDontHaveOnEnabledOrOnDisabled()
		{
			var list = New.List<MonoBehaviour>();
			gameObject.GetComponentsInChildren(true, list);
			foreach (var item in list)
			{
				if (!item)
					continue; // Unity might give us null components for missing or not-yet-compiled scripts.
				var type = item.GetType();
				var fullName = type.FullName;

				// Excluded types
				if (type == typeof(Panel) ||
				    type == typeof(ClickArea) ||
				    type == typeof(CanvasRegistrar) ||
				    fullName.StartsWith("TMPro.", StringComparison.OrdinalIgnoreCase) ||
				    fullName.StartsWith("UnityEngine.UI", StringComparison.OrdinalIgnoreCase) ||
				    fullName.Equals("JoshH.UI.UIGradient", StringComparison.OrdinalIgnoreCase)
				)
				{
					continue;
				}

				var isOnEnableImplemented = type.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance) != null;
				var isOnDisableImplemented = type.GetMethod("OnDisable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.Instance) != null;
				if (isOnEnableImplemented || isOnDisableImplemented)
				{
					Log.CriticalError($"'OnEnable' and 'OnDisable' is not allowed in script '{type}'. Use {nameof(Panel)} visibility callbacks instead, by deriving from '{nameof(PanelMonoBehaviour)}'.", gameObject);
				}
			}
			Release.ListUnsafe(list);
		}

#endif

		#endregion
	}

}

#endif
