#if !UNITY_CLOUD_BUILD

using System;
using System.Collections.Generic;
using Extenity.CodingToolbox;

namespace UnityEngine.CloudBuild
{

	/// <summary>
	/// The mock of Unity's CloudBuild DLL that is injected into the project in Cloud Builds. There is no library that
	/// can be used in development, so this is the mock class to make the code buildable.
	///
	/// The way to get the class definition is to call the code below in a Cloud Build and log the class internals.
	/// typeof(UnityEngine.CloudBuild.BuildManifestObject).Assembly.ToStringDetails();
	///
	/// This implementation is based on Unity 2019.2.9f1.
	/// </summary>
	// TODO MAINTENANCE: Update that in new Unity versions.
	[OverrideEnsuredNamespace("UnityEngine.CloudBuild")]
	public abstract class BuildManifestObject
	{
		// public static BuildManifestObject LoadCloudBuildManifest();
		// public static BuildManifestObject LoadFromResourcesOrCreateNew(string resourcesPath);

		/// <summary>
		/// Tries to get a manifest value - returns true if key was found and could be cast to type T, false otherwise.
		/// </summary>
		public abstract bool TryGetValue<T>(string key, out T result);

		/// <summary>
		/// Retrieve a manifest value or throw an exception if the given key isn't found.
		/// </summary>
		public abstract T GetValue<T>(string key);

		/// <summary>
		/// Retrieve a manifest value or return default value if the given key isn't found.
		/// </summary>
		[Obsolete("Use GetValue<type>(string, object) instead.")]
		public abstract string GetValue(string key, string defaultValue);

		/// <summary>
		/// Retrieve a manifest value or throw an exception if the given key isn't found.
		/// </summary>
		public abstract string GetValue(string key);

		/// <summary>
		/// Sets the value for a given key.
		/// </summary>
		public abstract void SetValue(string key, object value);

		/// <summary>
		/// Copy values from a dictionary. ToString() will be called on dictionary values before being stored.
		/// </summary>
		public abstract void SetValues(Dictionary<string, object> sourceDict);

		/// <summary>
		/// Remove all key/value pairs
		/// </summary>
		public abstract void ClearValues();

		/// <summary>
		/// Returns a Dictionary that represents the current BuildManifestObject
		/// </summary>
		public abstract Dictionary<string, object> ToDictionary();

		/// <summary>
		/// Returns a JSON formatted string that represents the current BuildManifestObject
		/// </summary>
		public abstract string ToJson();

		/// <summary>
		/// Returns an INI formatted string that represents the current BuildManifestObject
		/// </summary>
		public new abstract string ToString();
	}

}

#endif
