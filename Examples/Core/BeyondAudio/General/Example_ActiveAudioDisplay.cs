using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExtenityExamples.BeyondAudio
{

	public class Example_ActiveAudioDisplay : MonoBehaviour
	{
		public Example_ActiveAudioDisplayItem Template;
		private RectTransform Container;
		private List<Example_ActiveAudioDisplayItem> Items = new List<Example_ActiveAudioDisplayItem>(100);

		private void Start()
		{
			Template.gameObject.SetActive(false);
			Container = Template.transform.parent.GetComponent<RectTransform>();

#if BeyondAudioUsesUnityAudio
			AudioManager.Instance.OnAllocatedAudioSource.AddListener(OnAllocatedAudioSource);
			AudioManager.Instance.OnReleasingAudioSource.AddListener(OnReleasingAudioSource);
#else
			throw new System.NotImplementedException();
#endif
		}

#if BeyondAudioUsesUnityAudio

		private void OnAllocatedAudioSource(AudioSource audioSource, string eventName)
		{
			Debug.LogFormat(audioSource.gameObject, "Allocating audio source '{0}' for event '{1}'", audioSource, eventName);

			var item = Instantiate(Template.gameObject, Container).GetComponent<Example_ActiveAudioDisplayItem>();
			item.transform.localScale = Vector3.one;
			item.Set(audioSource, eventName);
			item.gameObject.SetActive(true);
			Items.Add(item);

			LayoutRebuilder.ForceRebuildLayoutImmediate(Container);
		}

		private void OnReleasingAudioSource(AudioSource audioSource)
		{
			Debug.LogFormat(audioSource.gameObject, "Releasing audio source '{0}'", audioSource);

			var count = 0;

			foreach (var item in Items)
			{
				if (!item.IsReleased && item.AudioSource == audioSource)
				{
					item.InformReleased();
					count++;
				}
			}

			if (count == 0)
				Debug.Log("Internal error! No UI item found.");
			else if (count > 1)
				Debug.Log("Internal error! More than one UI item found.");
		}

#endif
	}

}
