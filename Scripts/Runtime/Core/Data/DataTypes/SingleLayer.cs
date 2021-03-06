#if UNITY

using System;
using UnityEngine;

namespace Extenity.DataToolbox
{

	[Serializable]
	public class SingleLayer
	{
		[SerializeField]
		private int _LayerIndex = 0;

		public int LayerIndex
		{
			get => _LayerIndex;
			set
			{
				if (value < 0 || value > 31)
					throw new ArgumentOutOfRangeException(nameof(value), value, "Layer index should be value=>0 and value<=31");
				_LayerIndex = value;
			}
		}

		public int Mask => 1 << _LayerIndex;

		public string Name => LayerMask.LayerToName(LayerIndex);
	}

}

#endif
