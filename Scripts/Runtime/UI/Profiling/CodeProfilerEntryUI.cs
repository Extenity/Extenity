using System;
using Extenity.MathToolbox;
using Extenity.ProfilingToolbox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Extenity.UIToolbox
{

	public class CodeProfilerEntryUI : TreeViewItem<CodeProfilerEntry>
	{
		#region Initialization

		public override void OnItemCreated(TreeView<CodeProfilerEntry>.Node node)
		{
			Node = node;

			if (Node.IsRoot)
			{
				SetLabel("Root");
			}
			else
			{
				var label = CodeProfiler.GetLabelOrID(Node.Data.ID);
				SetLabel(label);
				RefreshValues();
			}

			IndentationPanel.offsetMin = new Vector2(node.Indentation * Indentation, IndentationPanel.offsetMin.y);

			NormalColor = Background.color;
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Data

		[NonSerialized]
		public TreeView<CodeProfilerEntry>.Node Node;

		#endregion

		#region Common Elements

		[Header("Common Elements")]
		public Image Background;
		public RectTransform IndentationPanel;
		public RectTransform ExpandIcon;
		public int Indentation = 5;
		public Color SelectedColor;
		private Color NormalColor;
		
		#endregion

		#region Profiler Elements

		[Header("Profiler Elements")]
		public TextMeshProUGUI LabelText;
		public TextMeshProUGUI LastDurationText;
		public TextMeshProUGUI AverageDurationText;
		public TextMeshProUGUI TotalCountText;

		public void SetLabel(string label)
		{
			LabelText.text = label;
		}

		public void SetValues(float lastDuration, float averageDuration, int totalCount)
		{
			LastDurationText.SetCharArrayForValue("N3", lastDuration.ConvertSecondsToMilliseconds());
			AverageDurationText.SetCharArrayForValue("N3", averageDuration.ConvertSecondsToMilliseconds());
			TotalCountText.SetCharArrayForInt(totalCount);
		}

		public void RefreshValues()
		{
			SetValues((float)Node.Data.LastDuration, Node.Data.RunningAverageDuration.Mean, Node.Data.TotalCount);
		}

		#endregion

		#region Selection

		public override void OnItemSelected()
		{
			Background.color = SelectedColor;
		}

		public override void OnItemDeselected()
		{
			Background.color = NormalColor;
		}

		#endregion

		#region Expand / Collapse

		public override void OnLeafStateChanged(bool isLeaf)
		{
			ExpandIcon.gameObject.SetActive(!isLeaf);
		}

		public override void OnItemExpanded()
		{
			ExpandIcon.localEulerAngles = new Vector3(0f, 0f, 0f);
		}

		public override void OnItemCollapsed()
		{
			ExpandIcon.localEulerAngles = new Vector3(0f, 0f, -90f);
		}

		#endregion
	}

}
