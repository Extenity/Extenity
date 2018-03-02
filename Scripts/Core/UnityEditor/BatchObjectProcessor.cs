using System;
using UnityEngine;

namespace Extenity.UnityEditorToolbox
{

	[Serializable]
	public class BatchObjectProcessorSelection
	{
		public GameObject Object;
		public bool IncludeChildren;
	}

	[Serializable]
	public class BatchObjectProcessorEntry
	{
		public string Configuration;
		public BatchObjectProcessorSelection[] Objects;
	}

	[Serializable]
	public class BatchObjectProcessorConfiguration
	{
		public string Name;

		[Header("Change Layers")]
		public bool ChangeLayers = false;
		public LayerMask Layer;

		[Header("Change Tags")]
		public bool ChangeTags = false;
		public string Tag;

		[Header("Change Static")]
		public bool ChangeStatic = false;
		[EnumMask]
		public StaticFlags StaticFlags;

		[Header("Change Navigation")]
		public bool ChangeNavMeshArea = false;
		public int AreaIndex = -1;
	}

	public class BatchObjectProcessor : MonoBehaviour
	{
		#region Data

		public BatchObjectProcessorConfiguration[] Configurations;
		public BatchObjectProcessorEntry[] Entries;

		public BatchObjectProcessorConfiguration GetConfiguration(string name)
		{
			for (var i = 0; i < Configurations.Length; i++)
			{
				if (Configurations[i].Name == name)
					return Configurations[i];
			}
			throw new Exception(string.Format("Batch object configuration '{0}' does not exist.", name));
		}

		#endregion
	}

}
