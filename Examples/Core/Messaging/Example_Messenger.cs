#if ExtenityMessenger && !UseLegacyMessenger

using UnityEngine;
using Extenity.MessagingToolbox;

namespace Extenity.Messaging
{

	[RequireComponent(typeof(Messenger))]
	public class Example_Messenger : MonoBehaviour
	{
		public class CustomType
		{
			public int Integro;

			public void SomeListenerWhichIsNotPartOfAUnityObject()
			{
				Log.Warning("This method should not be called from messenger.");
			}
		}

		protected void Awake()
		{
			Log.Info("Press F12 to list all registered listeners.");

			Log.Info("-------------  Adding message id 101");
			Messenger.AddMessageListener("101", Method_1_NoReturn_NoParam);
			Messenger.AddMessageListener("101", Method_2_NoReturn_NoParam);
			//Messenger.AddListener("101", Method_NoReturn_1Param_String); // Logs error at runtime but continues execution. We have already added a method with another parameter structure for this messageId.
			//Messenger.AddListener("101", Method_IntReturn_NoParam); // Compile time error. Return type should be void.

			Log.Info("-------------  Adding message id 201");
			Messenger.AddMessageListener("201", Method_NoReturn_1Param_String);

			Log.Info("-------------  Adding message id 301");
			//Messenger.AddListener("301", Method_NoReturn_1Param_CustomType); // Compile time error. Need to specify CustomType.
			//Messenger.AddListener<string>("301", Method_NoReturn_1Param_CustomType); // Compile time error. Mismatched input parameter types.
			Messenger.AddMessageListener<CustomType>("301", Method_NoReturn_1Param_CustomType);

			Log.Info("-------------  Adding message id 401");
			//Messenger.AddListener("401", Method_NoReturn_2Params_Int_String); // Compile time error. Need to specify input parameter types if method has more than one parameter.
			//Messenger.AddListener<string, int>("401", Method_NoReturn_2Params_Int_String); // Compile time error. Careful with parameter order.
			Messenger.AddMessageListener<int, string>("401", Method_NoReturn_2Params_Int_String);

			//var customObject = new CustomType();
			//Messenger.AddListener("12572", customObject.SomeListenerWhichIsNotPartOfAUnityObject); // Logs error at runtime but continues execution. CustomType is not a Unity object so we cannot add it's method as a listener.

			//Messenger.AddListener("92752", Method_NoReturn_1Param_OutInt); // Compile time error. Parameter 'out' and 'ref' statements are not supported.
			//Messenger.AddListener("73501", Method_NoReturn_1Param_RefInt); // Compile time error. Parameter 'out' and 'ref' statements are not supported.


			Log.Info("-------------  Emitting message id 101 with no parameter");
			Messenger.EmitMessage("101");
			Log.Info("-------------  Emitting message id 101 with string parameter (should fail)");
			Messenger.EmitMessage("101", "hehee"); // Logs error at runtime but continues execution.
			Log.Info("-------------  Emitting message id 101 with int parameter (should fail)");
			Messenger.EmitMessage("101", 7693863); // Logs error at runtime but continues execution.

			Log.Info("-------------  Emitting message id 201 with no parameter (should fail)");
			Messenger.EmitMessage("201"); // Logs error at runtime but continues execution.
			Log.Info("-------------  Emitting message id 201 with string parameter");
			Messenger.EmitMessage("201", "hehee");
			Log.Info("-------------  Emitting message id 201 with int parameter (should fail)");
			Messenger.EmitMessage("201", 7693863); // Logs error at runtime but continues execution.

			Log.Info("-------------  Emitting message id 301 with custom parameter");
			Messenger.EmitMessage("301", new CustomType() {Integro = 151});
			Log.Info("-------------  Emitting message id 301 with int parameter (should fail)");
			Messenger.EmitMessage("301", 7693863); // Logs error at runtime but continues execution.

			Log.Info("-------------  Emitting message id 401 with no parameter (should fail)");
			Messenger.EmitMessage("401"); // Logs error at runtime but continues execution.
			Log.Info("-------------  Emitting message id 401 with int, string parameters");
			Messenger.EmitMessage("401", 474378, "hehe");
			Log.Info("-------------  Emitting message id 401 with string, int parameters (should fail)");
			Messenger.EmitMessage("401", "hehe", 474378); // Logs error at runtime but continues execution.
		}

		protected void Update()
		{
			if (Input.GetKeyDown(KeyCode.F12))
			{
				Messenger.DebugLogListAllMessageListeners();
			}
		}

		protected void Method_1_NoReturn_NoParam()
		{
			Log.Info("Method_1_NoReturn_NoParam");
		}

		protected void Method_2_NoReturn_NoParam()
		{
			Log.Info("Method_2_NoReturn_NoParam");
		}

		protected int Method_IntReturn_NoParam()
		{
			Log.Info("Method_IntReturn_NoParam");
			return 2323232;
		}

		protected void Method_NoReturn_1Param_String(string text)
		{
			Log.Info("Method_NoReturn_1Param_String    text: " + text);
		}

		protected void Method_NoReturn_1Param_OutInt(out int value)
		{
			value = 3;
			Log.Info("Method_NoReturn_1Param_OutInt    value: " + value);
		}

		protected void Method_NoReturn_1Param_RefInt(ref int value)
		{
			Log.Info("Method_NoReturn_1Param_RefInt    value: " + value);
			value = 3;
		}

		protected void Method_NoReturn_1Param_CustomType(CustomType data)
		{
			Log.Info("Method_NoReturn_1Param_CustomType    data.Integro: " + data.Integro);
		}

		protected void Method_NoReturn_2Params_Int_String(int integro, string text)
		{
			Log.Info("Method_NoReturn_2Params_Int_String   integro: " + integro + "     text: " + text);
		}

		protected void Method_NoReturn_2Params_String_Int(string text, int integro)
		{
			Log.Info("Method_NoReturn_2Params_String_Int   text: " + text + "     integro: " + integro);
		}

		#region Messenger

		public Messenger Messenger
		{
			get { return GetComponent<Messenger>(); }
		}

		#endregion
	}

}

#endif
