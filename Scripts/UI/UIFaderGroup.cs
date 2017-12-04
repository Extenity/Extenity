using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public class UIFaderGroup : MonoBehaviour
	{
		#region Configuration

		[Header("Setup")]
		public List<UIFader> Faders;

		#endregion

		#region Initialization

		protected void Start()
		{
			RegisterFaders();
		}

		#endregion

		#region Deinitialization

		protected void OnDestroy()
		{
			DeregisterFaders();
		}

		#endregion

		#region Register / Deregister Faders

		private void RegisterFaders()
		{
			for (var i = 0; i < Faders.Count; i++)
			{
				InternalRegisterFader(Faders[i]);
			}
		}

		private void DeregisterFaders()
		{
			for (var i = 0; i < Faders.Count; i++)
			{
				InternalDeregisterFader(Faders[i]);
			}
		}

		private void InternalRegisterFader(UIFader fader)
		{
			if (!fader)
			{
				Debug.LogError("Tried to register a null fader.");
				return;
			}
			fader.OnFadeIn.AddListener(InternalOnFadeIn);

#if UNITY_EDITOR
			//_RegisteredFaders.Add(fader);
#endif
		}

		private void InternalDeregisterFader(UIFader fader)
		{
			if (fader)
				fader.OnFadeIn.RemoveListener(InternalOnFadeIn);
		}

		#endregion

		#region Operations

		private void InternalOnFadeIn(UIFader fader)
		{
			// Fade out all other faders.
			for (var i = 0; i < Faders.Count; i++)
			{
				var faderInList = Faders[i];
				if (faderInList != fader)
				{
					faderInList.FadeOut();
				}
			}
		}

		#endregion

		#region Validate

#if UNITY_EDITOR

		// This code is here to make it work if editor user decides to add/remove faders while in play mode.
		// But decided it's not worth the hassle.
		// The code is not tested yet.

		//private List<UIFader> _RegisteredFaders = new List<UIFader>();

		//private void OnValidate()
		//{
		//	if (!Application.isPlaying)
		//		return;

		//	// Check if Faders list is the same with RegisteredFaders list.
		//	// If not, this means we need to register to new faders and deregister from old faders.
		//	if (!_RegisteredFaders.ContentEquals(Faders, EqualityComparer<UIFader, UIFader>.Default))
		//	{
		//		// Quick solution is to deregister from old list of faders, and register into new list.
		//		// There will probably be the same items in both lists that we do unnecessary 
		//		// deregister +register. But since it's an editor only feature, we will choose simplicity.
		//		for (var i = 0; i < _RegisteredFaders.Count; i++)
		//		{
		//			InternalDeregisterFader(_RegisteredFaders[i]);
		//		}
		//		_RegisteredFaders.Clear();
		//		RegisterFaders();
		//	}
		//}

#endif

		#endregion
	}

}
