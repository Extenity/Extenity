using Extenity.UnityEditorToolbox;
using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;
using Logger = Extenity.Logger;

namespace ExtenityExamples.UnityEditorToolbox.Editor
{

	public static class Example_EditorMessageBox
	{
		[MenuItem(ExtenityMenu.Examples + "EditorMessageBox/MessageBox Without Cancel Button", priority = ExtenityMenu.ExamplesPriority + 1)]
		private static void MessageBox1()
		{
			EditorMessageBox.Show(new Rect(100, 100, 300, 200), "Title here!", "This is an example message box. Use it with care.", "Got it", "",
				() =>
				{
					Log.Info("Message box closed.");
				}
			);
		}

		[MenuItem(ExtenityMenu.Examples + "EditorMessageBox/MessageBox With Cancel Button", priority = ExtenityMenu.ExamplesPriority + 2)]
		private static void MessageBox2()
		{
			EditorMessageBox.Show(new Rect(100, 100, 300, 200),
				"Title here!",
				"This is an example message box. Use it with care.",
				"Got it", "Naah",
				() =>
				{
					Log.Info("Message box approved.");
				},
				() =>
				{
					Log.Info("Message box denied.");
				}
			);
		}

		[MenuItem(ExtenityMenu.Examples + "EditorMessageBox/MessageBox With Single User Input", priority = ExtenityMenu.ExamplesPriority + 3)]
		private static void MessageBox3()
		{
			var userInputs = new[]
			{
				new UserInputField("What you got?", "Nothing")
			};

			EditorMessageBox.Show(new Rect(100, 100, 300, 200),
				"Title here!",
				"This is an example message box. Use it with care.",
				userInputs,
				"Got it", "Naah",
				() =>
				{
					Log.Info("Message box approved. Value: " + userInputs[0].Value);
				},
				() =>
				{
					Log.Info("Message box denied. Value: " + userInputs[0].Value);
				}
			);
		}

		[MenuItem(ExtenityMenu.Examples + "EditorMessageBox/MessageBox Double User Inputs", priority = ExtenityMenu.ExamplesPriority + 4)]
		private static void MessageBox4()
		{
			var userInputs = new[]
			{
				new UserInputField("How many apples?"),
				new UserInputField("How many oranges?")
			};

			EditorMessageBox.Show(new Rect(100, 100, 300, 200),
				"Title here!",
				"This is an example message box. Use it with care.",
				userInputs,
				"Got it", "Naah",
				() =>
				{
					Debug.LogFormat("Message box approved. Apples: '{0}', Oranges: '{1}'", userInputs[0].Value, userInputs[1].Value);
				},
				() =>
				{
					Debug.LogFormat("Message box denied. Apples: '{0}', Oranges: '{1}'", userInputs[0].Value, userInputs[1].Value);
				}
			);
		}

		#region Log

		private static readonly Logger Log = new("Example");

		#endregion
	}

}
