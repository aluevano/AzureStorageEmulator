using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal static class NativeMethods
	{
		[DllImport("httpapi.dll", CharSet=CharSet.None, EntryPoint="HttpDeleteServiceConfiguration", ExactSpelling=true)]
		public static extern int HttpDeleteServiceConfigurationAcl(IntPtr mustBeZero, int configID, [In] ref Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET configInfo, int configInfoLength, IntPtr mustBeZero2);

		[DllImport("httpapi.dll", CharSet=CharSet.None, ExactSpelling=false)]
		public static extern int HttpInitialize(Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTPAPI_VERSION version, int flags, IntPtr mustBeZero);

		[DllImport("httpapi.dll", CharSet=CharSet.None, EntryPoint="HttpSetServiceConfiguration", ExactSpelling=true)]
		public static extern int HttpSetServiceConfigurationAcl(IntPtr mustBeZero, int configID, [In] ref Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_SET configInfo, int configInfoLength, IntPtr mustBeZero2);

		[DllImport("httpapi.dll", CharSet=CharSet.None, ExactSpelling=false)]
		public static extern int HttpTerminate(int flags, IntPtr mustBeZero);

		public struct HTTP_SERVICE_CONFIG_URLACL_KEY
		{
			public string UrlPrefix;
		}

		public struct HTTP_SERVICE_CONFIG_URLACL_PARAM
		{
			public string Sddl;
		}

		public struct HTTP_SERVICE_CONFIG_URLACL_SET
		{
			public Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_KEY Key;

			public Microsoft.WindowsAzure.Storage.Emulator.Controller.NativeMethods.HTTP_SERVICE_CONFIG_URLACL_PARAM Param;
		}

		public struct HTTPAPI_VERSION
		{
			public short Major;

			public short Minor;

			public HTTPAPI_VERSION(short maj, short min)
			{
				this.Major = maj;
				this.Minor = min;
			}
		}
	}
}