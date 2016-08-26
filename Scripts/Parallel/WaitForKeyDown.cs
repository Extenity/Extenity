using UnityEngine;

namespace Extenity.Parallel
{

	public class WaitForKeyDown : CustomYieldInstruction
	{
		public KeyCode KeyCode { get; set; }
		private bool IsKeyDown { get; set; }

		public WaitForKeyDown(KeyCode keyCode)
		{
			KeyCode = keyCode;
		}

		public override bool keepWaiting
		{
			get
			{
				var keepWaiting = true;
				if (Input.GetKeyDown(KeyCode))
				{
					if (!IsKeyDown)
					{
						keepWaiting = false;
					}
					IsKeyDown = true;
				}
				if (Input.GetKeyUp(KeyCode))
				{
					IsKeyDown = false;
				}
				return keepWaiting;
			}
		}
	}

}
