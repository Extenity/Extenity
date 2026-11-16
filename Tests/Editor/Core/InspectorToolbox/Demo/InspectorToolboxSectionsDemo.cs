using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtenityTests.InspectorToolbox.Demo
{

	[Serializable]
	public class DemoAudioSettings
	{
		[Range(0f, 1f)] public float MasterVolume = 0.8f;
		[Range(0f, 1f)] public float MusicVolume = 0.5f;
		public bool MuteInBackground = true;
	}

	[Serializable]
	public class DemoShopSettings
	{
		public string Currency = "Gold";
		public int StartingMoney = 100;
		public List<DemoItem> Items = new List<DemoItem> { new DemoItem { Id = "potion" }, new DemoItem { Id = "elixir", IsLimited = true } };
	}

	[Serializable]
	public class DemoDifficultySettings
	{
		public float EnemyHealthMultiplier = 1f;
		public float EnemyDamageMultiplier = 1f;
		public int Lives = 3;
	}

	/// <summary>Shows an object whose top-level fields are sections. Only used by tests and screenshots.</summary>
	public class InspectorToolboxSectionsDemo : ScriptableObject
	{
		public string ProjectName = "Sections Demo";
		public DemoAudioSettings Audio = new DemoAudioSettings();
		public DemoShopSettings Shop = new DemoShopSettings();
		public DemoDifficultySettings Difficulty = new DemoDifficultySettings();
		public DemoVersionSettings Version = new DemoVersionSettings { MinimumSupportedVersion = "0.9.0" };
		public List<string> Tags = new List<string> { "demo", "sections" };
	}

}
