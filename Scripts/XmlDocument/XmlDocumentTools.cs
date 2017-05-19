using System;
using System.Globalization;
using System.Xml;
using UnityEngine;

namespace Extenity.XmlDocumentToolbox
{

	public static class XmlDocumentTools
	{
		public static Vector2 GetAttributeVector2(this XmlNode me, string xTag = "X", string yTag = "Y")
		{
			return new Vector2(
					float.Parse(me.GetAttributeEnsured(xTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(yTag), CultureInfo.InvariantCulture)
					);

		}

		public static Vector2 GetAttributeVector2(this XmlNode me, float changeNaNsWith, string xTag = "X", string yTag = "Y")
		{
			var value = me.GetAttributeVector2(xTag, yTag);
			if (float.IsNaN(value.x)) value.x = changeNaNsWith;
			if (float.IsNaN(value.y)) value.y = changeNaNsWith;
			return value;
		}

		public static Vector3 GetAttributeVector3(this XmlNode me, string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			return new Vector3(
					float.Parse(me.GetAttributeEnsured(xTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(yTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(zTag), CultureInfo.InvariantCulture)
					);

		}

		public static Vector3 GetAttributeVector3(this XmlNode me, float changeNaNsWith, string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			var value = me.GetAttributeVector3(xTag, yTag, zTag);
			if (float.IsNaN(value.x)) value.x = changeNaNsWith;
			if (float.IsNaN(value.y)) value.y = changeNaNsWith;
			if (float.IsNaN(value.z)) value.z = changeNaNsWith;
			return value;
		}

		public static Quaternion GetAttributeQuaternion(this XmlNode me, string wTag = "W", string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			return new Quaternion(
					float.Parse(me.GetAttributeEnsured(xTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(yTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(zTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(wTag), CultureInfo.InvariantCulture)
					);

		}

		public static Quaternion GetAttributeQuaternion(this XmlNode me, Quaternion changeNaNsWith, string wTag = "W", string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			var value = me.GetAttributeQuaternion(wTag, xTag, yTag, zTag);
			if (float.IsNaN(value.w) || float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)
				|| (value.w == 0 && value.x == 0 && value.y == 0 && value.z == 0))
			{
				value = changeNaNsWith;
			}
			return value;
		}

		public static Vector2 GetAttributeAtPathVector2(this XmlNode me, string attributePath, string xTag = "X", string yTag = "Y")
		{
			me = me.SelectSingleNode(attributePath);
			return new Vector2(
					float.Parse(me.GetAttributeEnsured(xTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(yTag), CultureInfo.InvariantCulture)
					);

		}

		public static Vector2 GetAttributeAtPathVector2(this XmlNode me, string attributePath, float changeNaNsWith, string xTag = "X", string yTag = "Y")
		{
			var value = me.GetAttributeAtPathVector2(attributePath, xTag, yTag);
			if (float.IsNaN(value.x)) value.x = changeNaNsWith;
			if (float.IsNaN(value.y)) value.y = changeNaNsWith;
			return value;
		}

		public static Vector3 GetAttributeAtPathVector3(this XmlNode me, string attributePath, string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			me = me.SelectSingleNode(attributePath);
			return new Vector3(
					float.Parse(me.GetAttributeEnsured(xTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(yTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(zTag), CultureInfo.InvariantCulture)
					);

		}

		public static Vector3 GetAttributeAtPathVector3(this XmlNode me, string attributePath, float changeNaNsWith, string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			var value = me.GetAttributeAtPathVector3(attributePath, xTag, yTag, zTag);
			if (float.IsNaN(value.x)) value.x = changeNaNsWith;
			if (float.IsNaN(value.y)) value.y = changeNaNsWith;
			if (float.IsNaN(value.z)) value.z = changeNaNsWith;
			return value;
		}

		public static Quaternion GetAttributeAtPathQuaternion(this XmlNode me, string attributePath, string wTag = "W", string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			me = me.SelectSingleNode(attributePath);
			return new Quaternion(
					float.Parse(me.GetAttributeEnsured(xTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(yTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(zTag), CultureInfo.InvariantCulture),
					float.Parse(me.GetAttributeEnsured(wTag), CultureInfo.InvariantCulture)
					);

		}

		public static Quaternion GetAttributeAtPathQuaternion(this XmlNode me, string attributePath, Quaternion changeNaNsWith, string wTag = "W", string xTag = "X", string yTag = "Y", string zTag = "Z")
		{
			var value = me.GetAttributeAtPathQuaternion(attributePath, wTag, xTag, yTag, zTag);
			if (float.IsNaN(value.w) || float.IsNaN(value.x) || float.IsNaN(value.y) || float.IsNaN(value.z)
				|| (value.w == 0 && value.x == 0 && value.y == 0 && value.z == 0))
			{
				value = changeNaNsWith;
			}
			return value;
		}

		#region GetAttributeEnsured

		public static string GetAttributeEnsured(this XmlNode me, string name)
		{
			var value = me.Attributes[name];
			if (value == null)
				throw new Exception("Could not find attribute '" + name + "' in node '" + me.Name + "'.");
			return value.InnerText;
		}

		#endregion
	}

}
