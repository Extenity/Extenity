using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Extenity.CryptoToolbox;
using Extenity.DataToolbox;
using Newtonsoft.Json;
using UnityEngine;

namespace Extenity.ApplicationToolbox
{

	[JsonObject(MemberSerialization.OptIn)]
	public abstract class LiveSettingsBase<TDerived> where TDerived : LiveSettingsBase<TDerived>
	{
		#region Initialization

		protected LiveSettingsBase()
		{
#if UNITY_EDITOR
			CheckConsistency();
#endif
		}

		#endregion

		#region Data Structure

		private List<(FieldInfo FieldInfo, string Key)> GetFields()
		{
			return GetType()
				.GetFields(
					BindingFlags.Instance |
					BindingFlags.FlattenHierarchy |
					BindingFlags.Public |
					BindingFlags.NonPublic)
				.Where(field => field.GetCustomAttribute<JsonPropertyAttribute>() != null)
				.Select(field => (field, field.GetCustomAttribute<JsonPropertyAttribute>().PropertyName))
				.ToList();
		}

#if UNITY_EDITOR

		private void CheckDataStructureConsistency()
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

		#region Serialize

		public string Serialize(string key)
		{
			var json = ToJson(false);
			return SimpleTwoWayEncryptorAES.EncryptBase64WithIV(json, key);
		}

		public string ToJson(bool pretty = false)
		{
			return JsonConvert.SerializeObject(this, pretty ? Formatting.Indented : Formatting.None);
		}

		#endregion
		
		#region Deserialize

		public static TDerived Deserialize(string serialized, string key)
		{
			if (string.IsNullOrEmpty(serialized))
				throw new ArgumentNullException();
			var json = SimpleTwoWayEncryptorAES.DecryptBase64WithIV(serialized, key);
			return JsonConvert.DeserializeObject<TDerived>(json);
		}
		

		/* This was the old implementation. Now we use Json serialization.
		public void Parse(Dictionary<string, string> keyValueStore)
		{
			var fields = GetFields();
			if (fields.Count == 0)
				throw new InternalException(117457212);

			foreach (var field in fields)
			{
				var fieldType = field.FieldInfo.FieldType;
				if (fieldType == typeof(float))
				{
					ParseFloat(keyValueStore, field.Key, field.FieldInfo);
				}
				else if (fieldType == typeof(int))
				{
					ParseInt(keyValueStore, field.Key, field.FieldInfo);
				}
				else
				{
					// Field type is not implemented. Probably the type was not needed before. Just see the lines above and implement it.
					throw new InternalException(11845362, new Exception(fieldType.Name));
				}
			}
		}

		/// <summary>
		/// Tries to parse the value inside keyValueStore. Leaves the value intact when it fails to parse.
		/// </summary>
		private void ParseInt(Dictionary<string, string> keyValueStore, string key, FieldInfo fieldInfo)
		{
			if (keyValueStore.TryGetValue(key, out var text))
			{
				if (int.TryParse(text, out var valueInSettings))
				{
					fieldInfo.SetValue(this, valueInSettings);
				}
			}
		}

		/// <summary>
		/// Tries to parse the value inside keyValueStore. Leaves the value intact when it fails to parse.
		/// </summary>
		private void ParseFloat(Dictionary<string, string> keyValueStore, string key, FieldInfo fieldInfo)
		{
			if (keyValueStore.TryGetValue(key, out var text))
			{
				if (float.TryParse(text, out var valueInSettings))
				{
					fieldInfo.SetValue(this, valueInSettings);
				}
			}
		}
		*/

		#endregion

		#region Diff

#if UNITY_EDITOR

		public static string Diff(LiveSettingsBase<TDerived> original, LiveSettingsBase<TDerived> modified, string linePrefix = "\t")
		{
			if (original == null)
				throw new ArgumentNullException(nameof(original));
			if (modified == null)
				throw new ArgumentNullException(nameof(modified));

			var result = new StringBuilder();

			var originalJson = JsonUtility.ToJson(original, true);
			var modifiedJson = JsonUtility.ToJson(modified, true);

			var originalLines = originalJson.Split(StringTools.LineEndingCharacters, StringSplitOptions.RemoveEmptyEntries);
			var modifiedLines = modifiedJson.Split(StringTools.LineEndingCharacters, StringSplitOptions.RemoveEmptyEntries);

			if (originalLines.Length != modifiedLines.Length)
			{
				Log.Info("Original:\n" + originalJson);
				Log.Info("Modified:\n" + modifiedJson);
				throw new Exception(); // This is not expected.
			}

			for (int i = 0; i < originalLines.Length; i++)
			{
				if (originalLines[i] != modifiedLines[i])
				{
					var separator = originalLines[i].IndexOf(':');
					var key = originalLines[i].Substring(0, separator).Trim().Trim('\"');
					if (originalLines[i].Substring(0, separator) != modifiedLines[i].Substring(0, separator))
						throw new Exception(); // This is not expected.
					var originalValue = originalLines[i].Substring(separator + 1).Trim();
					var modifiedValue = modifiedLines[i].Substring(separator + 1).Trim();
					if (originalValue.EndsWith(","))
						originalValue = originalValue.Substring(0, originalValue.Length - 1);
					if (modifiedValue.EndsWith(","))
						modifiedValue = modifiedValue.Substring(0, modifiedValue.Length - 1);
					result.AppendLine($"{linePrefix}{key} \t: {originalValue}  =>  {modifiedValue}");
				}
			}

			return result.Length == 0
				? $"{linePrefix}No difference."
				: result.ToString();
		}

#endif

		#endregion

		#region Consistency

#if UNITY_EDITOR

		private void CheckConsistency()
		{
			CheckDataStructureConsistency();
		}

#endif

		#endregion
	}

}
