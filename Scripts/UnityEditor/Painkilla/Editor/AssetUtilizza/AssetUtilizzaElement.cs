using System;
using Extenity.IMGUIToolbox.Editor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Extenity.PainkillaTool.Editor
{

	[Serializable]
	public class AssetUtilizzaElement : TreeElement
	{
		public int intValue1;

		public float floatValue2, floatValue3;
		public Material material;
		public string text = "";
		public bool enabled;

		public AssetUtilizzaElement(string name, int depth, int id) : base(name, depth, id)
		{
			intValue1 = (int)(Random.value * 100);
			floatValue2 = Random.value;
			floatValue3 = Random.value;
			enabled = true;
		}
	}

}
