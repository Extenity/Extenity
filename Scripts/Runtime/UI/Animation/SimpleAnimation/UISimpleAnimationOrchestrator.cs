using System;
using Extenity.DataToolbox;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class UISimpleAnimationOrchestrator : MonoBehaviour
	{
		#region Initialization

		public enum InitialAnimationState
		{
			Untouched,
			PlaceToA,
			PlaceToB,
		}

		protected void Start()
		{
			switch (InitialState)
			{
				case InitialAnimationState.Untouched:
					break;
				case InitialAnimationState.PlaceToA:
					AnimateImmediateToA();
					break;
				case InitialAnimationState.PlaceToB:
					AnimateImmediateToB();
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(InitialState), (int)InitialState, "");
			}
		}

		#endregion

		#region Animation

		[Serializable]
		public struct Entry
		{
			public UISimpleAnimation Animation;
			public bool Inverted;

			public Entry(UISimpleAnimation animation, bool inverted)
				: this()
			{
				Animation = animation;
				Inverted = inverted;
			}

			public float AnimateToA(bool immediate)
			{
				if (!Animation)
					return 0f;

				if (Inverted)
					return Animation.AnimateToB(immediate);
				else
					return Animation.AnimateToA(immediate);
			}

			public float AnimateToB(bool immediate)
			{
				if (!Animation)
					return 0f;

				if (Inverted)
					return Animation.AnimateToA(immediate);
				else
					return Animation.AnimateToB(immediate);
			}
		}

		[Header("Animation")]
		public InitialAnimationState InitialState = InitialAnimationState.Untouched;
		public Entry[] Animations;

		//public class AnimationEvent : UnityEvent<UISimpleAnimationOrchestrator> { }

		//[NonSerialized]
		//public AnimationEvent OnAnimationStarted = new AnimationEvent();
		//[NonSerialized]
		//public AnimationEvent OnAnimationFinished = new AnimationEvent();

		/// ----------------------------------------------------------------------------------------

		public float AnimateToA(bool immediate)
		{
			if (Animations.IsNullOrEmpty())
			{
				if (DEBUG_ShowAnimationMessages)
				{
					Log.Warning($"Ignored animation request because no animation specified in animation orchestrator of '{gameObject.FullName()}'.", gameObject);
				}
				return 0f;
			}

			if (DEBUG_ShowAnimationMessages)
			{
				Log.Info("Animating 'To A'.", gameObject);
			}

			var maxLength = 0f;
			for (var i = 0; i < Animations.Length; i++)
			{
				var length = Animations[i].AnimateToA(immediate);
				if (maxLength < length)
					maxLength = length;
			}
			return maxLength;
		}

		public float AnimateImmediateToA()
		{
			return AnimateToA(true);
		}

		public float AnimateToA()
		{
			return AnimateToA(false);
		}

		/// ----------------------------------------------------------------------------------------

		public float AnimateToB(bool immediate)
		{
			if (Animations.IsNullOrEmpty())
			{
				if (DEBUG_ShowAnimationMessages)
				{
					Log.Warning($"Ignored animation request because no animation specified in animation orchestrator of '{gameObject.FullName()}'.", gameObject);
				}
				return 0f;
			}

			if (DEBUG_ShowAnimationMessages)
			{
				Log.Info("Animating 'To B'.", gameObject);
			}

			var maxLength = 0f;
			for (var i = 0; i < Animations.Length; i++)
			{
				var length = Animations[i].AnimateToB(immediate);
				if (maxLength < length)
					maxLength = length;
			}
			return maxLength;
		}

		public float AnimateImmediateToB()
		{
			return AnimateToB(true);
		}

		public float AnimateToB()
		{
			return AnimateToB(false);
		}

		#endregion

		#region Debug

		[Header("Debug")]
		public bool DEBUG_ShowAnimationMessages = false;

		#endregion
	}

}
