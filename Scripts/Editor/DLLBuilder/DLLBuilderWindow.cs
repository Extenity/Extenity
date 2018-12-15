using System;
using Extenity.DataToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public class DLLBuilderWindow : ExtenityEditorWindowBase
	{
		#region Configuration

		protected override WindowSpecifications Specifications => new WindowSpecifications
		{
			Title = "DLL Builder",
		};

		#endregion

		#region Initialization

		protected override void OnEnableDerived()
		{
			DLLBuilder.OnRepaintRequested.AddListener(ThreadSafeRepaint);
			InitializeStatusMessage();

			CompilationPipeline.assemblyCompilationStarted += RefreshUIOnCompilationStart;
			CompilationPipeline.assemblyCompilationFinished += RefreshUIOnCompilationEnd;
		}

		[MenuItem(Constants.MenuItemPrefix + "Open Builder", priority = 1000)]
		private static void ToggleWindow()
		{
			EditorTools.ToggleWindow<DLLBuilderWindow>();
		}

		#endregion

		#region Deinitialization

		private void OnDisable()
		{
			CompilationPipeline.assemblyCompilationStarted -= RefreshUIOnCompilationStart;
			CompilationPipeline.assemblyCompilationFinished -= RefreshUIOnCompilationEnd;

			DeinitializeStatusMessage();
		}

		#endregion

		#region GUI

		public static readonly GUILayoutOption[] ThinButtonOptions = { GUILayout.Width(230f), GUILayout.Height(24f) };
		public static readonly GUILayoutOption[] ThickButtonOptions = { GUILayout.Width(230f), GUILayout.Height(46f) };

		protected override void OnGUIDerived()
		{
			GUILayout.Space(20f);

			GUI.enabled = !DLLBuilder.IsProcessing && !EditorApplication.isCompiling;
			if (GUILayout.Button("Build Remote", ThickButtonOptions))
			{
				DLLBuilder.StartProcess(BuildTriggerSource.UI, null, true);
			}
			if (GUILayout.Button("Build All", ThickButtonOptions))
			{
				DLLBuilder.StartProcess(BuildTriggerSource.UI);
			}

			GUILayout.Space(20f);
			if (GUILayout.Button("Configuration", ThinButtonOptions))
			{
				DLLBuilderConfiguration.SelectOrCreateConfigurationAsset();
			}
			if (GUILayout.Button("Clear All Output DLLs", ThinButtonOptions))
			{
				Cleaner.ClearAllOutputDLLs(DLLBuilderConfiguration.Instance);
			}
			GUI.enabled = true;

			GUILayout.Space(30f);

			DrawStatusMessage();


			//if (GUILayout.Button("Build DLLs", ThinButtonOptions))
			//{
			//	DLLBuilder.BuildDLLs();
			//}

			//GUILayout.Space(40f);
			//GUILayout.Label("Package In Output Project");

			//if (GUILayout.Button("Copy Assets To Output Project", ThinButtonOptions))
			//{
			//	PackageBuilder.CopyExtenityAssetsToOutputProject();
			//}

			//GUILayout.Space(40f);
			//GUILayout.Label("Distribute To Outside Projects");

			//if (GUILayout.Button("Distribute", ThinButtonOptions))
			//{
			//	Distributer.DistributeToAll();
			//}
		}

		private void RefreshUIOnCompilationStart(string dummy)
		{
			ShowNotification(new GUIContent("COMPILING..."));
			Repaint();
		}

		private void RefreshUIOnCompilationEnd(string dummy1, CompilerMessage[] dummy2)
		{
			RemoveNotification();
			Repaint();
		}

		#endregion

		#region GUI - Status Message

		private GUIStyle[] StatusMessageStyles;
		private static readonly int DisplayedStatusMessageCount = 3;
		private DLLBuilder.StatusMessage[] LastStatusMessages;
		private int LastStatusMessageIndex;
		private DateTime LastStatusMessageTime;
		private int TotalStatusMessageCount;

		private void InitializeStatusMessage()
		{
			// Clear the list
			LastStatusMessages = new DLLBuilder.StatusMessage[DisplayedStatusMessageCount];
			LastStatusMessageIndex = -1;
			LastStatusMessageTime = DateTime.MinValue;
			TotalStatusMessageCount = 0;
			DLLBuilder.OnStatusChanged.AddListener(OnStatusMessageReceived);
		}

		private void DeinitializeStatusMessage()
		{
			DLLBuilder.OnStatusChanged.RemoveListener(OnStatusMessageReceived);
		}

		private void OnStatusMessageReceived()
		{
			LastStatusMessageTime = DateTime.Now;
			TotalStatusMessageCount++;
			LastStatusMessageIndex++;
			if (LastStatusMessageIndex >= DisplayedStatusMessageCount)
				LastStatusMessageIndex = 0;
			LastStatusMessages[LastStatusMessageIndex] = DLLBuilder.CurrentStatus.Clone();
			ThreadSafeRepaint();
		}

		private void DrawStatusMessage()
		{
			if (LastStatusMessageIndex < 0)
				return;

			if (StatusMessageStyles == null || StatusMessageStyles.Length == 0)
			{
				StatusMessageStyles = new GUIStyle[3];
				StatusMessageStyles[(int)StatusMessageType.Normal] = new GUIStyle(GUI.skin.label);
				StatusMessageStyles[(int)StatusMessageType.Warning] = new GUIStyle(GUI.skin.label);
				StatusMessageStyles[(int)StatusMessageType.Warning].normal.textColor = Color.yellow;
				StatusMessageStyles[(int)StatusMessageType.Error] = new GUIStyle(GUI.skin.label);
				StatusMessageStyles[(int)StatusMessageType.Error].normal.textColor = Color.red;
			}

			GUILayout.Label(LastStatusMessageTime.ToFullDateTimeMsec());

			var i = LastStatusMessageIndex;
			for (int iZero = 0; iZero < DisplayedStatusMessageCount; iZero++)
			{
				var message = LastStatusMessages[i];
				if (message == null)
					break;
				GUILayout.Label((TotalStatusMessageCount - iZero) + ") " + message.Message, StatusMessageStyles[(int)message.Type]);
				i--;
				if (i < 0)
					i += DisplayedStatusMessageCount;
			}
		}

		#endregion
	}

}
