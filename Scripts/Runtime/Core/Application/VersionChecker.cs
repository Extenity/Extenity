
namespace Extenity.ApplicationToolbox
{

	public enum VersionCheckStatus
	{
		NotCheckedYet,
		NoNewVersion,
		FailedToCheck,
		NewVersionAvailable,
	}

	public static class VersionChecker
	{
		public static VersionCheckStatus Status { get; private set; } = VersionCheckStatus.NotCheckedYet;

		// Commented out intentionally. Well, just poll the Status value whenever needed.
		// No need to allocate an event object for just a simple check. Since this check
		// probably will only be made at the start of the application.
		//public static readonly UnityEvent OnVersionChecked = new UnityEvent();

		public static void InformMinVersion(string minVersionString)
		{
			if (!string.IsNullOrWhiteSpace(minVersionString))
			{
				try
				{
					var minVersion = new ApplicationVersion(minVersionString);
					var currentVersion = ApplicationVersion.GetUnityVersion();
					if (currentVersion < minVersion)
					{
						Status = VersionCheckStatus.NewVersionAvailable;
					}
					else
					{
						Status = VersionCheckStatus.NoNewVersion;
					}
				}
				catch
				{
					// There is something wrong with "minimum allowed version" configuration received from the backend.
					// Something is probably wrong here. But we don't want to cause any trouble for the player. So we just skip the check and move on.
					Status = VersionCheckStatus.FailedToCheck;
				}
			}
			else
			{
				// There is no "minimum allowed version" configuration received from the backend.
				// Something is probably wrong here. But we don't want to cause any trouble for the player. So we just skip the check and move on.
				Status = VersionCheckStatus.FailedToCheck;
			}

			//OnVersionChecked.Invoke();
		}
	}

}
