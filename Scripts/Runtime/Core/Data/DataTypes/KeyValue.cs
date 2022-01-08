using System;

namespace Extenity.DataToolbox
{

	[Serializable]
	public struct KeyValue<TKey, TValue>
	{
		#region Data

		public TKey Key;
		public TValue Value;

		#endregion

		#region Initialization

		public KeyValue(TKey key, TValue value)
		{
			Key = key;
			Value = value;
		}

		#endregion

		#region Conversions

		public override string ToString()
		{
			return Key + ": " + Value;
		}

		#endregion
	}

}
