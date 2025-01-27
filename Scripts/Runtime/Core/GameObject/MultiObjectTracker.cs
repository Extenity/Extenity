#if UNITY

using System;
using System.Collections.Generic;
using UnityEngine;
using Extenity.MathToolbox;

namespace Extenity.GameObjectToolbox
{

	public class MultiObjectTracker : MonoBehaviour
	{
		#region Initialization

		private void Awake()
		{
			ResetResults();
		}

		#endregion

		#region Update

		private void FixedUpdate()
		{
			if (objects == null || objects.Count == 0)
			{
				ResetResults();
				return;
			}

			SetResultsToZero();
			List<int> removeObjectsAt = null;

			switch (mode)
			{
				case Mode.Center:
					{
						for (int i = 0; i < objects.Count; i++)
						{
							var obj = InternalGetObjectAt(ref removeObjectsAt, i);
							if (obj == null)
								continue;

							// Calculations
							{
								center += obj.position;

								if (calculateBounds)
									bounds.Encapsulate(obj.position);
							}

							totalCalculatedObjects++;
						}

						center /= (float)totalCalculatedObjects;
					}
					break;
				case Mode.TotalVelocity:
					{
						for (int i = 0; i < objects.Count; i++)
						{
							var obj = InternalGetObjectAt(ref removeObjectsAt, i);
							if (obj == null)
								continue;

							// Calculations
							{
								var component = obj.GetComponent<Rigidbody>();
								if (component != null)
									totalVelocity += component.linearVelocity;

								if (calculateBounds)
									bounds.Encapsulate(obj.position);
							}

							totalCalculatedObjects++;
						}
					}
					break;
				case Mode.CenterAndTotalVelocity:
					{
						for (int i = 0; i < objects.Count; i++)
						{
							var obj = InternalGetObjectAt(ref removeObjectsAt, i);
							if (obj == null)
								continue;

							// Calculations
							{
								center += obj.position;
								var component = obj.GetComponent<Rigidbody>();
								if (component != null)
									totalVelocity += component.linearVelocity;

								if (calculateBounds)
									bounds.Encapsulate(obj.position);
							}

							totalCalculatedObjects++;
						}

						center /= (float)totalCalculatedObjects;
					}
					break;
				case Mode.VelocityAppliedCenter:
					{
						for (int i = 0; i < objects.Count; i++)
						{
							var obj = InternalGetObjectAt(ref removeObjectsAt, i);
							if (obj == null)
								continue;

							// Calculations
							{
								Vector3 velocityAppliedCenter;

								var component = obj.GetComponent<Rigidbody>();
								if (component != null)
								{
									velocityAppliedCenter = obj.position + component.linearVelocity * velocityFactor;
								}
								else
								{
									velocityAppliedCenter = obj.position;
								}
								center += velocityAppliedCenter;

								if (calculateBounds)
									bounds.Encapsulate(velocityAppliedCenter);
							}

							totalCalculatedObjects++;
						}

						center /= (float)totalCalculatedObjects;
					}
					break;
				case Mode.VelocityAppliedCenterAndTotalVelocity:
					{
						for (int i = 0; i < objects.Count; i++)
						{
							var obj = InternalGetObjectAt(ref removeObjectsAt, i);
							if (obj == null)
								continue;

							// Calculations
							{
								Vector3 velocityAppliedCenter;
								var component = obj.GetComponent<Rigidbody>();
								if (component != null)
								{
									var velocity = component.linearVelocity;
									velocityAppliedCenter = obj.position + velocity * velocityFactor;
									totalVelocity += velocity;
								}
								else
								{
									velocityAppliedCenter = obj.position;
								}
								center += velocityAppliedCenter;

								if (calculateBounds)
									bounds.Encapsulate(velocityAppliedCenter);
							}

							totalCalculatedObjects++;
						}

						center /= (float)totalCalculatedObjects;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (removeObjectsAt != null)
			{
				RemoveObjectsByIndexList(removeObjectsAt);
			}

			// Reset parameters if every item has been destroyed within current frame
			if (objects.Count == 0)
			{
				ResetResults();
			}
		}

		#endregion

		#region Add / Remove Objects

		public List<Transform> objects;
		public bool autoRemoveDestroyedObjectsFromList = true;

		public void ClearObjects()
		{
			objects = new List<Transform>();
			ResetResults();
		}

		public void AddObject(Transform obj, bool checkIfAlreadyExists = true)
		{
			if (checkIfAlreadyExists && objects.Contains(obj))
				return;

			objects.Add(obj);
		}

		public bool RemoveObject(Transform obj)
		{
			return objects.Remove(obj);
		}

		public void RemoveObjectsByIndexList(List<int> indexList)
		{
			if (indexList == null)
				return;

			for (int i = indexList.Count - 1; i >= 0; i--)
			{
				objects.RemoveAt(indexList[i]);
			}
		}

		public void RemoveDestroyedObjectsFromList()
		{
			for (int i = objects.Count - 1; i >= 0; i--)
			{
				if (objects[i] == null)
				{
					objects.RemoveAt(i);
				}
			}
		}

		#endregion

		#region Mode

		public enum Mode
		{
			Center,
			TotalVelocity,
			CenterAndTotalVelocity,
			VelocityAppliedCenter,
			VelocityAppliedCenterAndTotalVelocity,
		}

		public Mode mode = Mode.Center;
		public bool discardSleepingObjects = false;
		public bool calculateBounds = false;

		// VelocityAppliedCenter mode variables
		public float velocityFactor = 0.2f;

		#endregion

		#region Internal Tools

		private Transform InternalGetObjectAt(ref List<int> removeObjectsAt, int i)
		{
			var obj = objects[i];

			if (obj == null)
			{
				if (autoRemoveDestroyedObjectsFromList)
				{
					if (removeObjectsAt == null)
						removeObjectsAt = new List<int>(10);
					removeObjectsAt.Add(i);
				}
				return null;
			}


			if (discardSleepingObjects)
			{
				var component = obj.GetComponent<Rigidbody>();
				if (component != null && component.IsSleeping())
					return null;
			}
			return obj;
		}

		#endregion

		#region Result

		private Vector3 center;
		private Vector3 totalVelocity;
		private Bounds bounds;
		private int totalCalculatedObjects;

		public void ResetResults()
		{
			center = Vector3Tools.NaN;
			totalVelocity = Vector3Tools.NaN;
			bounds = new Bounds(Vector3Tools.NaN, Vector3Tools.NaN);
			totalCalculatedObjects = int.MinValue;
		}

		public void SetResultsToZero()
		{
			center = Vector3.zero;
			totalVelocity = Vector3.zero;
			bounds.SetMinMax(Vector3Tools.PositiveInfinity, Vector3Tools.NegativeInfinity);
			totalCalculatedObjects = 0;
		}

		public Vector3 Center
		{
			get { return center; }
		}

		public Vector3 TotalVelocity
		{
			get { return totalVelocity; }
		}

		public Bounds Bounds
		{
			get { return bounds; }
		}

		public int TotalCalculatedObjects
		{
			get { return totalCalculatedObjects; }
		}

		public bool IsAllSleepingOrNull
		{
			get
			{
				if (objects == null)
					return true; // Count as sleeping

				for (int i = 0; i < objects.Count; i++)
				{
					var obj = objects[i];
					if (obj == null)
						continue; // Count as sleeping

					var component = obj.GetComponent<Rigidbody>();
					if (component == null || component.IsSleeping())
						continue; // Count as sleeping

					return false;
				}
				return true;
			}
		}

		#endregion
	}

}

#endif
