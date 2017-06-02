/*
using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;

namespace Extenity.CodeSnippetsToolbox.Editor
{

	public class CodeSnippetsWindow : ExtenityEditorWindowBase
	{
		#region Initialization

		protected void Awake()
		{
			EditorApplication.update += Update;

			StartNamespaceGathering();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			EditorApplication.update -= Update;

			CancelNamespaceGathering();
		}

		#endregion

		#region Update

		protected void Update()
		{
			UpdateNamespaceGathering();
		}

		#endregion

		#region Configuration

		private string BuildConfigurationKey<T>(Expression<Func<T>> propertyLambda)
		{
			return "CodeSnippets." + ApplicationTools.AsciiProductName + "." + TypeTools.GetPropertyName(propertyLambda);
		}

		private string _Namespace;
		public string Namespace
		{
			get
			{
				if (_Namespace == null)
				{
					_Namespace = EditorPrefs.GetString(BuildConfigurationKey(() => Namespace), "");
				}
				return _Namespace;
			}
			set
			{
				if (_Namespace == value)
					return;

				_Namespace = value;
				EditorPrefs.SetString(BuildConfigurationKey(() => Namespace), _Namespace);
			}
		}

		#endregion

		#region GUI

		protected override void OnGUIDerived()
		{
			EditorGUILayout.BeginVertical();

			Namespace = EditorGUILayout.TextField("Namespace (can be empty)", Namespace);

			GUILayout.Label("Namespaces: " + NamespacesCount);
			EditorGUILayoutTools.ProgressBar("Progress", NamespaceGatheringProgress);

			for (int i = 0; i < Namespaces.Count; i++)
			{
				var ns = Namespaces[i];
				if (GUILayout.Button(ns))
				{
					Namespace = ns;
				}
			}

			//ClassDirectoryPath = EditorGUILayout.TextField("Namespace (can be empty)", Namespace);

			EditorGUILayout.EndVertical();
		}

		#endregion

		#region All Namespaces In Project

		public List<string> Namespaces;
		public int NamespacesCount { get { return Namespaces == null ? 0 : Namespaces.Count; } }
		private List<string> NamespacesListToCheckAvailability;
		public bool IsGatheringNamespaces { get; private set; }
		public float NamespaceGatheringProgress { get; private set; }
		private Thread NamespaceGatheringThread;
		private ThreadStopper NamespaceGatheringThreadStopper;
		private static readonly string NamespaceContentSearchString = "namespace ";
		private static readonly object NamespaceListLocker = new object();
		private bool SignalToFinalizeNamespaceGathering { get; set; }
		private bool SignalToRefreshGUI { get; set; }

		private class ThreadStopper
		{
			public bool IsSignaledToStop;
		}

		public void StartNamespaceGathering()
		{
			if (IsGatheringNamespaces)
			{
				CancelNamespaceGathering();
			}

			IsGatheringNamespaces = true;
			SignalToFinalizeNamespaceGathering = false;
			NamespacesListToCheckAvailability = Namespaces.Clone();
			NamespaceGatheringProgress = 0f;

			// Sort initial list (if there is any)
			SortNamespaces();

			// Create thread
			{
				NamespaceGatheringThreadStopper = new ThreadStopper();
				NamespaceGatheringThread = new Thread(NamespaceGatheringThreadMethod);
				NamespaceGatheringThread.Start(NamespaceGatheringThreadStopper);
			}
		}

		private void UpdateNamespaceGathering()
		{
			if (SignalToFinalizeNamespaceGathering)
			{
				FinalizeNamespaceGathering();
			}

			if (SignalToRefreshGUI)
			{
				RefreshNamespaceGatheringGUI();
			}
		}

		private void RefreshNamespaceGatheringGUI()
		{
			Repaint();
		}

		private void NamespaceGatheringThreadMethod(object threadStopperObject)
		{
			try
			{
				var threadStopper = (ThreadStopper)threadStopperObject;

				if (threadStopper.IsSignaledToStop)
					return;

				var path = Application.dataPath;
				var sourceFiles = Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories);

				if (sourceFiles != null)
				{
					// Look for namespaces in all files
					for (int i = 0; i < sourceFiles.Length; i++)
					{
						// Read file content
						var sourceFile = sourceFiles[i];
						var fileContent = File.ReadAllText(sourceFile);

						if (threadStopper.IsSignaledToStop)
							return;

						// Quickly search for namespace in file
						var foundNamespaceIndex = 0;
						while (true)
						{
							foundNamespaceIndex = fileContent.IndexOf(NamespaceContentSearchString, foundNamespaceIndex);
							if (foundNamespaceIndex < 0)
								return;

							if (threadStopper.IsSignaledToStop)
								return;

							var previousChar = fileContent[foundNamespaceIndex - 1];
							if (previousChar == '\n' || previousChar == '\r')
							{
								var namespaceStartIndex = foundNamespaceIndex + NamespaceContentSearchString.Length;
								var lineEndIndex = fileContent.IndexOfNextLineEnding(namespaceStartIndex);
								if (lineEndIndex < 0)
									break; // There is something clearly wrong. Don't even try to handle this case.

								// Found it!
								var foundNamespace = fileContent.Substring(namespaceStartIndex, lineEndIndex - namespaceStartIndex);

								if (threadStopper.IsSignaledToStop)
									return;

								AddToNamespaces(foundNamespace);
								SignalToRefreshGUI = true;
								break;
							}

							// Calculate progress
							var progress = (float)i / sourceFiles.Length;

							if (threadStopper.IsSignaledToStop)
								return;
							NamespaceGatheringProgress = progress;
							SignalToRefreshGUI = true;
						}
					}
				}

				if (threadStopper.IsSignaledToStop)
					return;
				SignalToFinalizeNamespaceGathering = true;
			}
			catch
			{
			}
		}

		public void CancelNamespaceGathering(bool gracefullyEnded = false)
		{
			if (!IsGatheringNamespaces)
				return;

			if (gracefullyEnded)
				NamespaceGatheringProgress = 1f;
			else
				NamespaceGatheringProgress = 0f;

			NamespacesListToCheckAvailability = null;

			NamespaceGatheringThreadStopper.IsSignaledToStop = true;
			NamespaceGatheringThreadStopper = null;
			// NamespaceGatheringThread.Join(); // No need to wait for join. Let the thread fade itself out.
			NamespaceGatheringThread = null;

			SignalToFinalizeNamespaceGathering = false;
		}

		private void FinalizeNamespaceGathering()
		{
			SignalToFinalizeNamespaceGathering = false;

			// Remove detected unavailable namespaces
			{
				for (int i = 0; i < NamespacesListToCheckAvailability.Count; i++)
				{
					var unavailableNamespace = NamespacesListToCheckAvailability[i];
					Namespaces.Remove(unavailableNamespace);
				}
				NamespacesListToCheckAvailability = null;
			}

			CancelNamespaceGathering(true);
		}

		public void SortNamespaces()
		{
			lock (NamespaceListLocker)
			{
				if (Namespaces != null)
				{
					Namespaces.Sort();
				}
			}
		}

		public void AddToNamespaces(string value)
		{
			lock (NamespaceListLocker)
			{
				value = value.Trim();

				NamespacesListToCheckAvailability.Remove(value);

				if (Namespaces == null)
				{
					Namespaces = new List<string>(50);
				}

				if (Namespace.Contains(value))
					return;

				Namespaces.AddSorted(value);
			}
		}

		#endregion
	}

}
*/
