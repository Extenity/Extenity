using System;
using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Extenity.UnityEditorToolbox.Editor
{

	public static class LightmapEditorSettingsTools
	{
		public static void SetDirectSamples(int value)
		{
			SetLightmapSettingsIntProperty("m_LightmapEditorSettings.m_PVRDirectSampleCount", value);
		}

		public static void SetIndirectSamples(int value)
		{
			SetLightmapSettingsIntProperty("m_LightmapEditorSettings.m_PVRSampleCount", value);
		}

		public static void SetBounces(int value)
		{
			SetLightmapSettingsIntProperty("m_LightmapEditorSettings.m_PVRBounces", value);
		}

		public static void SetLightmapSettingsFloatProperty(string name, float val)
		{
			SetLightmapSettingsProperty(name, property => property.floatValue = val);
		}

		public static void SetLightmapSettingsIntProperty(string name, int val)
		{
			SetLightmapSettingsProperty(name, property => property.intValue = val);
		}

		public static void SetLightmapSettingsBoolProperty(string name, bool val)
		{
			SetLightmapSettingsProperty(name, property => property.boolValue = val);
		}

		public static void SetLightmapSettingsProperty(string name, Action<SerializedProperty> changer)
		{
			var lightmapSettings = GetLightmapSettingsSerializedObject();
			var prop = lightmapSettings.FindProperty(name);
			if (prop != null)
			{
				changer(prop);
				lightmapSettings.ApplyModifiedProperties();
			}
			else
				throw new Exception("Failed to find lightmap property: " + name);
		}

		public static SerializedObject GetLightmapSettingsSerializedObject()
		{
			var getLightmapSettingsMethod = typeof(LightmapEditorSettings).GetMethod("GetLightmapSettings", BindingFlags.Static | BindingFlags.NonPublic);
			var lightmapSettings = getLightmapSettingsMethod.Invoke(null, null) as Object;
			return new SerializedObject(lightmapSettings);
		}
	}

}
