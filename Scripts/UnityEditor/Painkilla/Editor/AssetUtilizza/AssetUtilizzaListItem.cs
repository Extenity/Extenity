using System;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Extenity.PainkillaTool.Editor
{

	[Serializable]
	public class AssetUtilizzaListItem : TreeViewItem
	{
		public float floatValue1, floatValue2, floatValue3;
		public Material material;
		public string text = "";
		public bool enabled = true;

		public AssetUtilizzaListItem(int id, int depth, string name) : base(id, depth, name)
		{
			floatValue1 = UnityEngine.Random.value;
			floatValue2 = UnityEngine.Random.value;
			floatValue3 = UnityEngine.Random.value;
		}

		//override 
	}

}
