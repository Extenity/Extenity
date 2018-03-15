using System;
using Extenity.DataToolbox;
using Extenity.IMGUIToolbox.Editor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Extenity.PainkillaTool.Editor
{

	[Serializable]
	public class AssetUtilizzaElement : TreeElement
	{
		// TODO: Delete these
		public int intValue1;
		public float floatValue2;

		#region Initialization

		public AssetUtilizzaElement(Material material, string sceneName) : base(material.name, 0, material.GetInstanceID())
		{
			intValue1 = (int)(Random.value * 100);
			floatValue2 = Random.value;

			Material = material;
			FoundInScenes = new[] { sceneName };
		}

		private AssetUtilizzaElement() : base(null, -1, 0)
		{
		}

		public static AssetUtilizzaElement CreateRoot()
		{
			return new AssetUtilizzaElement();
		}

		#endregion

		#region Material

		public Material Material;

		#endregion

		#region Found In Scenes

		public string[] FoundInScenes;

		public void AddScene(string sceneName)
		{
			FoundInScenes = FoundInScenes.Add(sceneName);
		}

		#endregion
	}

}
