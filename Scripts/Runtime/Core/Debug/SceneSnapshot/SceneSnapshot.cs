using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Extenity.ApplicationToolbox;
using Extenity.DataToolbox;
using Extenity.GameObjectToolbox;
using Extenity.SceneManagementToolbox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Extenity.DebugToolbox
{

	public class SceneSnapshot
	{
		public class FieldEntry
		{
			public readonly string Name;
			public readonly object Value;

			public FieldEntry(Component component, FieldInfo field)
			{
				Name = field.Name;
				Value = field.GetValue(component);
			}
		}

		public class ComponentEntry
		{
			public readonly Type Type;
			public readonly FieldEntry[] SerializedFields;

			public ComponentEntry(Component component)
			{
				Type = component.GetType();
				SerializedFields = component.GetUnitySerializedFields()
					.Select(field => new FieldEntry(component, field))
					.ToArray();
			}
		}

		public class GameObjectEntry
		{
			public readonly string FullPath;
			public readonly ComponentEntry[] Components;

			public GameObjectEntry(GameObject gameObject)
			{
				FullPath = gameObject.FullName();
				Components = gameObject.GetComponents<Component>()
					.Select(component => new ComponentEntry(component))
					.ToArray();
			}
		}

		public class SceneEntry
		{
			public readonly string Name;
			public readonly string Path;
			public readonly GameObjectEntry[] GameObjects;

			public SceneEntry(Scene scene)
			{
				Name = scene.name;
				Path = scene.path;
				GameObjects = scene.ListAllGameObjectsInScene()
					.Select(gameObject => new GameObjectEntry(gameObject))
					.OrderBy(item => item.FullPath)
					.ToArray();
			}
		}

		public SceneEntry[] Scenes;

		public void TakeSnapshotOfAllLoadedScenes()
		{
			var sceneInfos = SceneManagerTools.GetLoadedScenes(true);
			Scenes = sceneInfos.Select(sceneInfo => new SceneEntry(sceneInfo)).ToArray();

			// Also add the DontDestroyOnLoad scene
			Scenes = Scenes.Add(new SceneEntry(SceneManagerTools.GetDontDestroyOnLoadScene()));
		}

		public string BuildStringForGameObjectPaths()
		{
			if (Scenes == null)
				return null;

			var OUTPUT = new StringBuilder(); // Break the naming rules for easier reading

			foreach (var sceneEntry in Scenes)
			{
				OUTPUT.AppendLine($"Scene: '{sceneEntry.Name}' at path '{sceneEntry.Path}' with '{sceneEntry.GameObjects.Length}' game objects");
				foreach (var gameObjectEntry in sceneEntry.GameObjects)
				{
					OUTPUT.AppendLine(gameObjectEntry.FullPath);
				}
			}

			return OUTPUT.ToString();
		}

		public string BuildStringForAll()
		{
			if (Scenes == null)
				return null;

			var OUTPUT = new StringBuilder(); // Break the naming rules for easier reading

			foreach (var sceneEntry in Scenes)
			{
				OUTPUT.AppendLine($"Scene: '{sceneEntry.Name}' at path '{sceneEntry.Path}' with '{sceneEntry.GameObjects.Length}' game objects");
				foreach (var gameObjectEntry in sceneEntry.GameObjects)
				{
					OUTPUT.AppendLine(gameObjectEntry.FullPath);
					foreach (var componentEntry in gameObjectEntry.Components)
					{
						OUTPUT.AppendLine("\t" + componentEntry.Type.Name);
						foreach (var serializedField in componentEntry.SerializedFields)
						{
							OUTPUT.AppendLine("\t\t" + serializedField.Name + " = " + serializedField.Value?.ToString());
						}
					}
				}
			}

			return OUTPUT.ToString();
		}

		public void LogToFile(bool saveSimpleLog, bool saveDetailedLog)
		{
			var path = Path.Combine(Path.Combine(ApplicationTools.ApplicationPath, "Log"), "SceneSnapshot - " + DateTime.Now.ToFullDateTimeMsecForFilename() + ".txt");

			if (saveSimpleLog)
			{
				var message = BuildStringForGameObjectPaths();
				LogToFile(path, message);
			}
			if (saveDetailedLog)
			{
				var message = BuildStringForAll();
				LogToFile(path.AddSuffixToFileName(" - Detailed"), message);
			}
		}

		private void LogToFile(string path, string message)
		{
			Debug.Log("Saving log at path: " + path);
			DirectoryTools.CreateFromFilePath(path);
			File.WriteAllText(path, message);
		}
	}

}
