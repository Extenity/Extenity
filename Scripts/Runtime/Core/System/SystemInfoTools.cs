#if UNITY

using System;
using Extenity.CryptoToolbox;
using Extenity.DataToolbox;
using UnityEngine;
using Guid = Extenity.DataToolbox.Guid;

namespace Extenity.SystemToolbox
{

	public static class SystemInfoTools
	{
		#region DeviceUniqueIdentifier

		// TODO: Logging IDs to database each time a user launches the application and make sure it only runs if the user accepts the collection of analytics data. Required data: DeviceUniqueIdentifier, NonThrowingUnityDeviceUniqueIdentifier, device vendor, device model.

		/// <summary>
		/// The alternative to SystemInfo.deviceUniqueIdentifier which hopefully is more robust.
		/// The generated ID is lowercase hexadecimal string of varying lengths. The algorithm also
		/// adds a postfix to the ID that tells which method is used for getting the device ID on
		/// that device.
		///
		/// Note that this implementation is no way proven to be robust, as of 03/2019. It requires
		/// extensive testing in large scale with collecting data from user devices. The implementation
		/// is just based on developer experiences collected on forums. Being registered into an
		/// analytics database would allow us to see which device vendors and models tend to generate
		/// which type of device IDs.
		///
		/// The implementation is heavily commented with references to other developer experiences.
		/// Before using the method or modifying the algorithm, make sure you read all comments for
		/// all platforms and understand why this approach is required.
		///
		/// One of the key ideas here is your application SHOULD NOT use the device ID for accessing
		/// the user account wherever possible. This method of anonymous login mechanism is great for
		/// first time users that are just started trying the application. But the application should
		/// direct users to create an account or link with a social account as soon as possible.
		/// It especially should be considered a MUST for paying customers. As stated in PlayFab docs,
		/// "A free account lost forever is a bummer. A paid account lost forever is a revenue problem."
		/// More detailed information is available in PlayFab documents.
		/// https://api.playfab.com/docs/tutorials/landing-players/best-login
		/// 
		/// The implementation in some cases saves the generated ID in PlayerPrefs to cope with
		/// unexpectedly changing IDs. This makes the whole application vulnerable to hack attempts.
		/// Makes it easier for an intruder to gain access to another user's account if the victim's
		/// device ID is known by the intruder. While this looks bad, it will only affect users that
		/// are not yet linked their progress with a social or custom account. Login with using only
		/// the device ID should be a temporary step for the user. Also the implementation saves
		/// the device ID encrypted with varying encryption keys for every project. This closes some
		/// doors for an intruder to gain the device ID information of another user. Intruder won't
		/// be able to get the device ID from another application and use that in your application.
		/// 
		/// See these links for why Unity's implementation is bad.
		/// https://forum.unity.com/threads/systeminfo-deviceuniqueidentifier-android-6-0.367028/
		/// https://forum.unity.com/threads/unique-identifier-details.353256/
		/// https://fogbugz.unity3d.com/default.asp?743378_mpthnvaql975mjmi
		///
		/// Some comments:
		///   "When inserting a USB flash drive in Windows, SystemInfo.deviceUniqueIdentifier changes!"
		///   "One of the guy of the QA of my game reports that he lost all this progress by updating the game."
		///   "Short question. Why use hash method on a unique identifier?"
		///   "It seems like using SkipPermissionsDialog make all permissions denied which then results in a different uniqueIdentitifer."
		///   "What we have done for our user is to register the key they got (if not null) at their first launch, and we always use this key."
		///   "If WRITE_EXTERNAL_STORAGE defined in AndroidManifest, deviceUniqueIdentifier always return cd9e459ea708a948d5c2f5a6ca8838cf."
		///   "We see this on Nexus 7. So all our players loose their account and share the same one."
		///   "If the mac could not be read it is set to 00000000000000000000000000000000"
		/// </summary>
		public static string DeviceUniqueIdentifier
		{
			get
			{
				// Do not cache it for security reasons. We won't want to keep the ID in memory.
				// Also there should be no need for this to be cached since it should be called rarely.
				//    if (_DeviceUniqueIdentifier == null)
				//        _DeviceUniqueIdentifier = InternalGetDeviceUniqueIdentifier();
				//    return _DeviceUniqueIdentifier;

				return InternalGetDeviceUniqueIdentifier();
			}
		}

		#region Store device ID in PlayerPrefs

		public static Func<string> Builder;

		/// <summary>
		/// This string has chosen to be unlikely to collide with any other Asset Store packages in mind.
		/// </summary>
		private const string DeviceIDPrefsKey = "TokenStored";
		private const string DeviceIDStoredPrefix = "TAG=";

		private static string _GetStoredID()
		{
			// If you see an error here about 'Builder' being null, you have probably forgot
			// to give it a callback that generates the encryption key for your project.
			// You need to set the callback before calling DeviceUniqueIdentifier.
			// The error is intentionally left without a description for security reasons.
			var key = Builder();

			try
			{
				var id = PlayerPrefs.GetString(DeviceIDPrefsKey, "");
				if (string.IsNullOrEmpty(id))
					return null;
				id = SimpleTwoWayEncryptorAES.DecryptHexWithIV(id, key);
				if (!id.StartsWith(DeviceIDStoredPrefix))
					return null;
				id = id.Substring(DeviceIDStoredPrefix.Length);
				if (IsDeviceIDValid(id))
					return id;
			}
			catch
			{
				// Ignored.
			}
			return null;
		}

		private static void _StoreID(string id)
		{
#if UNITY_EDITOR
			Log.With(nameof(DeviceUniqueIdentifier)).Info("Storing device ID: " + id);
#endif
			var key = Builder();
			id = DeviceIDStoredPrefix + id;
			id = SimpleTwoWayEncryptorAES.EncryptHexWithIV(id, key);
			PlayerPrefs.SetString(DeviceIDPrefsKey, id);
			PlayerPrefs.Save();
		}

		private static bool IsDeviceIDValid(string id)
		{
			if (string.IsNullOrWhiteSpace(id) ||
				!id.IsAlphaNumericAscii('-', false, false) ||
				//!id.IsHexString(false) || Decided not to give a hex constraint. Maybe Unity generates non-hex alpha characters in some platforms.
				id.Equals(SystemInfo.unsupportedIdentifier, StringComparison.OrdinalIgnoreCase) ||
				id.IsAllZeros() || // This can happen with SystemInfo.deviceUniqueIdentifier failing to read MAC address on some devices.
				id.Equals("cd9e459ea708a948d5c2f5a6ca8838cf", StringComparison.OrdinalIgnoreCase) // Banned ID, which is MD5 hash of all zeros that SystemInfo.deviceUniqueIdentifier tends to generate.
			)
			{
				return false;
			}
			return true;
		}

		#endregion

		public static string NonThrowingUnityDeviceUniqueIdentifier
		{
			get
			{
				try
				{
					return SystemInfo.deviceUniqueIdentifier;
				}
				catch
				{
					// Well, things went bad. But this method does not attempt to do anything,
					// other than prevent things to blow.
					return "";
				}
			}
		}

		private static string GenerateGUIDIfUnityGeneratedDeviceIDIsInvalid()
		{
			var id = NonThrowingUnityDeviceUniqueIdentifier;
			return GenerateGUIDIfDeviceIDIsInvalid(id);
		}

		private static string GenerateGUIDIfDeviceIDIsInvalid(string id)
		{
			if (IsDeviceIDValid(id))
				return id + "0d"; // Postfix means the original SystemInfo.deviceUniqueIdentifier is used.

			// Generate using Guid.NewGuid. However despite how unlikely it might seem, this result too
			// will be validated in case it might not work on some platforms or in future for some unknown
			// reason.
			id = System.Guid.NewGuid().ToByteArray().ToHexStringCombined(true);
			if (IsDeviceIDValid(id))
				return id + "0e"; // Postfix means the ID is generated locally via Guid.NewGuid.

			// Falling back to basic ID generation. There is no chance for this to fail.
			{
				// Generate a seed beforehand because we would never know the state of Unity's RNG.
				// Generate a seed afterwards to clear RNG state. Make sure it's harder to link back
				// to this ID.
				id = Guid.NewPseudoGuid(true, true).Data.ToHexStringCombined(true);
				if (IsDeviceIDValid(id))
					return id + "0f"; // Postfix means the ID is generated locally via Unity's RNG.
			}

			// This will never be the case. But if it is, something really fishy is going on.
			throw new InternalException(1157128991);
			//throw new Exception("Failed to generate a device ID."); Let's not write down what happened in detail, which may give hints if there is a hack attempt.
		}

		private static string InternalGetDeviceUniqueIdentifier()
		{
#if UNITY_EDITOR

			// Device ID is nothing critical in editor environment. Just use what Unity has to say about it
			// though with some little modifications. Note that Unity's deviceUniqueIdentifier is so broken
			// that even plugging in a flash drive could change the device ID. So the first time we get a
			// request for device ID, we store it in user preferences and use that in future calls.
			//
			// Also an additional path hash is appended after the ID. There may be more than one project folder
			// in the same PC. We will treat them as if they are on separate devices by appending a path hash
			// to the ID. That way these projects would be treated like different clients of a multiplayer game
			// (don't know if there are any other use cases).

			// Try to get saved ID, if there are any.
			var storedID = _GetStoredID();
			if (!string.IsNullOrEmpty(storedID))
				return storedID;

			var id = GenerateGUIDIfUnityGeneratedDeviceIDIsInvalid();
			id = id + "dae" + ApplicationToolbox.ApplicationTools.PathHash.ToLowerInvariant() + "ade";
			_StoreID(id);
			return id;

#elif UNITY_ANDROID || UNITY_IOS

			// Unity has a proven track of getting messy with Android implementation of deviceUniqueIdentifier.
			// Hopefully these are in the past and I personally kind of in a love and hate relationship with
			// Unity. Decided not to implement it myself and just use SystemInfo.deviceUniqueIdentifier wrapped
			// inside a security blanket. Because Unity as a giant, should have the potential to implement it
			// better than anybody. I think it was just not in the mood lately.
			//
			// iOS implementation is looking more robust, so decided to use SystemInfo.deviceUniqueIdentifier
			// for iOS too.
			//
			// One additional feature over deviceUniqueIdentifier is that the system will check if device ID
			// is valid. If not, it will generate a device ID locally. See GenerateGUIDIfDeviceIDIsInvalid.
			//
			// Another additional feature is the initially grabbed/generated device ID will be stored inside
			// user preferences and the stored ID will be used in future calls. The idea here is to act as a
			// guard against any possible modifications to deviceUniqueIdentifier algorithm on Unity internals.
			// That may possibly end up unexpectedly changing a user's device ID with an update to the application
			// in which the application incorporates a newer Unity version. Looks like so many users have lost
			// their game accounts in the past because of their device IDs changed after updating the game.
			//
			// Note that if the user deletes the application with its configuration, the saved ID is gone.
			// When the user decides to install the application once more, there is a possibility that Unity
			// would fail to generate the same ID for that user again. As long as this implementation is based
			// on SystemInfo.deviceUniqueIdentifier, we would only hope that Unity will not fail us, as it was
			// happened in the past.
			//
			// See these links for further information.
			//     https://community.playfab.com/questions/14538/unity-ios-device-unique-identifier.html
			//     https://api.playfab.com/docs/tutorials/landing-players/best-login
			//     UnityEngine.iOS.Device.vendorIdentifier
			//     https://github.com/HuaYe1975/Unity-iOS-DeviceID
			//     https://developer.apple.com/documentation/uikit/uidevice/1620059-identifierforvendor
			//     https://docs.unity3d.com/ScriptReference/SystemInfo-deviceUniqueIdentifier.html

			// First, try to get the previously saved ID, if there is one.
			var storedID = _GetStoredID();
			if (!string.IsNullOrEmpty(storedID))
				return storedID;

			// Get the ID from Unity, but ensure it's not faulty. Generate a new one randomly if it is faulty.
			var id = GenerateGUIDIfUnityGeneratedDeviceIDIsInvalid();

			// Store the ID for future uses. This is required so that any changes to Unity's implementation
			// of SystemInfo.deviceUniqueIdentifier won't affect already existing users when we update Unity
			// to a new version.
			_StoreID(id);
			return id;

#elif UNITY_STANDALONE_WIN

			// This implementation is copied from iOS and Android implementation above. Not so much thought
			// was given on this platform yet. But the same principles apply here too.

			// First, try to get the previously saved ID, if there is one.
			var storedID = _GetStoredID();
			if (!string.IsNullOrEmpty(storedID))
				return storedID;

			// Get the ID from Unity, but ensure it's not faulty. Generate a new one randomly if it is faulty.
			var id = GenerateGUIDIfUnityGeneratedDeviceIDIsInvalid();

			// Store the ID for future uses. This is required so that any changes to Unity's implementation
			// of SystemInfo.deviceUniqueIdentifier won't affect already existing users when we update Unity
			// to a new version.
			_StoreID(id);
			return id;

#else

			// Oops! This is a new platform. Seeing that SystemInfo.deviceUniqueIdentifier is not trustworthy,
			// find a proper way to implement our own in this new platform.
			//
			// If decided to use Unity generated ID on this new platform, make sure the Unity generated ID is
			// not detected as faulty by GenerateGUIDIfDeviceIDIsInvalid. If so, modify the detection method.

			throw new NotImplementedException($"DeviceUniqueIdentifier is not yet implemented for platform '{Application.platform}'");

#endif
		}

		#endregion
	}

}

#endif
