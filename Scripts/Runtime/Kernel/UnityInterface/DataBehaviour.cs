using System.Collections.Generic;
using UnityEngine;

namespace Extenity.Kernel.UnityInterface
{

	// TODO: Implement this.
	// [EnsureDerivedTypesWontUseMethod(nameof(Awake), nameof(AwakeDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnEnable), nameof(OnEnableDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDestroy), nameof(OnDestroyDerived))]
	// [EnsureDerivedTypesWontUseMethod(nameof(OnDisable), nameof(OnDisableDerived))]
	public class DataBehaviour : MonoBehaviour
	{
		#region Initialization

		protected virtual void AwakeDerived() { }
		protected virtual void OnEnableDerived() { }

		protected virtual void Awake()
		{
			AllDataBehaviours.Add(this);
			AwakeDerived();
		}

		protected virtual void OnEnable()
		{
			AllActiveDataBehaviours.Add(this);
			OnEnableDerived();
		}

		#endregion

		#region Deinitialization

		protected virtual void OnDestroyDerived() { }
		protected virtual void OnDisableDerived() { }

		protected virtual void OnDestroy()
		{
			AllDataBehaviours.Remove(this);
			OnDestroyDerived();
		}

		protected virtual void OnDisable()
		{
			AllActiveDataBehaviours.Remove(this);
			OnDisableDerived();
		}

		#endregion

		#region All DataBehaviours

		public static readonly List<DataBehaviour> AllDataBehaviours = new List<DataBehaviour>(50);
		public static readonly List<DataBehaviour> AllActiveDataBehaviours = new List<DataBehaviour>(50);

		#endregion
	}

}
