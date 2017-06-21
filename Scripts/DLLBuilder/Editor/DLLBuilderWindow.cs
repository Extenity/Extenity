using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace Extenity.DLLBuilder
{

	public class DLLBuilderWindow : ExtenityEditorWindowBase
	{
		#region Initialization

		private void OnEnable()
		{
			DLLBuilder.OnRepaintRequested.AddListener(ThreadSafeRepaint);
			InitializeStatusMessage();
		}

		#endregion

		#region Deinitialization

		private void OnDisable()
		{
			DeinitializeStatusMessage();
		}

		#endregion

		#region Show

		[MenuItem(Constants.MenuItemPrefix + "Open Builder", priority = 1000)]
		public static void ShowWindow()
		{
			var window = GetWindow<DLLBuilderWindow>();
			window.titleContent = new GUIContent("DLL Builder");
		}

		#endregion

		#region GUI

		public static readonly GUILayoutOption[] ThinButtonOptions = { GUILayout.Width(230f), GUILayout.Height(24f) };
		public static readonly GUILayoutOption[] ThickButtonOptions = { GUILayout.Width(230f), GUILayout.Height(46f) };

		protected override void OnGUIDerived()
		{
			GUILayout.Space(20f);

			GUI.enabled = !DLLBuilder.IsProcessing;
			if (GUILayout.Button("Build And Distribute", ThickButtonOptions))
			{
				DLLBuilder.StartProcess(BuildTriggerSource.UI);
			}
			GUI.enabled = true;

			GUILayout.Space(20f);
			if (GUILayout.Button("Configuration", ThickButtonOptions))
			{
				DLLBuilderConfiguration.SelectOrCreateConfigurationAsset();
			}

			GUILayout.Space(40f);

			DrawStatusMessage();

			GUILayout.Space(40f);
			GUILayout.Label("Cleanup");

			if (GUILayout.Button("Clear All Output DLLs", ThinButtonOptions))
			{
				Cleaner.ClearAllOutputDLLs(DLLBuilderConfiguration.Instance);
			}

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

		#endregion

		#region GUI - Status Message

		private GUIStyle[] StatusMessageStyles;
		private static readonly int DisplayedStatusMessageCount = 3;
		private DLLBuilder.StatusMessage[] LastStatusMessages;
		private int LastStatusMessageIndex;
		private int TotalStatusMessageCount;

		private void InitializeStatusMessage()
		{
			// Clear the list
			LastStatusMessages = new DLLBuilder.StatusMessage[DisplayedStatusMessageCount];
			LastStatusMessageIndex = -1;
			TotalStatusMessageCount = 0;
			DLLBuilder.OnStatusChanged.AddListener(OnStatusMessageReceived);
		}

		private void DeinitializeStatusMessage()
		{
			DLLBuilder.OnStatusChanged.RemoveListener(OnStatusMessageReceived);
		}

		private void OnStatusMessageReceived()
		{
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
				StatusMessageStyles[(int)DLLBuilder.StatusMessageType.Normal] = new GUIStyle(GUI.skin.label);
				StatusMessageStyles[(int)DLLBuilder.StatusMessageType.Warning] = new GUIStyle(GUI.skin.label);
				StatusMessageStyles[(int)DLLBuilder.StatusMessageType.Warning].normal.textColor = Color.yellow;
				StatusMessageStyles[(int)DLLBuilder.StatusMessageType.Error] = new GUIStyle(GUI.skin.label);
				StatusMessageStyles[(int)DLLBuilder.StatusMessageType.Error].normal.textColor = Color.red;
			}

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
