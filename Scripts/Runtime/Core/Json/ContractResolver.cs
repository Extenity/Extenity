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
			property.Writable = CanSetMemberValue(member, true);
			return property;
		}

		static bool CanSetMemberValue(MemberInfo member, bool nonPublic)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					var fieldInfo = (FieldInfo)member;
					return nonPublic || fieldInfo.IsPublic;

				case MemberTypes.Property:
					var propertyInfo = (PropertyInfo)member;

					if (!propertyInfo.CanWrite)
						return false;
					if (nonPublic)
						return true;
					return propertyInfo.GetSetMethod(nonPublic) != null;

				default:
					return false;
			}
		}
	}

}
