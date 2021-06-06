//#define EnableMessageBoxEditorExamples

using Extenity.DesignPatternsToolbox;
using UnityEngine;

namespace Extenity.UIToolbox
{

	/// <summary>
	/// Quick way to show message box in application. It's just a wrapper for standard type
	/// of dialogs.
	///
	/// Most of the time the application will need to show dialogs in front of everything
	/// and prevent any other user interaction, which is what modal dialogs do. There is also
	/// modeless dialog type if the dialog is not intended to be the only focus point right at
	/// the moment.
	/// 
	/// It's okay to show more than one dialog at the same time. They will show up on top of
	/// each other. Though you may need to be careful if these are modeless dialogs because
	/// the user will be able to press the button of a previously shown dialog that is now
	/// left behind at the back of currently shown dialog.
	///
	/// Example usage:
	///    MessageBox.NewModal.Show(title, message, ...)
	///    MessageBox.NewModeless.Show(title, message, ...)
	/// </summary>
	public class MessageBox : SingletonUnity<MessageBox>
	{
		#region Initialization

		protected void Awake()
		{
			InitializeSingleton();
		}

		#endregion

		#region Dialog Instantiation

		[SerializeField]
		private MessageBoxDialog ModelessDialogPrefab = default;
		[SerializeField]
		private MessageBoxDialog ModalDialogPrefab = default;

		/// <summary>
		/// Instantiates a new modeless dialog.
		/// </summary>
		public static MessageBoxDialog NewModeless
		{
			get
			{
				return Instantiate(Instance.ModelessDialogPrefab);
			}
		}

		/// <summary>
		/// Instantiates a new modal dialog with blackout in background that prevents any
		/// user interaction outside of the dialog.
		/// </summary>
		public static MessageBoxDialog NewModal
		{
			get
			{
				return Instantiate(Instance.ModalDialogPrefab);
			}
		}

		#endregion

		#region Examples

#if EnableMessageBoxEditorExamples

		[UnityEditor.MenuItem("Example/MessageBox/" + nameof(ShowMessageBox_Modal))]
		public static void ShowMessageBox_Modal()
		{
			MessageBox.NewModal.Show("The Title", "Something went wrong but we don't know what. Frankly, we don't give a damn.",
				"OKAY THEN", null,
				() => Log.Info("'Okay' pressed."),
				() => Log.Info("'Cancel' pressed."));
		}

		[UnityEditor.MenuItem("Example/MessageBox/" + nameof(ShowMessageBox_Modeless))]
		public static void ShowMessageBox_Modeless()
		{
			MessageBox.NewModeless.Show("The Title", "Something went wrong but we don't know what. Frankly, we don't give a damn.",
				"OKAY THEN", null,
				() => Log.Info("'Okay' pressed."),
				() => Log.Info("'Cancel' pressed."));
		}

#endif

		#endregion
	}

}
