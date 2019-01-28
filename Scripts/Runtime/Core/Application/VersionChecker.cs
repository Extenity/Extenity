
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

		public static void InformMinVersion(string minVersionString, bool log)
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
						if (log)
							Log.Info($"New version '{minVersion}' available.");
					}
					else
					{
						Status = VersionCheckStatus.NoNewVersion;
						if (log)
							Log.Info("Up to date.");
					}
				}
				catch
				{
					// There is something wrong with "minimum allowed version" configuration received from the backend.
					Status = VersionCheckStatus.FailedToCheck;
					if (log)
						Log.Info("Failed to validate version check.");
				}
			}
			else
			{
				// There is no "minimum allowed version" configuration received from the backend.
				Status = VersionCheckStatus.FailedToCheck;
				if (log)
					Log.Info("Failed to check for new version.");
			}

			//OnVersionChecked.Invoke();
		}
	}

}
