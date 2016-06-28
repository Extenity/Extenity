using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdvancedInspector;

namespace SmartData
{

	[AdvancedInspector(true, false)]
	public class SmartVarManager : MonoBehaviour
	{
		#region Singleton

		public static SmartVarManager Instance { get; private set; }

		protected void Awake()
		{
			Instance = this;
		}

		#endregion

		#region Update

		protected void FixedUpdate()
		{
			for (int i = SmartVars.Count - 1; i >= 0; i--)
			{
				var variable = SmartVars[i].Target as SmartVar;
				if (variable != null)
				{
					if (variable.IsReferenceAlive)
					{
						if (variable.CheckForChangesAutomatically)
						{
							variable.CheckForChanges();
						}
					}
					else
					{
						variable._Deregister();
					}
				}
				else
				{
					Debug.Log("##### removing smart var");

					// Remove from list since it doesn't exist anymore
					SmartVars.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Smart Vars

		[Inspect, RuntimeResolve]
		internal List<WeakReference> SmartVars = new List<WeakReference>(10000);

		[Inspect]
		public int SmartVarCount { get { return SmartVars.Count; } }

		#endregion

		#region Register / Deregister Smart Var

		internal void _Register(SmartVar smartVar)
		{
			//Debug.Log("Registering SmartVar");

			if (smartVar == null)
				throw new NullReferenceException("smartVar");

			if (_IsRegistered(smartVar))
			{
				Debug.LogError("Smart var was already registered.");
				return;
			}

			SmartVars.Add(new WeakReference(smartVar));
		}

		internal void _Deregister(SmartVar smartVar)
		{
			//Debug.Log("DEregistering SmartVar");

			if (smartVar == null)
				throw new NullReferenceException("smartVar");

			for (int i = 0; i < SmartVars.Count; i++)
			{
				var obj = SmartVars[i].Target as SmartVar;
				if (obj == smartVar)
				{
					SmartVars.RemoveAt(i);
					return;
				}
			}

			Debug.LogError("Smart var was not registered.");
		}

		internal bool _IsRegistered(SmartVar smartVar)
		{
			for (int i = 0; i < SmartVars.Count; i++)
			{
				var obj = SmartVars[i].Target as SmartVar;
				if (obj == smartVar)
				{
					return true;
				}
			}
			return false;
		}

		#endregion
	}

}
