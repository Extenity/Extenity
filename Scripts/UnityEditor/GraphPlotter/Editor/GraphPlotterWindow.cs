using System.Collections.Generic;
using Extenity.TextureToolbox;
using UnityEditor;
using UnityEngine;

namespace Extenity.UnityEditorToolbox.GraphPlotting.Editor
{

	public class GraphPlotterWindow : EditorWindow
	{
		#region Configuration

		private static class EditorSettings
		{
			public const string TimeWindow = "GraphPlotter.TimeWindow";
			public const string InterpolationType = "GraphPlotter.InterpolationType";
			public const string GraphHeight = "GraphPlotter.GraphHeight";
			public const string LegendWidth = "GraphPlotter.LegendWidth";
		}

		private string[] InterpolationTypes = { "Linear", "Flat" };

		//private const int DefaultWindowWidth = 1200;
		//private const int DefaultWindowHeight = 800;

		// Graph sizing
		private const float SpaceAboveGraph = 30f;
		private const float SpaceBelowGraph = 20f;
		private const int MinimumGraphHeight = 100;
		private const int MaximumGraphHeight = 500;

		// Sub-second lines
		private const float SubSecondLinesInterval = 0.1f;
		private const float TimeWindowForSubSecondLinesToAppear = 3f;
		private const float TimeWindowForSubSecondLinesToGetFullyOpaque = 2f;

		// Colors and Style
		private const float DeselectedChannelAlpha = 0.02f;
		private Color backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);
		private Color legendBackgroundColor;
		private Color legendBackgroundColor_Free = new Color(0.87f, 0.87f, 0.87f);
		private Color legendBackgroundColor_Pro = new Color(0.25f, 0.25f, 0.25f);
		private Color settingsHeaderBackgroundColor;
		private Color settingsHeaderBackgroundColor_Free = new Color(0.87f, 0.87f, 0.87f);
		private Color settingsHeaderBackgroundColor_Pro = new Color(0.22f, 0.22f, 0.22f);
		private Color legendTextColorSelected;
		private Color legendTextColorSelected_Free = new Color(0f, 0f, 0f, 1f);
		private Color legendTextColorSelected_Pro = new Color(1f, 1f, 1f, 1f);
		private Color legendTextColorUnselected;
		private Color legendTextColorUnselected_Free = new Color(0f, 0f, 0f, 0.5f);
		private Color legendTextColorUnselected_Pro = new Color(1f, 1f, 1f, 0.5f);
		private Color channelHeaderColor;
		private Color channelHeaderColor_Free = new Color(0.2f, 0.2f, 0.2f);
		private Color channelHeaderColor_Pro = new Color(0.5f, 0.5f, 0.5f);
		private Color headerColor = new Color(0.7f, 0.7f, 0.7f);
		private Color minMaxColor = new Color(1f, 1f, 1f, 0.2f);
		private Color timeColor;
		private Color timeColor_Free = new Color(0f, 0f, 0f, 0.5f);
		private Color timeColor_Pro = new Color(1f, 1f, 1f, 0.5f);
		private Color zeroLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
		private Color timeLineColor = new Color(1f, 1f, 1f, 0.05f);
		private Color SecondLinesColor = new Color(1f, 1f, 1f, 0.05f);
		private Color SubSecondLinesColor = new Color(1f, 1f, 1f, 0.04f);
		private Color32[] TitleBarGradientShadowTextureColors =
		{
			new Color(0.1f, 0.1f, 0.1f, 0.3f),
			new Color(0.12f, 0.12f, 0.12f, 0f)
		};
		private Color32[] GraphAreaGradientShadowTextureColors =
		{
			new Color(0.1f, 0.1f, 0.1f, 0.7f),
			new Color(0.12f, 0.12f, 0.12f, 0f)
		};

		private GUIStyle headerStyle;
		private GUIStyle minStyle;
		private GUIStyle maxStyle;
		private GUIStyle timeIntervalSelectionStyle;
		private GUIStyle timeStyle;
		private GUIStyle valueTextStyle;
		private GUIStyle simpleStyle;

		private void CreateStyles()
		{
			if (EditorGUIUtility.isProSkin)
			{
				legendBackgroundColor = legendBackgroundColor_Pro;
				settingsHeaderBackgroundColor = settingsHeaderBackgroundColor_Pro;
				legendTextColorSelected = legendTextColorSelected_Pro;
				legendTextColorUnselected = legendTextColorUnselected_Pro;
				channelHeaderColor = channelHeaderColor_Pro;
				timeColor = timeColor_Pro;
			}
			else
			{
				legendBackgroundColor = legendBackgroundColor_Free;
				settingsHeaderBackgroundColor = settingsHeaderBackgroundColor_Free;
				legendTextColorSelected = legendTextColorSelected_Free;
				legendTextColorUnselected = legendTextColorUnselected_Free;
				channelHeaderColor = channelHeaderColor_Free;
				timeColor = timeColor_Free;
			}

			headerStyle = new GUIStyle();
			headerStyle.normal.textColor = headerColor;

			maxStyle = new GUIStyle();
			maxStyle.normal.textColor = minMaxColor;
			maxStyle.alignment = TextAnchor.LowerRight;

			minStyle = new GUIStyle();
			minStyle.normal.textColor = minMaxColor;
			minStyle.alignment = TextAnchor.UpperRight;

			//timeWindowStyle = new GUIStyle();
			//timeWindowStyle.normal.textColor = Color.grey;
			//timeWindowStyle.alignment = TextAnchor.MiddleRight;

			valueTextStyle = new GUIStyle();

			timeIntervalSelectionStyle = new GUIStyle();
			timeIntervalSelectionStyle.clipping = TextClipping.Overflow;
			timeIntervalSelectionStyle.alignment = TextAnchor.MiddleCenter;
			timeIntervalSelectionStyle.normal.textColor = Color.white;

			timeStyle = new GUIStyle();
			timeStyle.normal.textColor = timeColor;

			simpleStyle = new GUIStyle();
			simpleStyle.normal.textColor = Color.white;
		}

		#endregion

		private float scrollPositionY = 0f;
		private float scrollPositionTime = 0f;
		private float scrollPositionTimeMax = 0f;

		private float totalGraphHeight;
		private float legendTextOffset = 10f;
		private float extraScrollSpace = 30f;

		private Channel selectedChannel;

		// Graph height resizing
		private bool IsResizingGraphHeight = false;
		private int GraphHeightBeforeResizing;
		private float GraphHeightResizeDelta = 0f;
		private float MouseYPositionBeforeResizingGraphHeight;
		private int HeightResizingGraphIndex;

		private bool legendResize = false;

		private bool wasInPauseMode = false;

		private Graph timeIntervalSelectionGraph = null;
		private float timeIntervalStartTime;
		private float timeIntervalEndTime;

		// Saved editor settings.
		private float timeWindow;
		private int interpolationTypeIndex;
		private int graphHeight;
		private int legendWidth;

		private Vector3[] points = new Vector3[2000];
		private Vector3[] arrowPoints = new Vector3[4];
		private Vector3[] diamondPoints = new Vector3[5];
		private Vector3[] horizontalLines = new Vector3[7];

		private readonly List<TagEntry> TagEntries = new List<TagEntry>(100);

		#region Initialization

		[MenuItem("Window/Graph Plotter _#%g")]
		private static void CreateWindow()
		{
			GetWindow<GraphPlotterWindow>();
		}

		protected void Awake()
		{
			titleContent = new GUIContent("Graph Plotter");
			wantsMouseMove = true;
		}

		protected void OnEnable()
		{
			// Load settings
			timeWindow = EditorPrefs.GetFloat(EditorSettings.TimeWindow, 0.69897000433f);
			interpolationTypeIndex = EditorPrefs.GetInt(EditorSettings.InterpolationType, 0);
			graphHeight = EditorPrefs.GetInt(EditorSettings.GraphHeight, 140);
			legendWidth = EditorPrefs.GetInt(EditorSettings.LegendWidth, 170);

			CreateStyles();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			DestroyTextures();
		}

		#endregion

		#region GUI

		protected void OnGUI()
		{
			// Make sure content color is sane.
			GUI.contentColor = Color.white;

			CreateTexturesIfRequired();

			// calculate dynamic sizes.
			var width = position.width;
			var height = position.height;
			var graphWidth = width - legendWidth - 5f;

			totalGraphHeight = graphHeight - SpaceAboveGraph - SpaceBelowGraph;

			// settings header (prelude)
			var settingsRect = new Rect(0f, 0f, position.width, 25f);

			// draw background.
			GUI.color = backgroundColor;
			GUI.DrawTexture(new Rect(0, settingsRect.height, width, height - settingsRect.height), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);

			GUI.color = legendBackgroundColor;
			GUI.DrawTexture(new Rect(0f, settingsRect.height, legendWidth, height - settingsRect.height), EditorGUIUtility.whiteTexture);

			var currentEventType = Event.current.type;
			var mousePosition = Event.current.mousePosition;

			mousePosition.x = Mathf.Min(mousePosition.x, width);

			var isInPauseMode = EditorApplication.isPlaying && EditorApplication.isPaused;

			if (!isInPauseMode)
			{
				// smooth catch up. Nice!
				scrollPositionTime = 0f;
			}

			if (currentEventType == EventType.MouseDown &&
				mousePosition.x > legendWidth &&
				mousePosition.x < (width - 14f) &&
				mousePosition.y > settingsRect.height)
			{
				Debug.Break();
			}

			Channel newSelectedChannel = null;

			GatherContextFilteredGraphs(currentEventType);

			for (int i = 0; i < FilteredGraphs.Count; i++)
			{
				var graph = FilteredGraphs[i];
				var range = graph.Range;

				var graphAreaRect = new Rect(legendWidth, i * graphHeight + settingsRect.height - scrollPositionY, graphWidth, graphHeight);
				var graphRect = new Rect(graphAreaRect.xMin, graphAreaRect.yMin + SpaceAboveGraph, graphAreaRect.width - 20, totalGraphHeight - 5);
				//GUITools.DrawRect(graphAreaRect, Color.red, 2f);
				//GUITools.DrawRect(graphRect, Color.blue, 2f);

				var span = range.Span;

				GUI.color = Color.white;
				GUI.Label(new Rect(legendWidth + 10f, graphAreaRect.yMin + 10, 100f, 30f), graph.Title, headerStyle);

				var timeEnd = LatestTimeOfFilteredGraphs + scrollPositionTime;
				var timeStart = LatestTimeOfFilteredGraphs - timeWindow + scrollPositionTime;

				if (range.Sizing == ValueAxisSizing.Adaptive)
				{
					graph.CalculateValueAxisRangeInTimeWindow(timeStart, timeEnd);
				}

				if (range.Min < float.PositiveInfinity)
					GUI.Label(new Rect(graphRect.xMax - 200f - 5f, graphRect.yMax + 5f, 200f, 20f), range.Min.ToString(), minStyle);

				if (range.Max > float.NegativeInfinity)
					GUI.Label(new Rect(graphRect.xMax - 200f - 5f, graphRect.yMin - 5f - 20f, 200f, 20f), range.Max.ToString(), maxStyle);

				// Graph resizing
				{
					var resizeRect = new Rect(0f, graphAreaRect.yMax - 10, width - 12, 21);
					if (!legendResize)
					{
						EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.SplitResizeUpDown);
					}

					if (currentEventType == EventType.MouseDown && resizeRect.Contains(mousePosition) && !legendResize)
					{
						IsResizingGraphHeight = true;
						HeightResizingGraphIndex = i;
						GraphHeightBeforeResizing = graphHeight;
						MouseYPositionBeforeResizingGraphHeight = mousePosition.y;
						GraphHeightResizeDelta = 0;
					}

					if (currentEventType == EventType.MouseDrag && IsResizingGraphHeight)
					{
						GraphHeightResizeDelta = (mousePosition.y - MouseYPositionBeforeResizingGraphHeight);
						graphHeight = GraphHeightBeforeResizing + Mathf.FloorToInt(GraphHeightResizeDelta / (HeightResizingGraphIndex + 1));

						if (graphHeight < MinimumGraphHeight)
							graphHeight = MinimumGraphHeight;

						if (graphHeight > MaximumGraphHeight)
							graphHeight = MaximumGraphHeight;

						EditorPrefs.SetInt(EditorSettings.GraphHeight, graphHeight);
					}

					if (currentEventType == EventType.MouseUp && IsResizingGraphHeight)
					{
						IsResizingGraphHeight = false;
					}
				}

				// Do not draw graphs that is currently outside of display area.
				if (graphAreaRect.yMin > position.height || graphAreaRect.yMax < 0f)
					continue;

				Handles.color = zeroLineColor;
				var ratio = Mathf.Clamp(graphRect.height * range.Min / span + graphRect.yMax, graphRect.yMin, graphRect.yMax);

				horizontalLines[0] = new Vector3(graphRect.xMax, graphRect.yMin);
				horizontalLines[1] = new Vector3(graphRect.xMin, graphRect.yMin);
				horizontalLines[2] = new Vector3(graphRect.xMin, ratio);

				if (range.Min <= 0f && range.Max >= 0f)
				{
					horizontalLines[3] = new Vector3(graphRect.xMax, ratio);
				}
				else
				{
					horizontalLines[3] = new Vector3(graphRect.xMin, ratio);
				}

				horizontalLines[4] = new Vector3(graphRect.xMin, ratio);
				horizontalLines[5] = new Vector3(graphRect.xMin, graphRect.yMax);
				horizontalLines[6] = new Vector3(graphRect.xMax, graphRect.yMax);

				Handles.DrawPolyLine(horizontalLines);

				if (isInPauseMode)
				{
					var time = (timeEnd - timeStart) * (mousePosition.x - graphRect.xMin) / graphRect.width + timeStart;

					if (graphRect.Contains(mousePosition))
					{
						if (currentEventType == EventType.MouseDown)
						{
							timeIntervalStartTime = Mathf.Max(0f, time);
							timeIntervalEndTime = timeIntervalStartTime;
							timeIntervalSelectionGraph = graph;
						}
					}

					if (timeIntervalSelectionGraph == graph && currentEventType == EventType.MouseDrag)
					{
						timeIntervalEndTime = Mathf.Max(0f, time);
					}
				}

				DrawTickLines(graphRect, timeStart, timeEnd);

				foreach (var channel in graph.Channels)
				{
					var deselectedColor = channel.Color;
					deselectedColor.a = DeselectedChannelAlpha;

					var color = (selectedChannel == null) || (channel == selectedChannel) ? channel.Color : deselectedColor;

					Handles.color = color;

					var pointIndex = 0;

					for (int j = 0; j < channel.SampleBufferSize - 1; j++)
					{
						var index_a = (channel.CurrentSampleIndex + j) % channel.SampleBufferSize;
						var index_b = (index_a + 1) % channel.SampleBufferSize;

						var time_a = channel.SampleAxisX[index_a];
						var time_b = channel.SampleAxisX[index_b];

						if (float.IsNaN(time_a) || float.IsNaN(time_b))
							continue;

						if (time_b > time_a && !(time_b < timeStart || time_a > timeEnd))
						{
							var sample_a = channel.SampleAxisY[index_a];
							var sample_b = channel.SampleAxisY[index_b];

							if (float.IsNaN(sample_a) || float.IsNaN(sample_b))
								continue;

							var aNormalizedSample = (sample_a - range.Min) / span;
							if (span == 0f)
							{
								aNormalizedSample = 0.5f;
							}
							else
							{
								aNormalizedSample = Mathf.Clamp01(aNormalizedSample);
							}

							var bNormalizedSample = (sample_b - range.Min) / span;
							if (span == 0f)
							{
								bNormalizedSample = 0.5f;
							}
							else
							{
								bNormalizedSample = Mathf.Clamp01(bNormalizedSample);
							}

							// Draw graph step.
							if (interpolationTypeIndex == 0)
							{
								points[pointIndex++] = new Vector3(graphRect.xMin + graphRect.width * (time_b - timeStart) / timeWindow, graphRect.yMin + graphRect.height * (1f - bNormalizedSample), 0f);
							}
							else
							{
								points[pointIndex++] = new Vector3(graphRect.xMin + graphRect.width * (time_b - timeStart) / timeWindow, graphRect.yMin + graphRect.height * (1f - aNormalizedSample), 0f);
								points[pointIndex++] = new Vector3(graphRect.xMin + graphRect.width * (time_b - timeStart) / timeWindow, graphRect.yMin + graphRect.height * (1f - bNormalizedSample), 0f);
							}
						}
					}

					if (pointIndex > 0)
					{
						var lastPoint = points[pointIndex - 1];

						for (int p = pointIndex; p < points.Length; p++)
						{
							points[p] = lastPoint;
						}

						Handles.DrawPolyLine(points);
					}
				}

				if (timeIntervalSelectionGraph == graph && timeIntervalStartTime != timeIntervalEndTime)
				{
					GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);

					var selectionTime_left = Mathf.Max(0f, Mathf.Min(timeIntervalStartTime, timeIntervalEndTime));
					var selectionTime_right = Mathf.Max(0f, Mathf.Max(timeIntervalStartTime, timeIntervalEndTime));

					var left = graphRect.width * (selectionTime_left - timeStart) / (timeEnd - timeStart) + graphRect.xMin;
					var right = graphRect.width * (selectionTime_right - timeStart) / (timeEnd - timeStart) + graphRect.xMin;

					GUI.DrawTexture(new Rect(left, graphRect.yMin, right - left, graphRect.height), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);

					Handles.color = new Color(1f, 1f, 1f, 0.3f);
					Handles.DrawLine(new Vector3(left, 0, 0), new Vector3(left, height, 0));
					Handles.DrawLine(new Vector3(right, 0, 0), new Vector3(right, height, 0));

					GUI.color = Color.white;
					Handles.color = Color.white;
					Handles.DrawLine(new Vector3(left, graphRect.yMin, 0), new Vector3(left, graphRect.yMax, 0));
					Handles.DrawLine(new Vector3(right, graphRect.yMin, 0), new Vector3(right, graphRect.yMax, 0));
					Handles.DrawLine(new Vector3(left, (graphRect.yMin + graphRect.yMax) * 0.5f, 0), new Vector3(right, (graphRect.yMin + graphRect.yMax) * 0.5f, 0));

					GUI.Label(new Rect(left, graphRect.yMax, right - left, 20), (selectionTime_right - selectionTime_left) + " secs", timeIntervalSelectionStyle);
				}


				GUI.color = legendBackgroundColor;
				GUI.DrawTexture(new Rect(0f, graphAreaRect.yMin, legendWidth, graphAreaRect.height + 5), EditorGUIUtility.whiteTexture);

				// Draw context object name (with hyperlink to the object)
				if (graph.Context != null)
				{
					var contextNameRect = new Rect(22f, graphAreaRect.yMin + 10f, legendWidth - 30f, 16f);

					GUI.color = channelHeaderColor;
					GUI.Label(contextNameRect, graph.Context.name, simpleStyle);

					EditorGUIUtility.AddCursorRect(contextNameRect, MouseCursor.Link);

					if (currentEventType == EventType.MouseDown && contextNameRect.Contains(mousePosition))
					{
						EditorGUIUtility.PingObject(graph.Context);
					}
				}

				// Time line.

				var mouseTime = timeEnd;

				if (isInPauseMode)
				{
					mouseTime = Mathf.Lerp(timeStart, timeEnd, (mousePosition.x - graphRect.xMin) / graphRect.width);
				}

				mouseTime = Mathf.Max(mouseTime, 0f);

				Handles.color = timeLineColor;
				var x = (mouseTime - timeStart) / (timeEnd - timeStart) * graphRect.width + graphRect.xMin;
				Handles.DrawLine(new Vector3(x, settingsRect.height), new Vector3(x, position.height));

				for (int j = 0; j < graph.Channels.Count; j++)
				{
					var channel = graph.Channels[j];

					var deselectedColor = channel.Color;
					deselectedColor.a = DeselectedChannelAlpha;

					var channelColor = (selectedChannel == null) || (channel == selectedChannel) ? channel.Color : deselectedColor;

					var index = -1;

					for (int k = 1; k < channel.SampleAxisY.Length - 1; k++)
					{
						int sampleIndex_a = (channel.CurrentSampleIndex + k) % channel.SampleAxisY.Length;
						int sampleIndex_b = (sampleIndex_a + 1) % channel.SampleAxisY.Length;

						if (mouseTime >= channel.SampleAxisX[sampleIndex_a] &&
							mouseTime <= channel.SampleAxisX[sampleIndex_b])
						{
							index = Mathf.Abs(channel.SampleAxisX[sampleIndex_a] - mouseTime) <= Mathf.Abs(channel.SampleAxisX[sampleIndex_b] - mouseTime) ? sampleIndex_a : sampleIndex_b;
							break;
						}
					}

					var sampleValue = float.NaN;
					var time = float.NaN;
					var frame = -1;

					if (index > -1)
					{
						sampleValue = channel.SampleAxisY[index];
						time = channel.SampleAxisX[index];
						frame = channel.SampleFrames[index];
					}

					// Draw time marker.
					if (j == 0 && selectedChannel == null)
					{
						GUI.color = timeColor;

						if (!float.IsNaN(time))
						{
							GUI.Label(new Rect(legendTextOffset, graphAreaRect.yMax - legendTextOffset * 2f, legendWidth, 20),
								"t = " + time, timeStyle);
						}

						if (frame > -1)
						{
							GUI.Label(new Rect(legendTextOffset, graphAreaRect.yMax - legendTextOffset * 3.5f, legendWidth, 20),
								"frame = " + frame, timeStyle);
						}
					}

					Handles.color = channelColor;

					var normalizedSampleValue = (sampleValue - range.Min) / span;
					if (span == 0f)
					{
						normalizedSampleValue = 0.5f;
					}

					var clampedNormalizedSampleValue = Mathf.Clamp01(normalizedSampleValue);

					var samplePosition = new Vector3(graphRect.xMin + graphRect.width * (time - timeStart) / timeWindow, graphRect.yMax - graphRect.height * clampedNormalizedSampleValue, 0f);

					var handleRadius = 5f;

					if (normalizedSampleValue < 0f)
					{
						// Draw down arrow.
						arrowPoints[0] = samplePosition + new Vector3(-handleRadius, -handleRadius, 0);
						arrowPoints[1] = samplePosition + new Vector3(handleRadius, -handleRadius, 0);
						arrowPoints[2] = samplePosition + new Vector3(0, handleRadius, 0);
						arrowPoints[3] = arrowPoints[0];

						Handles.DrawPolyLine(arrowPoints);
					}
					else if (normalizedSampleValue > 1f)
					{
						// Draw up arrow.
						arrowPoints[0] = samplePosition + new Vector3(-handleRadius, handleRadius, 0);
						arrowPoints[1] = samplePosition + new Vector3(handleRadius, handleRadius, 0);
						arrowPoints[2] = samplePosition + new Vector3(0, -handleRadius, 0);
						arrowPoints[3] = arrowPoints[0];

						Handles.DrawPolyLine(arrowPoints);
					}
					else
					{
						// Draw circle.
						var size = handleRadius * 0.75f;
						diamondPoints[0] = samplePosition + new Vector3(0, size, 0);
						diamondPoints[1] = samplePosition + new Vector3(size, 0, 0);
						diamondPoints[2] = samplePosition + new Vector3(0, -size, 0);
						diamondPoints[3] = samplePosition + new Vector3(-size, 0, 0);
						diamondPoints[4] = diamondPoints[0];

						Handles.DrawPolyLine(diamondPoints);
					}

					string sampleValueString;
					if (float.IsNaN(sampleValue))
					{
						sampleValueString = "";
					}
					else
					{
						sampleValueString = " = " + sampleValue.ToString();
					}

					var valueText = channel.Description + sampleValueString;

					GUI.color = new Color(1f, 1f, 1f, 1f);
					valueTextStyle.normal.textColor = Color.white;

					if (channel == selectedChannel)
					{
						var sampleTextWidth = valueTextStyle.CalcSize(new GUIContent(valueText)).x;

						if (samplePosition.x + sampleTextWidth + 40 > position.width)
						{
							valueTextStyle.alignment = TextAnchor.MiddleRight;
							GUI.Label(new Rect(samplePosition.x - sampleTextWidth - 15, samplePosition.y - 20, sampleTextWidth, 20), valueText, valueTextStyle);
						}
						else
						{
							valueTextStyle.alignment = TextAnchor.MiddleLeft;
							GUI.Label(new Rect(samplePosition.x + 15, samplePosition.y, sampleTextWidth, 20), valueText, valueTextStyle);
						}

						GUI.color = new Color(1f, 1f, 1f, 0.5f);
						GUI.Label(new Rect(10, graphRect.yMax - 10, legendWidth, 20), "Time = " + time, timeStyle);
					}

					GUI.color = new Color(1f, 1f, 1f, 1f);

					valueTextStyle.normal.textColor = selectedChannel == null || selectedChannel == channel
						? legendTextColorSelected
						: legendTextColorUnselected;
					valueTextStyle.alignment = TextAnchor.MiddleLeft;
					valueTextStyle.clipping = TextClipping.Clip;

					var offset = 30f;
					var selectionRect = new Rect(0f, graphAreaRect.yMin + offset + 20 * j, legendWidth, 16f);
					GUI.Label(new Rect(22f, graphAreaRect.yMin + 30f + 20 * j, legendWidth - 30f, 16f), valueText, valueTextStyle);

					EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.Link);

					// Selection of channel.
					if (currentEventType == EventType.MouseDown && selectionRect.Contains(mousePosition))
					{
						newSelectedChannel = channel;
					}

					// Color marker.
					GUI.color = channelColor * 0.7f;
					GUI.DrawTexture(new Rect(10, graphAreaRect.yMin + offset + 20 * j + 6, 7, 7), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);

					GUI.color = channelColor;
					GUI.DrawTexture(new Rect(10 + 1, graphAreaRect.yMin + offset + 20 * j + 7, 5, 5), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);

					GUI.color = new Color(1f, 1f, 1f, 1f);

				}

				// Not cool to copy the list in every gui call. But simplifies the design, and the list is not too big anyway.
				TagEntries.Clear();
				graph.GetTagEntries(timeEnd - timeWindow, timeEnd, TagEntries);

				foreach (var entry in TagEntries)
				{
					var eventColor = Color.yellow;
					Handles.color = eventColor;

					var normalizedX = (entry.Time - timeStart) / timeWindow;
					if (normalizedX * graphRect.width >= 5f)
					{
						Handles.DrawLine(
							new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMin, 0f),
							new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMax, 0f)
						);

						Handles.DrawLine(
							new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMax, 0f),
							new Vector3(graphRect.xMin + graphRect.width * normalizedX + 5, graphRect.yMax + 5, 0f)
						);

						Handles.DrawLine(
							new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMax, 0f),
							new Vector3(graphRect.xMin + graphRect.width * normalizedX - 5, graphRect.yMax + 5, 0f)
						);

						Handles.DrawLine(
							new Vector3(graphRect.xMin + graphRect.width * normalizedX - 5, graphRect.yMax + 5, 0f),
							new Vector3(graphRect.xMin + graphRect.width * normalizedX + 5, graphRect.yMax + 5, 0f)
						);

						GUI.color = eventColor;
						GUI.contentColor = Color.white;
						GUI.Label(new Rect(graphRect.xMin + graphRect.width * normalizedX - 5, graphRect.yMax + 5f, 100f, 20f), entry.Text, simpleStyle);
					}
				}
			}

			// select/deselect.
			if (currentEventType == EventType.MouseDown)
			{
				selectedChannel = newSelectedChannel;
			}

			GUI.color = new Color(1f, 1f, 1f, 1f);
			GUI.DrawTexture(new Rect(legendWidth, settingsRect.height, 20f, position.height - settingsRect.height), GraphAreaGradientShadowTexture, ScaleMode.StretchToFill, true);
			GUI.DrawTexture(new Rect(0, settingsRect.height, width, 10), TitleBarGradientShadowTexture, ScaleMode.StretchToFill, true);

			for (int i = 0; i < FilteredGraphs.Count; i++)
			{
				// separator line
				Handles.color = Color.grey;
				Handles.DrawLine(new Vector3(0f, (i + 1) * graphHeight + settingsRect.height - scrollPositionY, 0f),
								  new Vector3(width, (i + 1) * graphHeight + settingsRect.height - scrollPositionY, 0f));
			}

			// Scrollbar
			var scrollMaxY = graphHeight * FilteredGraphs.Count + extraScrollSpace;
			var visibleHeightY = Mathf.Min(scrollMaxY, position.height - settingsRect.height);

			GUI.color = Color.white;
			scrollPositionY = GUI.VerticalScrollbar(new Rect(
				position.width - 15, settingsRect.height, 15f, position.height - settingsRect.height),
				scrollPositionY, visibleHeightY, 0f, scrollMaxY);
			scrollPositionY = Mathf.Max(scrollPositionY, 0f);

			if (isInPauseMode)
			{
				if (!wasInPauseMode)
				{
					// Reset scroll position when going into pause mode.
					scrollPositionTime = 0f;

					// Find the maximum time span in samples.
					var minTime = LatestTimeOfFilteredGraphs;
					var maxTime = LatestTimeOfFilteredGraphs;
					foreach (var graph in FilteredGraphs)
					{
						float graphMinTime, graphMaxTime;
						graph.GetMinMaxTime(out graphMinTime, out graphMaxTime);
						if (minTime > graphMinTime)
							minTime = graphMinTime;
						if (maxTime < graphMaxTime)
							maxTime = graphMaxTime;
					}

					scrollPositionTimeMax = (maxTime - minTime) + 1f;
				}

				GUI.color = Color.white;
				scrollPositionTime = GUI.HorizontalScrollbar(
					new Rect(legendWidth, height - 15f, width - legendWidth - 15f, 15f),
					scrollPositionTime,
					Mathf.Min(scrollPositionTimeMax, timeWindow),
					-scrollPositionTimeMax + timeWindow,
					timeWindow
				);

				scrollPositionTime = Mathf.Min(0f, scrollPositionTime);
			}

			// Top settings
			GUI.color = settingsHeaderBackgroundColor;
			GUI.DrawTexture(settingsRect, EditorGUIUtility.whiteTexture);
			GUI.color = Color.white;

			var padding = 5f;
			GUILayout.BeginArea(new Rect(settingsRect.xMin + padding, settingsRect.yMin + padding, settingsRect.width - 2 * padding, settingsRect.height - 2 * padding));
			GUILayout.BeginHorizontal();

			// Draw context filter dropdown
			DrawContextFilterDropdown(currentEventType);

			// Interpolation selection.
			GUILayout.Space(5f);
			GUILayout.Label("Interpolation", GUILayout.Width(85));
			EditorGUI.BeginChangeCheck();
			interpolationTypeIndex = EditorGUILayout.Popup(interpolationTypeIndex, InterpolationTypes, GUILayout.Width(120));
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetInt(EditorSettings.InterpolationType, interpolationTypeIndex);
			}

			EditorGUI.BeginChangeCheck();
			timeWindow = GUILayout.HorizontalSlider(timeWindow, 0.1f, 20f);
			if (EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetFloat(EditorSettings.TimeWindow, timeWindow);
			}
			GUILayout.Label(timeWindow.ToString("N1") + " secs", GUILayout.Width(60));
			GUILayout.Space(5f);

			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			var splitSize = 6f;
			var legendResizeRect = new Rect(legendWidth - splitSize / 2, 0, splitSize, height);
			EditorGUIUtility.AddCursorRect(legendResizeRect, MouseCursor.SplitResizeLeftRight);

			if (currentEventType == EventType.MouseDown && legendResizeRect.Contains(mousePosition) && !IsResizingGraphHeight)
			{
				legendResize = true;
			}

			if (currentEventType == EventType.MouseDrag && legendResize)
			{
				legendWidth = Mathf.FloorToInt(mousePosition.x);
				EditorPrefs.SetInt(EditorSettings.LegendWidth, legendWidth);
			}

			if (currentEventType == EventType.MouseUp && legendResize)
			{
				legendResize = false;
			}

			Repaint();

			wasInPauseMode = isInPauseMode;
		}

		#endregion

		#region GUI - Textures

		private Texture2D TitleBarGradientShadowTexture;
		private Texture2D GraphAreaGradientShadowTexture;

		private void CreateTexturesIfRequired()
		{
			if (TitleBarGradientShadowTexture == null)
			{
				TitleBarGradientShadowTexture = TextureTools.CreateVerticalGradientTexture(TitleBarGradientShadowTextureColors);
			}
			if (GraphAreaGradientShadowTexture == null)
			{
				GraphAreaGradientShadowTexture = TextureTools.CreateHorizontalGradientTexture(GraphAreaGradientShadowTextureColors);
			}
		}

		private void DestroyTextures()
		{
			if (TitleBarGradientShadowTexture != null)
			{
				DestroyImmediate(TitleBarGradientShadowTexture);
			}
			if (GraphAreaGradientShadowTexture != null)
			{
				DestroyImmediate(GraphAreaGradientShadowTexture);
			}
		}

		#endregion

		#region GUI - Tick Lines

		private void DrawTickLines(Rect graphRect, float timeStart, float timeEnd)
		{
			// Sub tick lines
			{
				var n = 0;
				var startTime = Mathf.CeilToInt(timeStart / SubSecondLinesInterval) * SubSecondLinesInterval;
				var t = startTime;

				if (timeWindow < TimeWindowForSubSecondLinesToAppear)
				{
					var subTimeTickColorWithAlpha = SubSecondLinesColor;
					subTimeTickColorWithAlpha.a *= 1f - (timeWindow - TimeWindowForSubSecondLinesToGetFullyOpaque) / (TimeWindowForSubSecondLinesToAppear - TimeWindowForSubSecondLinesToGetFullyOpaque);

					Handles.color = subTimeTickColorWithAlpha;

					while (t < timeEnd)
					{
						var x = graphRect.xMin + graphRect.width * (t - timeStart) / timeWindow;
						Handles.DrawLine(
							new Vector3(x, graphRect.yMax, 0f),
							new Vector3(x, graphRect.yMax - graphRect.height, 0f)
						);

						n++;
						t = startTime + n * 0.1f;
					}
				}
			}

			// Tick lines
			{
				Handles.color = SecondLinesColor;
				var n = 0;
				var startTime = Mathf.CeilToInt(timeStart);
				var t = startTime;

				while (t < timeEnd)
				{
					var x = graphRect.xMin + graphRect.width * (t - timeStart) / timeWindow;
					Handles.DrawLine(
						new Vector3(x, graphRect.yMax, 0f),
						new Vector3(x, graphRect.yMax - graphRect.height, 0f)
					);

					n++;
					t = startTime + n;
				}
			}
		}

		#endregion

		#region Context Filtering

		private GameObject ContextFilter = null;
		private readonly List<Graph> FilteredGraphs = new List<Graph>(10);
		private float LatestTimeOfFilteredGraphs;

		private List<GameObject> ContextObjects = new List<GameObject>(20);
		private string[] DisplayedContextNames;

		public bool SetContextFilter(GameObject filteredObject)
		{
			if (ContextFilter == filteredObject)
				return true; // Ignore

			if (!filteredObject)
			{
				ShowNotification(new GUIContent("Displaying all objects"));

				ContextFilter = null;
				return true;
			}

			if (Graphs.IsAnyGraphForContextExists(filteredObject))
			{
				ContextFilter = filteredObject;

				EditorGUIUtility.PingObject(ContextFilter);
				ShowNotification(new GUIContent($"Displaying only object '{filteredObject.name}'"));
				return true;
			}
			else
			{
				ContextFilter = null;

				ShowNotification(new GUIContent($"No graph to filter for '{filteredObject.name}'!"));
				return false;
			}
		}

		private void GatherContextFilteredGraphs(EventType currentEventType)
		{
			if (currentEventType != EventType.Layout)
				return;

			if (ContextFilter != null)
			{
				FilteredGraphs.Clear();
				foreach (var graph in Graphs.All)
				{
					if (graph.Context == ContextFilter)
					{
						FilteredGraphs.Add(graph);
					}
				}
			}
			else
			{
				// Not cool to copy the list in every GUI call. But simplifies the design, and the list is not too big anyway.
				FilteredGraphs.Clear();
				FilteredGraphs.AddRange(Graphs.All);
			}

			LatestTimeOfFilteredGraphs = 0f;
			for (int i = 0; i < FilteredGraphs.Count; i++)
			{
				var time = FilteredGraphs[i].LatestTime;
				if (LatestTimeOfFilteredGraphs < time)
					LatestTimeOfFilteredGraphs = time;
			}
		}

		private void DrawContextFilterDropdown(EventType currentEventType)
		{
			// Gather visible context object names
			if (currentEventType == EventType.Layout)
			{
				ContextObjects.Clear();
				Graphs.GatherDisplayedContextObjectNames(ContextObjects, ref DisplayedContextNames);
			}

			// Draw dropdown
			{
				// Find current context filter index
				var currentContextFilterIndex = 0; // This is the 'All' option.
				for (int i = 0; i < ContextObjects.Count; i++)
				{
					if (ContextFilter == ContextObjects[i])
					{
						currentContextFilterIndex = i + 1;
					}
				}

				var newContextFilterIndex = EditorGUILayout.Popup(currentContextFilterIndex, DisplayedContextNames, GUILayout.Width(160));

				if (currentContextFilterIndex != newContextFilterIndex)
				{
					scrollPositionY = 0;

					if (newContextFilterIndex == 0)
					{
						SetContextFilter(null);
					}
					else
					{
						SetContextFilter(ContextObjects[newContextFilterIndex - 1]);
					}
				}
			}
		}

		#endregion
	}

}