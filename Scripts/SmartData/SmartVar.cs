using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdvancedInspector;
using UnityEngine.Events;

namespace SmartData
{

	[Serializable]
	public abstract class SmartVar
	{
		#region Initialization / Deinitialization

		public SmartVar()
		{
			if (Application.isPlaying) // Don't try to create variable in editor mode.
			{
				_Register();
			}
		}

		public SmartVar(bool checkForChangesAutomatically)
			: base()
		{
			CheckForChangesAutomatically = checkForChangesAutomatically;
		}

		~SmartVar()
		{
			//if (Application.isPlaying) // Don't try to remove variable in editor mode.
			{
				_Deregister();
			}
		}

		#endregion

		#region Register To Manager

		protected bool IsRegistered { get; private set; }

		internal void _Register()
		{
			if (IsRegistered)
				return;

			SmartVarManager._Register(this);
			IsRegistered = true;
		}

		internal void _Deregister()
		{
			if (!IsRegistered)
				return;

			if (SmartVarManager.IsQuittingApplication)
				return;

			// Last check before deregistering from manager. This is needed because manager won't be able to send any more messages so we would miss the messages where all values gets NaN.
			CheckForChanges();

			SmartVarManager._Deregister(this);
			IsRegistered = false;
		}

		#endregion

		#region Reference

		public abstract bool IsReferenceAlive { get; }

		#endregion

		#region Tag

		public object Tag;

		#endregion

		#region Check For Changes

		public bool CheckForChangesAutomatically { get; set; }

		public abstract void CheckForChanges();

		#endregion
	}

	[Serializable]
	//[AdvancedInspector(false, false)]
	public abstract class SmartVar<TReference> : SmartVar where TReference : UnityEngine.Component
	{
		#region Initialization

		public SmartVar() : base() { }
		public SmartVar(bool checkForChangesAutomatically) : base(checkForChangesAutomatically) { }

		#endregion

		#region Reference To Data

		[SerializeField]
		private TReference _Reference;
		[Inspect(priority: 2)]
		public TReference Reference
		{
			get
			{
				return _Reference;
			}
			set
			{
				if (_Reference == value)
					return;

				if (_Reference == null && value != null)
				{
					_Register();
				}

				_Reference = value;

				if (value == null)
				{
					_Deregister();
				}

				OnReferenceChanged.Invoke(_Reference, Tag);
			}
		}

		public override bool IsReferenceAlive { get { return _Reference != null /*&& _Reference.gameObject != null*/; } }

		#endregion

		#region Events

		/// <summary>
		/// Parameter 'TReference' reference: Reference to the object that this SmartVar gets it's values from.
		/// Parameter 'object' tag: Value of user defined Tag of this SmartVar.
		/// </summary>
		[Serializable]
		public class SmartVarEvent : UnityEvent<TReference, object> { }

		[Inspect, Group("Events", priority: 10)]
		public SmartVarEvent OnReferenceChanged = new SmartVarEvent();

		#endregion
	}

}
