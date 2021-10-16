using System;
using System.Collections.Generic;
using Extenity.DataToolbox;
using Extenity.FlowToolbox;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public struct FeedLine
	{
		public float ExpireTime;
		public FeedLineUI UI;
		//public string Content; No need to keep content data. But keep it commented out for future needs.

		public FeedLine(float expireTime, FeedLineUI ui/*, string content*/)
		{
			ExpireTime = expireTime;
			UI = ui;
			//Content = content;
		}
	}

	public class FeedUI : MonoBehaviour
	{
		#region Initialization

		protected void Awake()
		{
			InitializeLines();
			InitializeLineUIs();
			InitializeLinePositioning();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			DeinitializeLinePositioning();
		}

		#endregion

		#region Update

		protected void LateUpdate()
		{
			foreach (var line in Lines)
			{
				var position = line.UI.RectTransform.anchoredPosition;
				var diffY = line.UI.TargetPosition.y - position.y;
				if (diffY > 0.01f)
				{
					var t = Time.unscaledDeltaTime * LineMoveAnimationSpeed;
					if (t > 1f)
						t = 1f;
					position.y += diffY * t;
					line.UI.RectTransform.anchoredPosition = position;
				}
			}
		}

		#endregion

		#region Lines

		[Header("Lines")]
		public int LineCount = 10;
		public float LineDuration = 5f;
		public bool UnscaledLineDuration = false;

		[NonSerialized]
		public CircularArray<FeedLine> Lines;

		private void InitializeLines()
		{
			Lines = new CircularArray<FeedLine>(LineCount);
		}

		public void Append(string content)
		{
#if UNITY_EDITOR
			if (_AppendLineNumberPrefix)
			{
				content = ++_LastLineNumber + " | " + content;
			}
#endif

			if (Lines.Count == 0)
			{
				this.FastInvoke(TimeToDecay, LineDuration, UnscaledLineDuration);
			}
			else if (Lines.Count == Lines.Capacity)
			{
				RemoveTailingImmediately();
			}

			var lineUI = GetLineUIFromPool();
			lineUI.Text.SetText(content);
			Lines.Add(new FeedLine(Loop.Time + LineDuration, lineUI));
			InvalidateLinePositions();
		}

		private void RemoveTailingImmediately()
		{
			if (Lines.Count == 0)
				return;

			var uiToBeRemoved = Lines.TailingItem.UI;
			Lines.RemoveTailing();
			AddToPool(uiToBeRemoved);
			InvalidateLinePositions();
		}

		private void TimeToDecay()
		{
			var now = Loop.Time;

			while (Lines.Count > 0 && Lines.TailingItem.ExpireTime <= now + 0.01f)
			{
				var uiToBeRemoved = Lines.TailingItem.UI;
				Lines.RemoveTailing();
				AddToPool(uiToBeRemoved);
				InvalidateLinePositions();
			}

			if (Lines.Count > 0)
			{
				this.FastInvoke(TimeToDecay, Lines.TailingItem.ExpireTime - now, UnscaledLineDuration);
			}
		}

		#endregion

		#region Line UIs

		[Header("Line UI")]
		public FeedLineUI LineTemplate;

		private List<FeedLineUI> PooledLineUIs;

		private void InitializeLineUIs()
		{
			LineTemplate.gameObject.SetActive(false);

			PooledLineUIs = new List<FeedLineUI>(LineCount);
			for (int i = 0; i < LineCount; i++)
			{
				var lineUI = Instantiate(LineTemplate, LineTemplate.transform.parent);
#if UNITY_EDITOR
				lineUI.gameObject.name = "Line-" + i;
#endif
				PooledLineUIs.Add(lineUI);
			}
		}

		private FeedLineUI GetLineUIFromPool()
		{
			// By the time the code gets to this method, there must be at least one line should exist in the pool.
			Debug.Assert(PooledLineUIs.Count > 0);

			var index = PooledLineUIs.Count - 1;
			var ui = PooledLineUIs[index];
			PooledLineUIs.RemoveAt(index);
			ui.gameObject.SetActive(true);
			ui.RectTransform.anchoredPosition = new Vector2(0f, -100f);
			return ui;
		}

		private void AddToPool(FeedLineUI ui)
		{
			Debug.Assert(!PooledLineUIs.Contains(ui));

			ui.gameObject.SetActive(false);
			PooledLineUIs.Add(ui);
		}

		#endregion

		#region Line Positioning

		[Header("Line Positioning")]
		public bool InverseLines;
		public float LinePositionStepY = 20f;

		[Header("Animations")]
		public float LineAppearanceOffsetY = 20f;
		[Range(1f, 20f)]
		public float LineMoveAnimationSpeed = 10f;

		private bool IsLinePositionsInvalidated;

		private void InitializeLinePositioning()
		{
			Loop.RegisterLateUpdate(OnCustomLateUpdate);
		}

		private void DeinitializeLinePositioning()
		{
			Loop.DeregisterLateUpdate(OnCustomLateUpdate);
		}

		private void OnCustomLateUpdate()
		{
			if (IsLinePositionsInvalidated)
			{
				RecalculateLinePositions();
				IsLinePositionsInvalidated = false;
			}
		}

		private void InvalidateLinePositions()
		{
			IsLinePositionsInvalidated = true;
		}

		private void RecalculateLinePositions()
		{
			var stepY = LinePositionStepY;
			var positionY = InverseLines
				? -stepY * (Lines.Count - 1)
				: 0f;

			foreach (var line in Lines)
			{
				var ui = line.UI;

				ui.TargetPosition = new Vector2(0f, positionY);

				var justBeingCreated = ui.RectTransform.anchoredPosition.y < -10;
				if (justBeingCreated)
				{
					ui.RectTransform.anchoredPosition = ui.TargetPosition + new Vector2(0f, LineAppearanceOffsetY);
				}

				positionY += stepY;
			}
		}

		#endregion

		#region Debug

#if UNITY_EDITOR

		[Header("Debug")]
		[NonSerialized, ShowInInspector]
		public bool _AppendLineNumberPrefix;
		private int _LastLineNumber;

#endif

		#endregion
	}

}
