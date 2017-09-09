using Extenity.UnityEditorToolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace ExtenityExamples.UnityEditorToolbox.Editor
{

	public static class Example_EditorMessageBox
	{
		[MenuItem("Examples/EditorMessageBox/MessageBox Without Cancel Button")]
		private static void MessageBox1()
		{
			EditorMessageBox.Show(new Rect(100, 100, 300, 200), "Title here!", "This is an example message box. Use it with care.", "Got it", "",
				() =>
				{
					Debug.Log("Message box closed.");
				}
			);
		}

		[MenuItem("Examples/EditorMessageBox/MessageBox With Cancel Button")]
		private static void MessageBox2()
		{
			EditorMessageBox.Show(new Rect(100, 100, 300, 200),
				"Title here!",
				"This is an example message box. Use it with care.",
				"Got it", "Naah",
				() =>
				{
					Debug.Log("Message box approved.");
				},
				() =>
				{
					Debug.Log("Message box denied.");
				}
			);
		}

		[MenuItem("Examples/EditorMessageBox/MessageBox With Single User Input")]
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
					Debug.Log("Message box approved. Value: " + userInputs[0].Value);
				},
				() =>
				{
					Debug.Log("Message box denied. Value: " + userInputs[0].Value);
				}
			);
		}

		[MenuItem("Examples/EditorMessageBox/MessageBox Double User Inputs")]
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
	}

}
