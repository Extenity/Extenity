#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Extenity.ParallelToolbox
{

	public class WaitForKeyDown : CustomYieldInstruction
	{
		public KeyCode KeyCode { get; set; }
		public KeyCode ResetKeyCode { get; set; }
		private bool IsKeyDown { get; set; }

		public WaitForKeyDown(KeyCode keyCode, KeyCode resetKeyCode = KeyCode.None)
		{
			KeyCode = keyCode;
			ResetKeyCode = resetKeyCode;
		}

		public override bool keepWaiting
		{
			get
			{
				if (KeyCode == KeyCode.None)
					return false;
				if (Input.GetKey(ResetKeyCode))
				{
					KeyCode = KeyCode.None;
					return false;
				}

				var keepWaiting = true;
				if (!Input.GetKey(KeyCode)) // We would miss Key Up event if this yield instruction is not called in every update. So this is a backup to detect key release.
				{
					IsKeyDown = false;
				}
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

#endif
