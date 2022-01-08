namespace Extenity.DataToolbox
{

	public static class GuidTools
	{
		public static System.Guid ToGuid(this byte[] bytes)
		{
			return new System.Guid(bytes);
		}

		#region Create Random

		/// <summary>
		/// Generates a guid that depends on UnityEngine.Random's seed. Makes sure the generated guid is always the same if you give it the same seed. 
		/// Note that this method of generating a guid is not reliable to be unique. The randomness properties of the guid is actually the same as the 
		/// UnityEngine.Random (probably 32 bit seed).
		/// </summary>
		public static System.Guid NewPseudoGuid()
		{
			var data = new byte[16];
			for (int i = 0; i < 16; i++)
			{
				data[i] = (byte)(UnityEngine.Random.value * 256f);
			}
			return new System.Guid(data);
		}

		#endregion
	}

}
