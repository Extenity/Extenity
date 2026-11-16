using System.Collections.Generic;
using UnityEngine;

namespace ExtenityTests.InspectorToolbox.Demo
{

	/// <summary>
	/// An object without a custom editor whose fields have nested inspectors: shows them inside whatever draws
	/// the object by default. Only used by tests and screenshots.
	/// </summary>
	public class InspectorToolboxPlainDemo : ScriptableObject
	{
		public string Name = "No custom editor";
		public DemoItem Item = new DemoItem { Id = "nested-item", IsLimited = true };
		public DemoVersionSettings Version = new DemoVersionSettings { MinimumSupportedVersion = "1.0.0" };
		public DemoBoxedSettings BoxedSettings = new DemoBoxedSettings { Volume = 0.4f };
		public DemoIdentifier Identifier = new DemoIdentifier("flattened");
		public List<DemoItem> Items = new List<DemoItem> { new DemoItem { Id = "first" }, new DemoItem { Id = "second" } };
		[SerializeReference] public DemoObjective Objective = new DemoWaitObjective { Description = "Wait a little" };
	}

}
