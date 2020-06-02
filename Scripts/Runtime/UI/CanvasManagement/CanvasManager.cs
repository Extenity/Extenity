using System.Collections.Generic;
using UnityEngine;

namespace Extenity.UIToolbox
{

	public static class CanvasManager
	{
		/// <summary>
		/// CAUTION! Use it as readonly!
		/// </summary>
		public static readonly List<Canvas> Canvases = new List<Canvas>(10);

		internal static void Register(Canvas canvas)
		{
			ClearDestroyedEntries();

			if (!Canvases.Contains(canvas))
			{
				Canvases.Add(canvas);
			}
		}

		internal static void Deregister(Canvas canvas)
		{
			ClearDestroyedEntries();

			for (int i = 0; i < Canvases.Count; i++)
			{
				if (Canvases[i] == canvas)
				{
					Canvases.RemoveAt(i);
					return;
				}
			}
		}

		private static void ClearDestroyedEntries()
		{
			for (int i = Canvases.Count - 1; i >= 0; i--)
			{
				if (!Canvases[i])
				{
					Canvases.RemoveAt(i);
				}
			}
		}
	}

}
