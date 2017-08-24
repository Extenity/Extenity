using System;
using System.Collections.Generic;
using Extenity.DebugFlowTool.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Extenity.DebugFlowTool.Unity
{

	public static class DebugFlow
	{
		#region Initialization

		private static bool IsInitialized;

		private static void InitializeIfRequired()
		{
			if (!IsInitialized)
			{
				CreateRunID();
				InitializeGameObject();
				InitializePages();
				InitializeThreading();
				InitializeConnector(); // This should be placed below all others.

				IsInitialized = true;
			}
		}

		#endregion

		#region Deinitialization

		//protected void OnDestroy()
		//{
		//}

		#endregion

		#region Update

		//protected void Update()
		//{
		//}

		#endregion

		#region Metadata

		/// <summary>
		/// A randomly generated ID that represents the application instance. Gets a new value each time the application runs.
		/// </summary>
		public static int RunID { get; private set; }

		private static void CreateRunID()
		{
			var random = new System.Random((int)(DateTime.Now.Ticks % int.MaxValue));
			RunID = random.Next(100, int.MaxValue);
		}

		#endregion

		#region Pages

		public static List<Page> Pages;

		private static void InitializePages()
		{
			Pages = new List<Page>(5);
		}

		public static Page GetPage(string name)
		{
			for (var i = 0; i < Pages.Count; i++)
			{
				if (Pages[i].PageName == name)
					return Pages[i];
			}
			return null;
		}

		#endregion

		#region Getters

		public static TimedChartGroup GetTimedChartGroup(string pageName, string groupName)
		{
			var page = GetPage(pageName);
			if (page != null)
			{
				return page.GetTimedChartGroup(groupName);
			}
			return null;
		}

		public static TimedChart GetTimedChart(string pageName, string groupName, string chartName)
		{
			var page = GetPage(pageName);
			if (page != null)
			{
				var group = page.GetTimedChartGroup(groupName);
				if (group != null)
				{
					return group.GetTimedChart(chartName);
				}
			}
			return null;
		}

		#endregion

		#region Creators

		public static TimedChart CreateTimedChart(string pageName, string groupName, string chartName, Color color)
		{
			lock (DebugFlowLocker)
			{
				InitializeIfRequired();

				var page = InternalGetOrCreatePage(pageName);
				var group = InternalGetOrCreateTimedChartGroup(page, groupName);
				return InternalGetOrCreateTimedChart(group, chartName, color);
			}
		}

		private static Page InternalGetOrCreatePage(string pageName)
		{
			var page = GetPage(pageName);
			if (page != null)
				return page;

			// Create page
			page = new Page(GetNextUniqueID(), pageName);
			Pages.Add(page);
			Connector.Connection.InformCreatePage(page);
			return page;
		}

		private static TimedChartGroup InternalGetOrCreateTimedChartGroup(Page page, string groupName)
		{
			var group = page.GetTimedChartGroup(groupName);
			if (group != null)
				return group;

			// Create TimedChartGroup
			group = new TimedChartGroup(GetNextUniqueID(), groupName);
			page.TimedChartGroups.Add(group);
			Connector.Connection.InformCreateTimedChartGroup(group);
			return group;
		}

		private static TimedChart InternalGetOrCreateTimedChart(TimedChartGroup group, string chartName, Color color)
		{
			var chart = group.GetTimedChart(chartName);
			if (chart != null)
				return chart;

			// Create TimedChart
			chart = new TimedChart(GetNextUniqueID(), chartName, color.ToDebugFlowColor());
			group.TimedCharts.Add(chart);
			Connector.Connection.InformCreateTimedChart(chart);
			return chart;
		}

		#endregion

		#region Connector

		public static Connector Connector;

		private static void InitializeConnector()
		{
			Connector = DebugFlowGameObject.AddComponent<Connector>();
		}

		#endregion

		#region MonoBehaviour

		private static GameObject DebugFlowGameObject;

		private static void InitializeGameObject()
		{
			DebugFlowGameObject = new GameObject("_DebugFlow");
			GameObject.DontDestroyOnLoad(DebugFlowGameObject);
			DebugFlowGameObject.hideFlags = HideFlags.DontSave;
		}

		#endregion

		#region Threading

		private static object DebugFlowLocker;

		private static void InitializeThreading()
		{
			DebugFlowLocker = new object();
		}

		#endregion

		#region ID Generator

		private static int LastGivenID = 100;

		internal static int GetNextUniqueID()
		{
			return ++LastGivenID;
		}

		#endregion
	}

}
