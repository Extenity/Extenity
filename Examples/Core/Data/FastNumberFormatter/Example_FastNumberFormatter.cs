using Extenity.DataToolbox;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;

namespace ExtenityExamples.DataToolbox
{

	public class Example_FastNumberFormatter : MonoBehaviour
	{
		public TextMeshProUGUI Text_C;
		public TextMeshProUGUI Text_D;
		public TextMeshProUGUI Text_E;
		public TextMeshProUGUI Text_F;
		public TextMeshProUGUI Text_G;
		public TextMeshProUGUI Text_X;
		public TextMeshProUGUI Text_P;
		public TextMeshProUGUI Text_N;
		public TextMeshProUGUI Text_R;
		public TextMeshProUGUI Text_Null;

		char[] chars = new char[1000];
		float counter = 15.4f;

		private void Update()
		{
			counter += 0.1572f;

			ConvertAndWrite("C5", (int)counter, Text_C);
			ConvertAndWrite("D5", (int)counter, Text_D);
			ConvertAndWrite("E2", counter, Text_E);
			ConvertAndWrite("F4", counter, Text_F);
			ConvertAndWrite("G2", counter, Text_G);
			ConvertAndWrite("X8", (int)counter, Text_X);
			ConvertAndWrite("P2", counter, Text_P);
			ConvertAndWrite("N2", counter, Text_N);
			ConvertAndWrite("R", counter, Text_R);
			ConvertAndWrite(null, counter, Text_Null);
		}

		private void ConvertAndWrite(string format, float value, TextMeshProUGUI ui)
		{
			Profiler.BeginSample(format != null ? format : "null");

			//var length = FastNumberFormatter.NumberToString(format, value, NumberFormatInfo.CurrentInfo, chars);
			var length = value.ToStringAsCharArray(format, chars);
			ui.SetCharArray(chars, 0, length);

			Profiler.EndSample();
		}

		private void ConvertAndWrite(string format, int value, TextMeshProUGUI ui)
		{
			Profiler.BeginSample(format);

			//var length = FastNumberFormatter.NumberToString(format, value, NumberFormatInfo.CurrentInfo, chars);
			var length = value.ToStringAsCharArray(format, chars);
			ui.SetCharArray(chars, 0, length);

			Profiler.EndSample();
		}
	}

}
