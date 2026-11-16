using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExtenityTests.InspectorToolbox.Demo
{

	[Serializable]
	public class DemoItem
	{
		public string Id = "item";
		public int Price = 10;
		public bool IsLimited;
		public int Stock = 5;
		public string Notes = "";
	}

	[Serializable]
	public struct DemoBand
	{
		public string Name;
		public int From;
		public int To;
		public float Weight;
	}

	[Serializable]
	public abstract class DemoObjective
	{
		public string Description = "Describe the objective";
		public bool IsOptional;
	}

	[Serializable]
	public class DemoCollectObjective : DemoObjective
	{
		public string ItemId = "coin";
		public int Amount = 10;
	}

	[Serializable]
	public class DemoReachObjective : DemoObjective
	{
		public Vector3 Position;
		public float Radius = 2f;
	}

	[Serializable]
	public class DemoWaitObjective : DemoObjective
	{
		public float Seconds = 30f;
	}

	[Serializable]
	public struct DemoBoxedSettings
	{
		public float Volume;
		public bool Muted;
	}

	/// <summary>Shows the list building blocks of the inspector toolbox. Only used by tests and screenshots.</summary>
	public class InspectorToolboxListDemo : ScriptableObject
	{
		public List<DemoItem> UnityDefaultList = new List<DemoItem>
		{
			new DemoItem { Id = "default-a" },
			new DemoItem { Id = "default-b", IsLimited = true },
		};

		public List<DemoItem> Items = new List<DemoItem>
		{
			new DemoItem { Id = "sword", Price = 120 },
			new DemoItem { Id = "shield", Price = 80, IsLimited = true, Stock = 2 },
			new DemoItem { Id = "sword", Price = 95, Notes = "Same id as the first one" },
		};

		public List<string> SearchableNames = new List<string> { "Apple", "Apricot", "Banana", "Cherry", "Date", "Elderberry" };

		public List<string> PickedCategories = new List<string> { "Fruit/Apple", "Vegetable/Carrot", "Gone" };

		[SerializeReference]
		public List<DemoObjective> Objectives = new List<DemoObjective>
		{
			new DemoCollectObjective { Description = "Collect ten coins" },
			new DemoReachObjective { Description = "Reach the tower", IsOptional = true },
			null,
		};

		public List<DemoBand> Bands = new List<DemoBand>
		{
			new DemoBand { Name = "Cheap", From = 0, To = 50, Weight = 0.6f },
			new DemoBand { Name = "Normal", From = 50, To = 200, Weight = 0.3f },
			new DemoBand { Name = "Luxury", From = 200, To = 1000, Weight = 0.1f },
		};

		public DemoItem SingleItem = new DemoItem { Id = "single" };
		public DemoBoxedSettings BoxedSettings = new DemoBoxedSettings { Volume = 0.7f };
	}

}
