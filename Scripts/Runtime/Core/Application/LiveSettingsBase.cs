using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Extenity.DataToolbox;

namespace Extenity.ApplicationToolbox
{

	public class LiveKeyAttribute : Attribute
	{
		public readonly string Key;

		public LiveKeyAttribute(string key)
		{
			Key = key;
		}
	}

	public abstract class LiveSettingsBase
	{
		#region Initialization

		public LiveSettingsBase()
		{
#if UNITY_EDITOR
			CheckConsistency();
#endif
		}

		#endregion

		#region Read Data

		private delegate bool TryParseHandler<T>(string value, out T result);

		public void ReadData(Dictionary<string, string> keyValueStore)
		{
			var fields = GetFields();
			if (fields.Count == 0)
				throw new InternalException(117457212);

			foreach (var field in fields)
			{
				var fieldType = field.FieldInfo.FieldType;
				if (fieldType == typeof(float))
				{
					ParseLiveSetting<float>(keyValueStore, field.Key, field.FieldInfo, float.TryParse);
				}
				else if (fieldType == typeof(int))
				{
					ParseLiveSetting<int>(keyValueStore, field.Key, field.FieldInfo, int.TryParse);
				}
				else
				{
					// Field type is not implemented. Probably the type was not needed before. Just see the lines above and implement it.
					throw new InternalException(11845362, new Exception(fieldType.Name));
				}
			}
		}

		private List<(FieldInfo FieldInfo, string Key)> GetFields()
		{
			return GetType()
				.GetFields(
					BindingFlags.Instance |
					BindingFlags.FlattenHierarchy |
					BindingFlags.Public |
					BindingFlags.NonPublic)
				.Where(field => field.GetCustomAttribute<LiveKeyAttribute>() != null)
				.Select(field => (field, field.GetCustomAttribute<LiveKeyAttribute>().Key))
				.ToList();
		}

		/// <summary>
		/// Tries to parse the value inside keyValueStore. Leaves the value intact when it fails to parse.
		/// </summary>
		private void ParseLiveSetting<T>(Dictionary<string, string> keyValueStore, string key, FieldInfo fieldInfo, TryParseHandler<T> tryParseHandler)
		{
			if (keyValueStore.TryGetValue(key, out var text))
			{
				if (tryParseHandler(text, out var valueInSettings))
				{
					fieldInfo.SetValue(this, valueInSettings);
				}
			}
		}

		#endregion

		#region Consistency

#if UNITY_EDITOR

		private void CheckConsistency()
		{
			var fields = GetFields();

			// Check if there are duplicate keys.
			{
				var duplicates = fields.Select(field => field.Key).Duplicates(EqualityComparer<string>.Default);
				foreach (var duplicate in duplicates)
				{
					Log.Error($"There are duplicate keys for '{duplicate}' in {GetType().Name}.");
				}
			}
		}

#endif

		#endregion
	}

}
