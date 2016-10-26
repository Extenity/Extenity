using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AdvancedInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SmartData
{

	[AdvancedInspector(true, false)]
	public class SmartVarManager : MonoBehaviour
	{
		#region Initialization

#if UNITY_EDITOR
		[InitializeOnLoadMethod]
		private static void ClearOnEditorModeChanges()
		{
			SmartVars.Clear();

			// That didn't work out well since playmodeStateChanged invoked too late (after Awake)
			//EditorApplication.playmodeStateChanged += PlaymodeStateChanged;
		}

		//private static void PlaymodeStateChanged()
		//{
		//	SmartVars.Clear();
		//}
#endif

		#endregion

		#region Deinitialization

		public static bool IsQuittingApplication;

		protected void OnApplicationQuit()
		{
			IsQuittingApplication = true;
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
		internal static List<WeakReference> SmartVars = new List<WeakReference>(10000);

		[Inspect]
		public static int SmartVarCount { get { return SmartVars.Count; } }

		#endregion

		#region Register / Deregister Smart Var

		internal static void _Register(SmartVar smartVar)
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

		internal static void _Deregister(SmartVar smartVar)
		{
			//Debug.Log("Deregistering SmartVar");

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

		internal static bool _IsRegistered(SmartVar smartVar)
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
