using System;
using System.Collections.Generic;
using System.Linq;
using Extenity.GameObjectToolbox;
using UnityEngine;

namespace Extenity.CameraToolbox
{

	[Serializable]
	public class CameraEffectsConfiguration
	{
		#region Data

		[Serializable]
		public class CameraEffectComponentData
		{
			[Serializable]
			public struct FieldData
			{
				public string Name;
				public object Value;
			}

			public string Name;
			public List<FieldData> SerializedFields;
		}

		public List<CameraEffectComponentData> Components;

		#endregion

		#region Camera Component Filtering

		private static readonly List<string> BaseCameraGameObjectComponents = new List<string>
		{
			"Transform",
			"Camera",
			"AudioListener",
			"GUILayer",
			"FlareLayer",
		};

		#endregion

		#region Get/Set Configuration Of Camera

		public void GetEffectsConfigurationFromCamera(Camera camera)
		{
			if (camera == null)
				throw new ArgumentNullException("camera");

			var effectComponents = GetEffectComponentsOfCamera(camera);

			// Create data
			var newComponents = new List<CameraEffectComponentData>(effectComponents.Count);

			// Get properties from components
			for (int i = 0; i < effectComponents.Count; i++)
			{
				var effectComponent = effectComponents[i];
				var effectComponentType = effectComponent.GetType();

				var componentData = new CameraEffectComponentData
				{
					Name = effectComponentType.Name,
					SerializedFields = new List<CameraEffectComponentData.FieldData>()
				};

				// Get serialized fields
				var effectComponentSerializedFields = effectComponent.GetUnitySerializedFields();
				foreach (var field in effectComponentSerializedFields)
				{
					if (field.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
					{
						Debug.LogErrorFormat("Field type '{0}' is not supported.", field.FieldType);
						continue;
					}

					var fieldName = field.Name;
					var fieldValue = field.GetValue(effectComponent);
					componentData.SerializedFields.Add(new CameraEffectComponentData.FieldData { Name = fieldName, Value = fieldValue });
				}

				newComponents.Add(componentData);
			}

			// Switch to new data if everything went right.
			Components = newComponents;
		}

		public void SetEffectsConfigurationToCamera(Camera camera)
		{
			if (camera == null)
				throw new ArgumentNullException("camera");

			// Remove all effects before starting to set new ones.
			RemoveEffectsFromCamera(camera);

			// Set serialized fields data into instantiated components
			for (int iComponent = 0; iComponent < Components.Count; iComponent++)
			{
				var effectComponentData = Components[iComponent];

				// Instantiate component
				var effectComponentType = Type.GetType(effectComponentData.Name);
				if (effectComponentType == null)
				{
					Debug.LogErrorFormat("Camera effect '{0}' does not exist.", effectComponentData.Name);
					continue;
				}
				var effectComponent = camera.gameObject.AddComponent(effectComponentType);

				// Set data. But more importantly, compare serialized fields with the fields contained in data and give warning to user if there is anything odd.
				{
					// Check if all fields in instantiated component also exists in data
					var effectComponentSerializedFields = effectComponent.GetUnitySerializedFields();
					for (int iComponentField = effectComponentSerializedFields.Count - 1; iComponentField >= 0; iComponentField--)
					{
						var fieldName = effectComponentSerializedFields[iComponentField].Name;
						if (effectComponentData.SerializedFields.All(item => item.Name != fieldName))
						{
							Debug.LogWarningFormat(
								"Camera effect '{0}' has the field '{1}' which does not exist in saved camera properties.",
								effectComponent.name,
								fieldName);

							// Also remove from the list. We don't need them.
							effectComponentSerializedFields.RemoveAt(iComponentField);
						}
					}

					// Check if all fields in data also exists in instantiated component
					for (int iDataField = 0; iDataField < effectComponentData.SerializedFields.Count; iDataField++)
					{
						var fieldData = effectComponentData.SerializedFields[iDataField];
						var fieldName = fieldData.Name;
						var field = effectComponentSerializedFields.FirstOrDefault(item => item.Name == fieldName);
						if (field == null)
						{
							Debug.LogWarningFormat(
								"Camera effect '{0}' does not have the field '{1}' which exists in saved camera properties.",
								effectComponent.name,
								fieldName);
						}
						else
						{
							// Set the data
							field.SetValue(effectComponent, fieldData.Value);
						}
					}
				}
			}
		}

		public static void RemoveEffectsFromCamera(Camera camera)
		{
			var effectComponents = GetEffectComponentsOfCamera(camera);

			for (int i = 0; i < effectComponents.Count; i++)
			{
				Component.DestroyImmediate(effectComponents[i]);
			}
		}

		public static List<Component> GetEffectComponentsOfCamera(Camera camera)
		{
			var allComponents = camera.GetComponents<Component>();

			// Filter only the effect components
			var effectComponents = new List<Component>(allComponents.Length);
			{
				for (int i = 0; i < allComponents.Length; i++)
				{
					var component = allComponents[i];
					if (!BaseCameraGameObjectComponents.Contains(component.GetType().Name))
					{
						effectComponents.Add(component);
					}
				}
			}

			return effectComponents;
		}

		#endregion
	}

}
