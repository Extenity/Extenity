// ============================================================================
//   Monitor Components v. 1.04 - written by Peter Bruun (twitter.com/ptrbrn)
//   More info on Asset Store: http://u3d.as/9MW
// ============================================================================

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MonitorComponents 
{
	public class MonitorsEditorWindow : EditorWindow
	{
		[MenuItem ("Window/Monitors _#%m")]
		static void Init() 
		{
			EditorWindow.GetWindow(typeof(MonitorsEditorWindow));
		}

		private Monitors monitors;

		private Texture2D topTexture;
		private Texture2D leftTexture;

		private float subTimeTicks = 0.1f;

		private float scrollPositionY = 0f;
		private float scrollPositionTime = 0f;
		private float scrollPositionTimeMax = 0f;

		private float monitorGraphHeight;
		private float legendTextOffset = 10f;
		private float extraScrollSpace = 30f;

		private MonitorInput selectedMonitorInput;

		// static sizes.
		private float headerHeight = 30f;
		private float extraSpace = 20f;
		private float deselectionAlpha = 0.2f;

		// dynamic sizes.
		private float width, height;
		private float monitorWidth;
		private int monitorHeight_min = 100;
		private int monitorHeight_max = 500;
		
		private bool monitorHeightResize = false;
		private int monitorHeightResizeOldHeight;
		private float monitorHeightResizeDelta = 0f;
		private float monitorHeightResizeYstart;
		private int monitorHeightResizeIndex;

		private bool legendResize = false;

		private bool wasInPauseMode = false;

		private string[] interpolationTypes = { "Linear", "Piecewise constant" };

		private Monitor timeIntervalSelectionMonitor = null;
		private float timeIntervalStartTime;
		private float timeIntervalEndTime;

		private GameObject gameObjectFilter = null;

		// Saved editor settings.
		private float timeWindow_exp;
		private int interpolationTypeIndex;
		private int monitorHeight;
		private int legendWidth;

		// Colors
		private Color backgroundColor;

		private Color legendBackgroundColor;
		private Color legendBackgroundColor_light;
		private Color legendBackgroundColor_dark;

		private Color settingsHeaderBackgroundColor_light; 
		private Color settingsHeaderBackgroundColor_dark; 
		private Color settingsHeaderBackgroundColor; 

		private Color legendTextColorSelected;
		private Color legendTextColorSelected_dark;
		private Color legendTextColorSelected_light;

		private Color legendTextColorUnselected;
		private Color legendTextColorUnselected_dark;
		private Color legendTextColorUnselected_light;

		private Color monitorInputHeaderColor;
		private Color monitorInputHeaderColor_dark;
		private Color monitorInputHeaderColor_light;
		
		private Color headerColor;
		private Color minMaxColor;
		private Color timeColor;
		private Color timeColor_dark;
		private Color timeColor_light;
		private Color zeroLineColor;
		private Color timeLineColor;
		private Color timeTickColor;
		private Color subTimeTickColor;

		private GUIStyle headerStyle;
		private GUIStyle minStyle, maxStyle;
		private GUIStyle timeIntervalSelectionStyle;
		private GUIStyle timeStyle;
		private GUIStyle valueTextStyle;
		private GUIStyle simpleStyle;

		private Vector3[] points = new Vector3[2000];
		private Vector3[] arrowPoints = new Vector3[4];
		private Vector3[] diamondPoints = new Vector3[5];
		private Vector3[] horizontalLines = new Vector3[7];

		public MonitorsEditorWindow() : base()
		{
			string titleText = "Monitors";

// EditorWindow.title is deprecated from Unity 5.1 and forward.
#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0
    		title = titleText;
#else
    		titleContent = new GUIContent(titleText);
#endif

			wantsMouseMove = true;
			width = 1000;
			height = 900;

			monitors = Monitors.Instance;

			// Colors
			backgroundColor = new Color(32f/255f, 32f/255f, 32f/255f, 1f);

			legendBackgroundColor_light = new Color(222f/255f, 222f/255f, 222f/255f);
			legendBackgroundColor_dark = new Color(65f/255f, 65f/255f, 65f/255f);

			settingsHeaderBackgroundColor_light = new Color(222f/255f, 222f/255f, 222f/255f); 
			settingsHeaderBackgroundColor_dark = new Color(58f/255f, 58f/255f, 58f/255f); 

			legendTextColorSelected_dark = new Color(1f, 1f, 1f, 1f);
			legendTextColorSelected_light = new Color(0f, 0f, 0f, 1f);

			legendTextColorUnselected_dark = new Color(1f, 1f, 1f, 0.5f);
			legendTextColorUnselected_light = new Color(0f, 0f, 0f, 0.5f);

			monitorInputHeaderColor_dark = new Color(0.5f, 0.5f, 0.5f);
			monitorInputHeaderColor_light = new Color(0.2f, 0.2f, 0.2f);
			
			headerColor = new Color(0.7f, 0.7f, 0.7f);
			minMaxColor = new Color(1f, 1f, 1f, 0.2f);
			timeColor_dark = new Color(1f, 1f, 1f, 0.5f);
			timeColor_light = new Color(0f, 0f, 0f, 0.5f);
			zeroLineColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
			timeLineColor = new Color(1f, 1f, 1f, 0.05f);
			timeTickColor = new Color(1f, 1f, 1f, 0.05f);
			subTimeTickColor = new Color(1f, 1f, 1f, 0.04f);
		}

		void OnEnable()
		{
			// static styles
			headerStyle = new GUIStyle();
			headerStyle.normal.textColor = headerColor;
			
			maxStyle = new GUIStyle();
			maxStyle.normal.textColor = minMaxColor;
			maxStyle.alignment = TextAnchor.LowerRight;
			
			minStyle = new GUIStyle();
			minStyle.normal.textColor = minMaxColor;
			minStyle.alignment = TextAnchor.UpperRight;
			
			GUIStyle timeWindowStyle = new GUIStyle();
			timeWindowStyle.normal.textColor = Color.grey;
			timeWindowStyle.alignment = TextAnchor.MiddleRight;
			
			valueTextStyle = new GUIStyle();
			
			timeIntervalSelectionStyle = new GUIStyle();
			timeIntervalSelectionStyle.clipping = TextClipping.Overflow;
			timeIntervalSelectionStyle.alignment = TextAnchor.MiddleCenter;
			timeIntervalSelectionStyle.normal.textColor = Color.white;
			
			timeStyle = new GUIStyle();

			simpleStyle = new GUIStyle();
			simpleStyle.normal.textColor = Color.white;
		}

		public void OnGUI()
		{
			// Make sure content color is sane.
			GUI.contentColor = Color.white;

			// Load editor settings.
			timeWindow_exp = EditorPrefs.GetFloat(TIME_WINDOW_EXP, 0.69897000433f);
			interpolationTypeIndex = EditorPrefs.GetInt(INTERPOLATION_TYPE_INDEX, 0);
			monitorHeight = EditorPrefs.GetInt(MONITOR_HEIGHT, 140);
			legendWidth = EditorPrefs.GetInt(LEGEND_WIDTH, 170);

			// Dynamic colors.
			if (EditorGUIUtility.isProSkin)
			{
				legendBackgroundColor = legendBackgroundColor_dark;
				timeColor = timeColor_dark;
				legendTextColorSelected = legendTextColorSelected_dark;
				legendTextColorUnselected = legendTextColorUnselected_dark;
				settingsHeaderBackgroundColor = settingsHeaderBackgroundColor_dark;
				monitorInputHeaderColor = monitorInputHeaderColor_dark;
			}
			else
			{
				legendBackgroundColor = legendBackgroundColor_light;
				timeColor = timeColor_light;
				legendTextColorSelected = legendTextColorSelected_light;
				legendTextColorUnselected = legendTextColorUnselected_light;
				settingsHeaderBackgroundColor = settingsHeaderBackgroundColor_light;
				monitorInputHeaderColor = monitorInputHeaderColor_light;
			}

			timeStyle.normal.textColor = timeColor;

			// textures (must check this in every OnGUI for some reason).

			if (topTexture == null)
			{
				topTexture = new Texture2D(1, 2);
				topTexture.hideFlags = HideFlags.HideAndDontSave;
				topTexture.wrapMode = TextureWrapMode.Clamp;
				topTexture.SetPixel(0, 1, new Color(22f/255f, 22f/255f, 22f/255f, 1f));
				topTexture.SetPixel(0, 0, new Color(32f/255f, 32f/255f, 32f/255f, 0f));
				topTexture.Apply();	
			}

			if (leftTexture == null)
			{
				leftTexture = new Texture2D(2, 1);
				leftTexture.hideFlags = HideFlags.HideAndDontSave;
				leftTexture.wrapMode = TextureWrapMode.Clamp;
				leftTexture.SetPixel(0, 0, new Color(22f/255f, 22f/255f, 22f/255f, 1f));
				leftTexture.SetPixel(1, 0, new Color(32f/255f, 32f/255f, 32f/255f, 0f));
				leftTexture.Apply();	
			}

			// calculate dynamic sizes.
			width = position.width;
			height = position.height;
			monitorWidth = width - legendWidth - 5f;

			monitorGraphHeight = monitorHeight - headerHeight - extraSpace;

			// settings header (prelude)
			Rect settingsRect = new Rect(0f, 0f, position.width, 25f);
			float timeWindow = Mathf.Pow(10f, timeWindow_exp);

			// draw background.
			GUI.color = backgroundColor;
			GUI.DrawTexture(new Rect(0, settingsRect.height, width, height - settingsRect.height), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);

			GUI.color = legendBackgroundColor;
			GUI.DrawTexture(new Rect(0f, settingsRect.height, legendWidth, height - settingsRect.height), EditorGUIUtility.whiteTexture);

			Event e = Event.current;

			Vector2 mousePosition = e.mousePosition;

			mousePosition.x = Mathf.Min(mousePosition.x, width);

			bool isInPauseMode = (EditorApplication.isPlaying && EditorApplication.isPaused);

			if (!isInPauseMode)
			{
				// smooth catch up. Nice!
				scrollPositionTime = 0f;	
			}

			if (e.type == EventType.MouseDown && 
				mousePosition.x > legendWidth && 
				(mousePosition.x < (width - 14f)) && 
				mousePosition.y > settingsRect.height)
			{
				Debug.Break();
			}

			MonitorInput newSelectedMonitorInput = null;

			int lineCount = 0;

			List<Monitor> visibleMonitors;

			if (gameObjectFilter != null)
			{
				visibleMonitors = new List<Monitor>(); // TODO: remove alloc.
				foreach(var monitor in monitors.All)
				{
					if (monitor.GameObject == gameObjectFilter)
					{
						visibleMonitors.Add(monitor);
					}
				}
			}
			else
			{
				visibleMonitors = monitors.All;
			}

			float latestTime = 0f;
			for (int i = 0; i < visibleMonitors.Count; i++)
			{
				latestTime = Mathf.Max(latestTime, visibleMonitors[i].latestTime);
			}

			for (int i = 0; i < visibleMonitors.Count; i++)
			{
				Monitor monitor = visibleMonitors[i];

				Rect monitorRect = new Rect(legendWidth, i * monitorHeight + settingsRect.height - scrollPositionY, monitorWidth, monitorHeight);
				Rect graphRect = new Rect(monitorRect.xMin, monitorRect.yMin + headerHeight, monitorRect.width - 20, monitorGraphHeight - 5); 

				float span = monitor.Max - monitor.Min;

				GUI.color = Color.white;
				GUI.Label(new Rect(legendWidth + 10f, monitorRect.yMin + 10, 100f, 30f), monitor.Name, headerStyle);

				float maxTime = latestTime + scrollPositionTime;
				float minTime = latestTime - timeWindow + scrollPositionTime;
		
				if(monitor.Mode == ValueAxisMode.Adaptive)
				{
					monitor.Min = float.PositiveInfinity;
					monitor.Max = float.NegativeInfinity;

					foreach(MonitorInput monitorInput in monitor.inputs)
					{
						float min, max;
						monitorInput.GetMinMax(minTime, maxTime, out min, out max);
						monitor.Min = Mathf.Min (min, monitor.Min);
						monitor.Max = Mathf.Max (max, monitor.Max);
					}
				}

				if (monitor.Min < float.PositiveInfinity)
					GUI.Label(new Rect(graphRect.xMax - 200f - 5f, graphRect.yMax + 5f, 200f, 20f), monitor.Min.ToString(), minStyle);
		
				if (monitor.Max > float.NegativeInfinity)
					GUI.Label(new Rect(graphRect.xMax - 200f - 5f, graphRect.yMin - 5f - 20f, 200f, 20f), monitor.Max.ToString(), maxStyle);

				// monitor resizing.

				Rect resizeRect = new Rect(0f, monitorRect.yMax - 10, width - 12, 21);
				if (!legendResize)
				{
					EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.SplitResizeUpDown);
				}

				if (e.type == EventType.MouseDown && resizeRect.Contains(mousePosition) && !legendResize)
				{
					monitorHeightResize = true;
					monitorHeightResizeIndex = i;
					monitorHeightResizeOldHeight = monitorHeight;
					monitorHeightResizeYstart = mousePosition.y;
					monitorHeightResizeDelta = 0;
				}

				if (e.type == EventType.MouseDrag && monitorHeightResize)
				{
					monitorHeightResizeDelta = (mousePosition.y - monitorHeightResizeYstart);
					monitorHeight = monitorHeightResizeOldHeight + Mathf.FloorToInt(monitorHeightResizeDelta / (monitorHeightResizeIndex + 1));

					if (monitorHeight < monitorHeight_min)
						monitorHeight = monitorHeight_min;

					if (monitorHeight > monitorHeight_max)
						monitorHeight = monitorHeight_max;
				}

				if (e.type == EventType.MouseUp && monitorHeightResize)
				{
					monitorHeightResize = false;
				}

				// Is monitor visible? otherwise cull...
				if (monitorRect.yMin < position.height && monitorRect.yMax > 0f)  
				{
					Handles.color = zeroLineColor;

					horizontalLines[0] = new Vector3(graphRect.xMax, graphRect.yMin);
					horizontalLines[1] = new Vector3(graphRect.xMin, graphRect.yMin);
					horizontalLines[2] = new Vector3(graphRect.xMin, Mathf.Clamp(graphRect.height * monitor.Min/span + graphRect.yMax, graphRect.yMin, graphRect.yMax));

					if(monitor.Min <= 0f && monitor.Max >= 0f)
					{
						horizontalLines[3] = new Vector3(graphRect.xMax, Mathf.Clamp(graphRect.height * monitor.Min/span + graphRect.yMax, graphRect.yMin, graphRect.yMax));
					}
					else
					{
						horizontalLines[3] = new Vector3(graphRect.xMin, Mathf.Clamp(graphRect.height * monitor.Min/span + graphRect.yMax, graphRect.yMin, graphRect.yMax));
					}

					horizontalLines[4] = new Vector3(graphRect.xMin, Mathf.Clamp(graphRect.height * monitor.Min/span + graphRect.yMax, graphRect.yMin, graphRect.yMax));
					horizontalLines[5] = new Vector3(graphRect.xMin, graphRect.yMax);
					horizontalLines[6] = new Vector3(graphRect.xMax, graphRect.yMax);

					Handles.DrawPolyLine(horizontalLines);

					lineCount++;
		
					if (isInPauseMode)
					{
						float time = (maxTime - minTime) * (mousePosition.x - graphRect.xMin)/graphRect.width + minTime;
						
						if (graphRect.Contains(mousePosition))
						{
							if (e.type == EventType.MouseDown)
							{
								timeIntervalStartTime = Mathf.Max(0f, time);
								timeIntervalEndTime = timeIntervalStartTime;
								timeIntervalSelectionMonitor = monitor;
							}
						}

						if (timeIntervalSelectionMonitor == monitor && e.type == EventType.MouseDrag)
						{
							timeIntervalEndTime = Mathf.Max(0f, time);
						}
					}

					int n = 0;
					float startTime = Mathf.CeilToInt(minTime/subTimeTicks) * subTimeTicks;
					float t = startTime;

					float transparentTime = 3;
					float opaqueTime = 2;

					// Sub tick lines.
					if(timeWindow < transparentTime)
					{
						Color subTimeTickColorWithAlpha = subTimeTickColor;
						subTimeTickColorWithAlpha.a = subTimeTickColor.a * Mathf.Lerp(1f, 0f, (timeWindow - opaqueTime)/(transparentTime - opaqueTime));

						Handles.color = subTimeTickColorWithAlpha;

						while(t < maxTime)
						{
							Handles.DrawLine(
								new Vector3(graphRect.xMin + graphRect.width * (t - minTime) / timeWindow, graphRect.yMax, 0f), 
								new Vector3(graphRect.xMin + graphRect.width * (t - minTime) / timeWindow, graphRect.yMax - graphRect.height, 0f)
							);

							lineCount++;

							n++;
							t = startTime + n * 0.1f;
						}
					}


					// Tick lines.
					Handles.color = timeTickColor;
					n = 0;
					startTime = Mathf.CeilToInt(minTime);
					t = startTime;

					while(t < maxTime)
					{
						Handles.DrawLine(
							new Vector3(graphRect.xMin + graphRect.width * (t - minTime) / timeWindow, graphRect.yMax, 0f), 
							new Vector3(graphRect.xMin + graphRect.width * (t - minTime) / timeWindow, graphRect.yMax - graphRect.height, 0f)
						);

						lineCount++;

						n++;
						t = startTime + n;
					}

					foreach(MonitorInput monitorInput in monitor.inputs)
					{
						Color deselectedColor = monitorInput.Color;
						deselectedColor.a = deselectionAlpha;

						Color color = (selectedMonitorInput == null) || (monitorInput == selectedMonitorInput) ? monitorInput.Color : deselectedColor;

						Handles.color = color;
						
						int pointIndex = 0;

						for(int j = 0; j < monitorInput.numberOfSamples-1; j++)
						{
							int index_a = (monitorInput.sampleIndex + j) % monitorInput.numberOfSamples;
							int index_b = (index_a + 1) % monitorInput.numberOfSamples;

							float time_a = monitorInput.times[index_a];
							float time_b = monitorInput.times[index_b];

							if(float.IsNaN(time_a) || float.IsNaN(time_b))
								continue;

							if (time_b > time_a && !(time_b < minTime || time_a > maxTime))
							{
								float sample_a = monitorInput.samples[index_a];
								float sample_b = monitorInput.samples[index_b];

								if(float.IsNaN(sample_a) || float.IsNaN(sample_b))
									continue;

								float aNormalizedSample = (sample_a - monitor.Min)/span;
								if (span == 0f)
								{
									aNormalizedSample = 0.5f;
								}
								else
								{
									aNormalizedSample = Mathf.Clamp01(aNormalizedSample);
								}

								float bNormalizedSample = (sample_b - monitor.Min)/span;
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
									points[pointIndex++] = new Vector3(graphRect.xMin + graphRect.width * (time_b - minTime) / timeWindow, graphRect.yMin + graphRect.height * (1f - bNormalizedSample), 0f);
								}
								else
								{
									points[pointIndex++] = new Vector3(graphRect.xMin + graphRect.width * (time_b - minTime) / timeWindow, graphRect.yMin + graphRect.height * (1f - aNormalizedSample), 0f); 
									points[pointIndex++] = new Vector3(graphRect.xMin + graphRect.width * (time_b - minTime) / timeWindow, graphRect.yMin + graphRect.height * (1f - bNormalizedSample), 0f);
								}
							}
						}

						if (pointIndex > 0)
						{
							Vector3 lastPoint = points[pointIndex - 1];

							for(int p = pointIndex; p < points.Length; p++)
							{
								points[p] = lastPoint;
							}

							Handles.DrawPolyLine(points);
							lineCount++;
						}
					}

					if (timeIntervalSelectionMonitor == monitor && timeIntervalStartTime != timeIntervalEndTime)
					{
						GUI.color = new Color(0.5f, 0.5f, 0.5f, 0.1f);

						float selectionTime_left = Mathf.Max(0f, Mathf.Min(timeIntervalStartTime, timeIntervalEndTime));
						float selectionTime_right = Mathf.Max(0f, Mathf.Max(timeIntervalStartTime, timeIntervalEndTime));

						float left = graphRect.width * (selectionTime_left - minTime)/(maxTime - minTime) + graphRect.xMin;
						float right = graphRect.width * (selectionTime_right - minTime)/(maxTime - minTime) + graphRect.xMin;
						
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
					GUI.DrawTexture(new Rect(0f, monitorRect.yMin, legendWidth, monitorRect.height + 5), EditorGUIUtility.whiteTexture);

					// Game object name label.

					if (monitor.GameObject != null)
					{
						Rect gameObjectNameRect = new Rect(22f, monitorRect.yMin + 10f, legendWidth - 30f, 16f);

						GUI.color = monitorInputHeaderColor;
						GUI.Label(gameObjectNameRect, monitor.GameObject.name, simpleStyle);

						EditorGUIUtility.AddCursorRect(gameObjectNameRect, MouseCursor.Link);

						if (e.type == EventType.MouseDown && gameObjectNameRect.Contains(mousePosition))
						{
							EditorGUIUtility.PingObject(monitor.GameObject);
						}
					}

					// Time line.

					float mouseTime = maxTime; 

					if (isInPauseMode)
					{
						mouseTime = Mathf.Lerp(minTime, maxTime, (mousePosition.x - graphRect.xMin) / graphRect.width);
					}

					mouseTime = Mathf.Max(mouseTime, 0f);

					Handles.color = timeLineColor;
					float x = (mouseTime - minTime)/(maxTime - minTime) * graphRect.width + graphRect.xMin;
					Handles.DrawLine(new Vector3(x, settingsRect.height), new Vector3(x, position.height));
					
					for(int j = 0; j < monitor.inputs.Count; j++)
					{
						MonitorInput monitorInput = monitor.inputs[j];

						Color deselectedColor = monitorInput.Color;
						deselectedColor.a = deselectionAlpha;

						Color monitorInputColor = (selectedMonitorInput == null) || (monitorInput == selectedMonitorInput) ? monitorInput.Color : deselectedColor;

						int index = -1;

						for(int k = 1; k < monitorInput.samples.Length - 1; k++)
						{
							int sampleIndex_a = (monitorInput.sampleIndex + k) % monitorInput.samples.Length;
							int sampleIndex_b = (sampleIndex_a + 1) % monitorInput.samples.Length;

							if(mouseTime >= monitorInput.times[sampleIndex_a] && 
							   mouseTime <= monitorInput.times[sampleIndex_b])
							{
								index = Mathf.Abs(monitorInput.times[sampleIndex_a] - mouseTime) <= Mathf.Abs(monitorInput.times[sampleIndex_b] - mouseTime) ? sampleIndex_a : sampleIndex_b; 
								break;
							}
						}

						float sampleValue = float.NaN;
						float time = float.NaN;
						int frame = -1;

						if (index > -1)
						{
							sampleValue = monitorInput.samples[index];
							time = monitorInput.times[index];
							frame = monitorInput.frames[index];
						}

						// Draw time marker.
						if (j == 0 && selectedMonitorInput == null)
						{
							GUI.color = timeColor;

							if (!float.IsNaN(time))
							{
								GUI.Label(new Rect(legendTextOffset, monitorRect.yMax - legendTextOffset * 2f, legendWidth, 20), 
									"t = " + time, timeStyle);
							}

							if (frame > -1)
							{
								GUI.Label(new Rect(legendTextOffset, monitorRect.yMax - legendTextOffset * 3.5f, legendWidth, 20), 
									"frame = " + frame, timeStyle);
							}
						}

						Handles.color = monitorInputColor;

						float normalizedSampleValue = (sampleValue - monitor.Min)/span;
						if (span == 0f)
						{
							normalizedSampleValue = 0.5f;
						}

						float clampedNormalizedSampleValue = Mathf.Clamp01(normalizedSampleValue);

						Vector3 samplePosition = new Vector3(graphRect.xMin + graphRect.width * (time - minTime) / timeWindow, graphRect.yMax - graphRect.height * clampedNormalizedSampleValue, 0f);

						float handleRadius = 5f;

						if (normalizedSampleValue < 0f)
						{	
							// Draw down arrow.
							arrowPoints[0] = samplePosition + new Vector3(-handleRadius, -handleRadius, 0);
							arrowPoints[1] = samplePosition + new Vector3(handleRadius, -handleRadius, 0);
							arrowPoints[2] = samplePosition + new Vector3(0, handleRadius, 0);
							arrowPoints[3] = arrowPoints[0];

							Handles.DrawPolyLine(arrowPoints);
							lineCount++;
						}
						else if (normalizedSampleValue > 1f)
						{
							// Draw up arrow.
							arrowPoints[0] = samplePosition + new Vector3(-handleRadius, handleRadius, 0);
							arrowPoints[1] = samplePosition + new Vector3(handleRadius, handleRadius, 0);
							arrowPoints[2] = samplePosition + new Vector3(0, -handleRadius, 0);
							arrowPoints[3] = arrowPoints[0];

							Handles.DrawPolyLine(arrowPoints);
							lineCount++;
						}
						else
						{
							// Draw circle.
							float size = handleRadius * 0.75f;
							diamondPoints[0] = samplePosition + new Vector3(0, size, 0);
							diamondPoints[1] = samplePosition + new Vector3(size, 0, 0);
							diamondPoints[2] = samplePosition + new Vector3(0, -size, 0);
							diamondPoints[3] = samplePosition + new Vector3(-size, 0, 0);
							diamondPoints[4] = diamondPoints[0];
							
							Handles.DrawPolyLine(diamondPoints);
							lineCount++;
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

						string valueText = monitorInput.Description + sampleValueString;
				
						GUI.color = new Color(1f, 1f, 1f, 1f);
						valueTextStyle.normal.textColor = Color.white;

						if (monitorInput == selectedMonitorInput)
						{
							float sampleTextWidth = valueTextStyle.CalcSize(new GUIContent(valueText)).x;

							if(samplePosition.x + sampleTextWidth + 40 > position.width)
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
				
						valueTextStyle.normal.textColor = selectedMonitorInput == null || selectedMonitorInput == monitorInput ? legendTextColorSelected : legendTextColorUnselected; 
						valueTextStyle.alignment = TextAnchor.MiddleLeft;
						valueTextStyle.clipping = TextClipping.Clip;

						float offset = 30f;
						Rect selectionRect = new Rect(0f, monitorRect.yMin + offset + 20 * j, legendWidth, 16f);
						GUI.Label(new Rect(22f, monitorRect.yMin + 30f + 20 * j, legendWidth - 30f, 16f), valueText, valueTextStyle);

						EditorGUIUtility.AddCursorRect(selectionRect, MouseCursor.Link);

						// Selection of monitor input.
						if (e.type == EventType.MouseDown && selectionRect.Contains(mousePosition))
						{
							newSelectedMonitorInput = monitorInput;
						}

						// Color marker.
						GUI.color = monitorInputColor * 0.7f;
						GUI.DrawTexture(new Rect(10, monitorRect.yMin + offset + 20 * j + 6, 7, 7), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);

						GUI.color = monitorInputColor;
						GUI.DrawTexture(new Rect(10 + 1, monitorRect.yMin + offset + 20 * j + 7, 5, 5), EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill);

						GUI.color = new Color(1f, 1f, 1f, 1f);
						
					}

					List<MonitorEvent> monitorEvents = new List<MonitorEvent>();
					monitor.GetEvents(maxTime - timeWindow, maxTime, monitorEvents);
				
					foreach(var monitorEvent in monitorEvents)
					{
						Color eventColor = Color.yellow;
						Handles.color = eventColor;

						float normalizedX = (monitorEvent.time - minTime) / timeWindow;
						if (normalizedX * graphRect.width >= 5f)
						{
							Handles.DrawLine(
								new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMin, 0f), 
								new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMax, 0f)
							);
							lineCount++;

							Handles.DrawLine(
								new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMax, 0f), 
								new Vector3(graphRect.xMin + graphRect.width * normalizedX + 5, graphRect.yMax + 5, 0f)
							);
							lineCount++;

							Handles.DrawLine(
								new Vector3(graphRect.xMin + graphRect.width * normalizedX, graphRect.yMax, 0f), 
								new Vector3(graphRect.xMin + graphRect.width * normalizedX - 5, graphRect.yMax + 5, 0f)
							);
							lineCount++;

							Handles.DrawLine(
								new Vector3(graphRect.xMin + graphRect.width * normalizedX - 5, graphRect.yMax + 5, 0f), 
								new Vector3(graphRect.xMin + graphRect.width * normalizedX + 5, graphRect.yMax + 5, 0f)
							);
							lineCount++;

							GUI.color = eventColor;
							GUI.contentColor = Color.white;
							GUI.Label(new Rect(graphRect.xMin + graphRect.width * normalizedX - 5, graphRect.yMax + 5f, 100f, 20f), monitorEvent.text, simpleStyle);
						}
					}
				}
			}

			// select/deselect.
			if (e.type == EventType.MouseDown)
			{ 
				selectedMonitorInput = newSelectedMonitorInput;
			}

			// Left gradient.
			GUI.DrawTexture(new Rect(legendWidth, settingsRect.height, 10f, position.height - settingsRect.height), leftTexture, ScaleMode.StretchToFill, true);

			// Draw top gradient.	
			GUI.color = new Color(1f, 1f, 1f, 0.3f);
			GUI.DrawTexture(new Rect(0, settingsRect.height, width, 8), topTexture, ScaleMode.StretchToFill);

			for (int i = 0; i < visibleMonitors.Count; i++)
			{
				// separator line
				Handles.color = Color.grey;
				Handles.DrawLine(new Vector3(0f, (i + 1) * monitorHeight + settingsRect.height - scrollPositionY, 0f),
	 			                 new Vector3(width, (i + 1) * monitorHeight + settingsRect.height - scrollPositionY, 0f));
				lineCount++;
			}

			// Scrollbar
			float scrollMaxY = monitorHeight * visibleMonitors.Count + extraScrollSpace;
			float visibleHeightY = Mathf.Min(scrollMaxY, position.height - settingsRect.height);

			GUI.color = Color.white;
			scrollPositionY = GUI.VerticalScrollbar(new Rect(
				position.width - 15, settingsRect.height, 15f, position.height - settingsRect.height), 
				scrollPositionY, visibleHeightY, 0f, scrollMaxY);
			scrollPositionY = Mathf.Max(scrollPositionY, 0f);

			if (isInPauseMode)
			{	
				if (!wasInPauseMode)
				{
					// Reset scroll positionwhen going into pause mode.
					scrollPositionTime = 0f;

					// Find the maximum time span in samples.

					float minTime = float.PositiveInfinity;
					float maxTime = float.NegativeInfinity;
					
					foreach(var monitor in visibleMonitors)
					{
						float monitorMinTime, monitorMaxTime;
						monitor.GetMinMaxTime(out monitorMinTime, out monitorMaxTime);

						minTime = Mathf.Min(minTime, monitorMinTime);
						maxTime = Mathf.Max(maxTime, latestTime);
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

			float padding = 5f;
			float timeWindowFloored = Mathf.RoundToInt(timeWindow * 10f)/10f;
			GUILayout.BeginArea(new Rect(settingsRect.xMin + padding, settingsRect.yMin + padding, settingsRect.width - 2 * padding, settingsRect.height - 2 * padding));
			GUILayout.BeginHorizontal();

			// Find list of unique game objects (linked to monitors).
			List<GameObject> gameObjects = new List<GameObject>();
			foreach (Monitor monitor in monitors.All)
			{
				if (monitor.GameObject != null)
				{
					if (!gameObjects.Contains(monitor.GameObject))
					{
						gameObjects.Add(monitor.GameObject);
					}
				}
			}

			gameObjects.Sort((GameObject a, GameObject b) => {
				int nameDelta = a.name.CompareTo(b.name);
				if (nameDelta == 0)
				{
					return a.GetInstanceID().CompareTo(b.GetInstanceID());
				}
				else
				{
					return nameDelta;
				}
			});

			string[] visibleGameObjectNames = new string[gameObjects.Count];
			for(int i = 0; i < visibleGameObjectNames.Length; i++)
			{
				visibleGameObjectNames[i] = gameObjects[i].name;
			}

			for(int i = 0; i < visibleGameObjectNames.Length; i++)
			{
				int lastIndexWithSameName = i;
				for(int j = i + 1; j < visibleGameObjectNames.Length; j++)
				{
					if (visibleGameObjectNames[j] == visibleGameObjectNames[i])
					{
						lastIndexWithSameName = j;
					}
					else
					{
						break;
					}
				}

				if (lastIndexWithSameName > i)
				{
					int n = 1;
					for(int j = i; j <= lastIndexWithSameName; j++)
					{
						visibleGameObjectNames[j] = visibleGameObjectNames[j] + "/" + n + "";
						n++;
					}

					i = lastIndexWithSameName + 1;
				}
				
			}

			// Game object filter.

			int gameObjectFilterIndex = 0;

			string[] gameObjectFilterOptions = new string[gameObjects.Count + 1];
			gameObjectFilterOptions[0] = "All";
			for(int i = 0; i < gameObjects.Count; i++)
			{
				gameObjectFilterOptions[i + 1] = visibleGameObjectNames[i];

				if (gameObjectFilter == gameObjects[i])
				{
					gameObjectFilterIndex = i + 1;
				}
			}

			gameObjectFilterIndex = EditorGUILayout.Popup(gameObjectFilterIndex, gameObjectFilterOptions, GUILayout.Width(160));
		

			GameObject gameObjectFilter_old = gameObjectFilter;

			if (gameObjectFilterIndex == 0)
			{
				gameObjectFilter = null;
			}
			else
			{
				gameObjectFilter = gameObjects[gameObjectFilterIndex - 1];
			}

			if (gameObjectFilter != gameObjectFilter_old)
			{
				scrollPositionY = 0;
			}

			// Interpolation selection.
			GUILayout.Space(5f);
			GUILayout.Label("Interpolation", GUILayout.Width(75));
			interpolationTypeIndex = EditorGUILayout.Popup(interpolationTypeIndex, interpolationTypes, GUILayout.Width(120));

			timeWindow_exp = GUILayout.HorizontalSlider(timeWindow_exp, -1f, 1.30102999566f);
			GUILayout.Label(timeWindowFloored.ToString("0.0") + " secs.", GUILayout.Width(60));
			GUILayout.Space(5f);

			GUILayout.EndHorizontal();
			GUILayout.EndArea();

			float splitSize = 6;
			Rect legendResizeRect = new Rect(legendWidth - splitSize/2, 0, splitSize, height);
			EditorGUIUtility.AddCursorRect(legendResizeRect, MouseCursor.SplitResizeLeftRight);

			if (e.type == EventType.MouseDown && legendResizeRect.Contains(mousePosition) && !monitorHeightResize)
			{
				legendResize = true;
			}

			if (e.type == EventType.MouseDrag && legendResize)
			{
				legendWidth = Mathf.FloorToInt(mousePosition.x);
			}

			if (e.type == EventType.MouseUp && legendResize)
			{
				legendResize = false;
			}

			Repaint(); 

			wasInPauseMode = isInPauseMode;

			// Save editor settings.
			EditorPrefs.SetFloat(TIME_WINDOW_EXP, timeWindow_exp);
			EditorPrefs.SetInt(INTERPOLATION_TYPE_INDEX, interpolationTypeIndex);
			EditorPrefs.SetInt(MONITOR_HEIGHT, monitorHeight);
			EditorPrefs.SetInt(LEGEND_WIDTH, legendWidth);
		}

		public void OnDestroy()
		{
			if (topTexture != null)
			{
				DestroyImmediate(topTexture);
			}

			if (leftTexture != null)
			{
				DestroyImmediate(leftTexture);
			}
		}

	    // Constants for saving editor settings.
		private static string TIME_WINDOW_EXP = "MONITORCOMPONENTS_TIME_WINDOW_EXP";
		private static string INTERPOLATION_TYPE_INDEX = "MONITORCOMPONENTS_INTERPOLATION_TYPE_INDEX";
		private static string MONITOR_HEIGHT = "MONITORCOMPONENTS_MONITOR_HEIGHT";
		private static string LEGEND_WIDTH = "MONITORCOMPONENTS_LEGEND_WIDTH";

		public GameObject Filter 
		{ 
			set 
			{
				if (monitors.All.Exists((monitor) => monitor.GameObject == value)) 
				{
					gameObjectFilter = value; 
				}
			}
		}
	}
}