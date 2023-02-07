using System;
using Extenity.MathToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox.TouchInput
{

	[HideMonoScript]
	public abstract class ElementBase<TSchemeElement> : MonoBehaviour
		where TSchemeElement : SchemeElementBase
	{
		#region Initialization

		protected virtual void Start()
		{
			InitializeSchemeElements();

			Loop.RegisterUpdate(CustomUpdate, -1000);
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroy()
		{
			Loop.DeregisterUpdate(CustomUpdate);

			DeinitializeSchemeElements();
		}

		#endregion

		#region Update

		protected virtual void CustomUpdate()
		{
			if (IsAnimatingMovement)
			{
				CalculateMovementAnimation();
			}
		}

		#endregion

		#region Links

		[BoxGroup("Links"), PropertyOrder(8)]
		public RectTransform Base = default;

		#endregion

		#region Scheme Elements

		protected abstract void ApplySchemeElement(TSchemeElement schemeElement);

		public TSchemeElement ActiveSchemeElement { get; private set; }

		[BoxGroup("Links"), PropertyOrder(20)]
		[ListDrawerSettings(Expanded = true)]
		public TSchemeElement[] SchemeElements;

		private void InitializeSchemeElements()
		{
			TouchInputManager.OnInputSchemeChanged.AddListener(SetupForInputScheme);
			if (TouchInputManager.IsInstanceAvailable)
			{
				SetupForInputScheme(TouchInputManager.Instance.CurrentScheme);
			}
		}

		private void DeinitializeSchemeElements()
		{
			if (TouchInputManager.IsInstanceAvailable)
			{
				TouchInputManager.OnInputSchemeChanged.RemoveListener(SetupForInputScheme);
			}
		}

		private void SetupForInputScheme(string scheme)
		{
			if (SchemeElements == null || SchemeElements.Length == 0)
				return;

			// 'Null' scheme will be applied if none found with the specified name.
			ActiveSchemeElement = null;

			foreach (var schemeElement in SchemeElements)
			{
				if (schemeElement.SchemeName.Equals(scheme, StringComparison.OrdinalIgnoreCase))
				{
					ActiveSchemeElement = schemeElement;
					// schemeElement.ChangeActivation(true); Nope! This will be done after every other one is disabled. See below.
				}
				else
				{
					schemeElement.ChangeActivation(false);
				}
			}

			if (ActiveSchemeElement != null)
			{
				ActiveSchemeElement.ChangeActivation(true);
			}

			// Log.Info(ActiveSchemeElement != null
			// 	         ? $"Changing scheme of element '{gameObject.FullName()}' to '{ActiveSchemeElement.FullGameObjectName()}'"
			// 	         : $"Resetting scheme of element '{gameObject.FullName()}'.");
			ApplySchemeElement(ActiveSchemeElement);
		}

		#endregion

		#region Show / Hide UI

		[BoxGroup("Links"), PropertyOrder(15)]
		public UIFader Fader;

		#endregion

		#region Movement with Animation

		private bool IsAnimatingMovement => MovementAnimationRemainingTime > 0f;
		private float MovementAnimationDuration;
		private float MovementAnimationRemainingTime;
		private Vector2 MovementAnimationStartPosition;
		private Vector2 MovementAnimationEndPosition;

		private void CalculateMovementAnimation()
		{
			MovementAnimationRemainingTime -= Loop.DeltaTime;
			if (MovementAnimationRemainingTime <= 0f)
			{
				Base.anchoredPosition = MovementAnimationEndPosition;
				StopMovementAnimation();
			}
			else
			{
				var t = MovementAnimationRemainingTime / MovementAnimationDuration;
				Base.anchoredPosition = Vector2.Lerp(MovementAnimationStartPosition, MovementAnimationEndPosition, 1f - t * t);
			}
		}

		public void MoveTo(RectTransform targetLocation, float animationDuration)
		{
			if (!targetLocation)
			{
				return; // Just ignore the movement request when target location is not set.
			}
			var screenPosition = targetLocation.TransformPoint(Vector3.zero);
			MoveTo(screenPosition, animationDuration);
		}

		public void MoveTo(Vector2 screenPosition, float animationDuration)
		{
			// Not sure if it's the best place to setup anchors. But we will leave it for now. Feel free to investigate.
			Base.anchorMin = new Vector2(0.5f, 0.5f);
			Base.anchorMax = new Vector2(0.5f, 0.5f);

			var position = Base.parent.InverseTransformPoint(screenPosition);

			if (animationDuration > 0f)
			{
				MovementAnimationRemainingTime = animationDuration;
				MovementAnimationDuration = animationDuration;
				MovementAnimationStartPosition = Base.anchoredPosition;
				MovementAnimationEndPosition = position;
			}
			else
			{
				StopMovementAnimation(); // Stop if currently animating.
				Base.anchoredPosition = position;
			}
		}

		public void StopMovementAnimation()
		{
			MovementAnimationRemainingTime = 0f;
			MovementAnimationDuration = 0f;
			MovementAnimationStartPosition = Vector2Tools.Zero;
			MovementAnimationEndPosition = Vector2Tools.Zero;
		}

		#endregion
	}

}
