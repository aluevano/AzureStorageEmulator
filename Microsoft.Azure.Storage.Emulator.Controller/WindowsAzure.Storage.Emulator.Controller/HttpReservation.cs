using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal static class HttpReservation
	{
		private const int HttpServiceConfigUrlAclInfo = 2;

		private const int HttpInitializeConfig = 2;

		public static bool ArePortsReserved(string urlPrefix, string accountName)
		{
			Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET hTTPSERVICECONFIGURLACLSET = new Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET();
			bool flag;
			bool flag1 = false;
			string str = HttpReservation.CreateSddl(accountName);
			hTTPSERVICECONFIGURLACLSET.Key.UrlPrefix = urlPrefix;
			hTTPSERVICECONFIGURLACLSET.Param.Sddl = str;
			Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTPAPI_VERSION hTTPAPIVERSION = new Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTPAPI_VERSION(1, 0);
			int num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpInitialize(hTTPAPIVERSION, 2, IntPtr.Zero);
			if (num != 0)
			{
				throw HttpReservation.GetException("HttpInitialize", num);
			}
			try
			{
				num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpDeleteServiceConfigurationAcl(IntPtr.Zero, 2, ref hTTPSERVICECONFIGURLACLSET, Marshal.SizeOf(typeof(Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET)), IntPtr.Zero);
				if (num != 0)
				{
					flag1 = false;
				}
				else
				{
					num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpSetServiceConfigurationAcl(IntPtr.Zero, 2, ref hTTPSERVICECONFIGURLACLSET, Marshal.SizeOf(typeof(Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET)), IntPtr.Zero);
					if (num != 0)
					{
						throw HttpReservation.GetException("HttpSetServiceConfigurationAcl", num);
					}
					flag1 = true;
				}
				flag = flag1;
			}
			finally
			{
				Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpTerminate(2, IntPtr.Zero);
			}
			return flag;
		}

		private static string CreateSddl(string account)
		{
			string str = (new NTAccount(account)).Translate(typeof(SecurityIdentifier)).ToString();
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { str };
			return string.Format(invariantCulture, "D:(A;;GX;;;{0})", objArray);
		}

		private static Exception GetException(string fcn, int errorCode)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { fcn, HttpReservation.GetWin32ErrorMessage(errorCode) };
			return new HttpReservationException(string.Format(invariantCulture, "{0} failed: {1}", objArray));
		}

		private static string GetWin32ErrorMessage(int errorCode)
		{
			return Marshal.GetExceptionForHR(HttpReservation.HRESULT_FROM_WIN32(errorCode)).Message;
		}

		private static int HRESULT_FROM_WIN32(int errorCode)
		{
			if (errorCode <= 0)
			{
				return errorCode;
			}
			return 65535 & errorCode | 458752 | -2147483648;
		}

		public static void ModifyReservation(string urlPrefix, string accountName, bool removeReservation)
		{
			Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET hTTPSERVICECONFIGURLACLSET = new Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET();
			string str = HttpReservation.CreateSddl(accountName);
			hTTPSERVICECONFIGURLACLSET.Key.UrlPrefix = urlPrefix;
			hTTPSERVICECONFIGURLACLSET.Param.Sddl = str;
			Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTPAPI_VERSION hTTPAPIVERSION = new Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTPAPI_VERSION(1, 0);
			int num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpInitialize(hTTPAPIVERSION, 2, IntPtr.Zero);
			if (num != 0)
			{
				throw HttpReservation.GetException("HttpInitialize", num);
			}
			try
			{
				if (removeReservation)
				{
					num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpDeleteServiceConfigurationAcl(IntPtr.Zero, 2, ref hTTPSERVICECONFIGURLACLSET, Marshal.SizeOf(typeof(Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET)), IntPtr.Zero);
				}
				num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpSetServiceConfigurationAcl(IntPtr.Zero, 2, ref hTTPSERVICECONFIGURLACLSET, Marshal.SizeOf(typeof(Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET)), IntPtr.Zero);
				if (num != 0)
				{
					throw HttpReservation.GetException("HttpSetServiceConfigurationAcl", num);
				}
			}
			finally
			{
				Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpTerminate(2, IntPtr.Zero);
			}
		}

		public static bool TryDeleteReservation(string urlPrefix, string accountName)
		{
			Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET hTTPSERVICECONFIGURLACLSET = new Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET();
			bool flag;
			string str = HttpReservation.CreateSddl(accountName);
			hTTPSERVICECONFIGURLACLSET.Key.UrlPrefix = urlPrefix;
			hTTPSERVICECONFIGURLACLSET.Param.Sddl = str;
			Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTPAPI_VERSION hTTPAPIVERSION = new Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTPAPI_VERSION(1, 0);
			int num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpInitialize(hTTPAPIVERSION, 2, IntPtr.Zero);
			if (num != 0)
			{
				throw HttpReservation.GetException("HttpInitialize", num);
			}
			try
			{
				num = Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpDeleteServiceConfigurationAcl(IntPtr.Zero, 2, ref hTTPSERVICECONFIGURLACLSET, Marshal.SizeOf(typeof(Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET)), IntPtr.Zero);
				flag = num == 0;
			}
			finally
			{
				Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HttpTerminate(2, IntPtr.Zero);
			}
			return flag;
		}
	}
}