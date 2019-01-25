using Extenity.DesignPatternsToolbox;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class MessageBox : SingletonUnity<MessageBox>
	{
		#region Initialization

		protected void Awake()
		{
			InitializeSingleton(true);
		}

		#endregion

		#region Deinitialization

		protected override void OnDestroy()
		{
			if (Dialog)
			{
				Dialog.Close();
			}

			base.OnDestroy();
		}

		#endregion

		#region Dialog

		[SerializeField]
		private MessageBoxDialog _Dialog;

		public static MessageBoxDialog Dialog => Instance._Dialog;

		#endregion
	}

}
