using System;
using System.Reflection;

// TODO: Not tested yet. Can be heavily optimized.

namespace Extenity.Primitives.Bitfield
{

	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
	public sealed class BitfieldLengthAttribute : Attribute
	{
		private uint length;

		public BitfieldLengthAttribute(uint length)
		{
			this.length = length;
		}

		public uint Length { get { return length; } }
	}

	public interface IBitfield
	{
	}

	public static class BitfieldHelper
	{
		public static long ToLong(this IBitfield obj)
		{
			long result = 0;
			int offset = 0;

			var fieldInfos = obj.GetType().GetFields();
			Array.Sort(fieldInfos, delegate(FieldInfo first, FieldInfo second)
				{
					return first.FieldHandle.Value.ToInt32().CompareTo(second.FieldHandle.Value.ToInt32());
				});

			// For every field suitably attributed with a BitfieldLength
			for (int iFieldInfo = 0; iFieldInfo < fieldInfos.Length; iFieldInfo++)
			{
				FieldInfo fieldInfo = fieldInfos[iFieldInfo];
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof (BitfieldLengthAttribute), false);
				if (customAttributes.Length == 1)
				{
					uint fieldLength = ((BitfieldLengthAttribute) customAttributes[0]).Length;

					// Calculate a bitmask of the desired length
					long mask = 0;
					for (int i = 0; i < fieldLength; i++)
#pragma warning disable 675
						mask |= 1 << i;
#pragma warning restore 675

					result |= ((UInt32) fieldInfo.GetValue(obj) & mask) << offset;

					offset += (int) fieldLength;
				}
			}

			return result;
		}
	}

}
