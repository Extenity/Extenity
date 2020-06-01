using UnityEngine;

namespace Extenity.Kernel
{

	public class VersioningHandler : MonoBehaviour
	{
		// TODO: Ability to select where Versioning event emitting should happen as per project requirement (Update, FixedUpdate, LateUpdate). Do that by just adding Loop.GetCallbacks(someEnum) method. And add "Do not modify it on runtime" in documentation as it will not work properly.

		protected void Awake()
		{
			Loop.UpdateCallbacks.AddListener(CustomUpdate);
		}

		protected void OnDestroy()
		{
			Loop.UpdateCallbacks.RemoveListener(CustomUpdate);
		}

		private void CustomUpdate()
		{
			Versioning.EmitEventsInQueue();
		}
	}

}
