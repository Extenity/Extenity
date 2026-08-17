using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Extenity.JsonToolbox
{

	public class ContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			property.Writable = CanSetMemberValue(member);
			return property;
		}

		/// <summary>
		/// Allows deserializing into non-public fields and properties with non-public setters.
		/// </summary>
		static bool CanSetMemberValue(MemberInfo member)
		{
			return member switch
			{
				FieldInfo => true,
				PropertyInfo propertyInfo => propertyInfo.CanWrite,
				_ => false
			};
		}
	}

}