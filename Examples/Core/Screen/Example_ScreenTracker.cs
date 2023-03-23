using Extenity.ScreenToolbox;
using Extenity.UnityEditorToolbox.Editor;
using Sirenix.OdinInspector;
using UnityEngine;
using Logger = Extenity.Logger;

namespace ExtenityExamples.ScreenToolbox
{

	// [ExecuteInEditMode] // See 11653662.
	public class Example_ScreenTracker : MonoBehaviour
	{
		[ShowInInspector, InlineProperty, HideLabel]
		public ScreenInfo ScreenInfo => ScreenTracker.Info;

		private void OnEnable()
		{
			ScreenTracker.OnScreenModified.AddListener(OnScreenModified);
		}

		private void OnDisable()
		{
			ScreenTracker.OnScreenModified.RemoveListener(OnScreenModified);
		}

		private void OnScreenModified(ScreenInfo screenInfo)
		{
			Log.Info(screenInfo.ToString());
			EditorWindowTools.RepaintAllViews(); // Refresh the inspector
		}

		#region Log

		private static readonly Logger Log = new("Example");

		#endregion
	}

}
