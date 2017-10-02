using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Daytimer.Functions
{
	/// <summary>
	/// Based off of http://www.davidmoore.info/2011/06/20/how-to-check-if-the-current-user-is-an-administrator-even-if-uac-is-on/
	/// </summary>
	public class UserInfo
	{
		private static bool? _cachedIsElevated = null;

		/// <summary>
		/// Gets if the current process is running with elevated permissions.
		/// </summary>
		public static bool IsElevated
		{
			get
			{
				if (_cachedIsElevated.HasValue)
					return _cachedIsElevated.Value;

				WindowsIdentity identity = WindowsIdentity.GetCurrent();

				if (identity == null)
					throw new InvalidOperationException("Couldn't get the current user identity");

				WindowsPrincipal principal = new WindowsPrincipal(identity);

				_cachedIsElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
				return _cachedIsElevated.Value;
			}
		}

		/// <summary>
		/// Gets if the current user is a member of the administrators group.
		/// </summary>
		public static bool IsCurrentUserAdmin
		{
			get
			{
				WindowsIdentity identity = WindowsIdentity.GetCurrent();

				if (identity == null)
					throw new InvalidOperationException("Couldn't get the current user identity");

				WindowsPrincipal principal = new WindowsPrincipal(identity);

				// Check if this user has the Administrator role. If they do, return immediately.
				// If UAC is on, and the process is not elevated, then this will actually return false.
				if (principal.IsInRole(WindowsBuiltInRole.Administrator))
					return true;

				// If we're not running in Vista onwards, we don't have to worry about checking for UAC.
				if (Environment.OSVersion.Platform != PlatformID.Win32NT || Environment.OSVersion.Version < OSVersions.Win_Vista)
				{
					// Operating system does not support UAC; skipping elevation check.
					return false;
				}

				int tokenInfLength = Marshal.SizeOf(typeof(int));
				IntPtr tokenInformation = Marshal.AllocHGlobal(tokenInfLength);

				try
				{
					IntPtr token = identity.Token;
					bool result = GetTokenInformation(token, TokenInformationClass.TokenElevationType, tokenInformation, tokenInfLength, out tokenInfLength);

					if (!result)
					{
						Exception exception = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
						throw new InvalidOperationException("Couldn't get token information", exception);
					}

					TokenElevationType elevationType = (TokenElevationType)Marshal.ReadInt32(tokenInformation);

					switch (elevationType)
					{
						case TokenElevationType.TokenElevationTypeDefault:
							// TokenElevationTypeDefault - User is not using a split token, so they cannot elevate.
							return false;

						case TokenElevationType.TokenElevationTypeFull:
							// TokenElevationTypeFull - User has a split token, and the process is running elevated. Assuming they're an administrator.
							return true;

						case TokenElevationType.TokenElevationTypeLimited:
							// TokenElevationTypeLimited - User has a split token, but the process is not running elevated. Assuming they're an administrator.
							return true;

						default:
							// Unknown token elevation type.
							return false;
					}
				}
				finally
				{
					if (tokenInformation != IntPtr.Zero)
						Marshal.FreeHGlobal(tokenInformation);
				}
			}
		}

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength);

		/// <summary>
		/// Passed to <see cref="GetTokenInformation"/> to specify what
		/// information about the token to return.
		/// </summary>
		enum TokenInformationClass
		{
			TokenUser = 1,
			TokenGroups,
			TokenPrivileges,
			TokenOwner,
			TokenPrimaryGroup,
			TokenDefaultDacl,
			TokenSource,
			TokenType,
			TokenImpersonationLevel,
			TokenStatistics,
			TokenRestrictedSids,
			TokenSessionId,
			TokenGroupsAndPrivileges,
			TokenSessionReference,
			TokenSandBoxInert,
			TokenAuditPolicy,
			TokenOrigin,
			TokenElevationType,
			TokenLinkedToken,
			TokenElevation,
			TokenHasRestrictions,
			TokenAccessInformation,
			TokenVirtualizationAllowed,
			TokenVirtualizationEnabled,
			TokenIntegrityLevel,
			TokenUiAccess,
			TokenMandatoryPolicy,
			TokenLogonSid,
			MaxTokenInfoClass
		}

		/// <summary>
		/// The elevation type for a user token.
		/// </summary>
		enum TokenElevationType
		{
			TokenElevationTypeDefault = 1,
			TokenElevationTypeFull,
			TokenElevationTypeLimited
		}
	}
}
