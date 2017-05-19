#if !UNITY_WEBPLAYER
using System.Xml.Linq;
using System.Xml.XPath;

namespace Extenity.XDocumentToolbox
{

	public static class XDocumentTools
	{
		#region Evaluate MultiWay

		public static string XPathEvaluateStringMultiWay(this XElement element, string key, string defaultValue = "")
		{
			var expression = string.Format("string({0}/@Value | @{0})", key);
			try
			{
				return (string)element.XPathEvaluate(expression);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static bool XPathEvaluateBoolMultiWay(this XElement element, string key, bool defaultValue = false)
		{
			var expression1 = string.Format("boolean('true'={0}/@Value)", key);
			var expression2 = string.Format("boolean('true'=@{0})", key);
			try
			{
				return (bool)element.XPathEvaluate(expression1) | (bool)element.XPathEvaluate(expression2);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static int XPathEvaluateIntMultiWay(this XElement element, string key, int defaultValue = 0)
		{
			var expression = string.Format("number({0}/@Value | @{0})", key);
			try
			{
				return (int)(double)element.XPathEvaluate(expression);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static float XPathEvaluateFloatMultiWay(this XElement element, string key, float defaultValue = float.NaN)
		{
			var expression = string.Format("number({0}/@Value | @{0})", key);
			try
			{
				return (float)(double)element.XPathEvaluate(expression);
			}
			catch
			{
				return defaultValue;
			}
		}

		public static double XPathEvaluateDoubleMultiWay(this XElement element, string key, double defaultValue = double.NaN)
		{
			var expression = string.Format("number({0}/@Value | @{0})", key);
			try
			{
				return (double)element.XPathEvaluate(expression);
			}
			catch
			{
				return defaultValue;
			}
		}

		#endregion
	}

}

#endif
