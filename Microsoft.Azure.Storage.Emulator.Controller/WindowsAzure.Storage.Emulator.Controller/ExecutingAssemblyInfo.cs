using System;
using System.IO;
using System.Reflection;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal static class ExecutingAssemblyInfo
	{
		private static string fullPath;

		public static string DirectoryName
		{
			get
			{
				return Path.GetDirectoryName(ExecutingAssemblyInfo.FullPath);
			}
		}

		public static string FullPath
		{
			get
			{
				if (ExecutingAssemblyInfo.fullPath == null)
				{
					ExecutingAssemblyInfo.fullPath = ExecutingAssemblyInfo.GetFullPath();
				}
				return ExecutingAssemblyInfo.fullPath;
			}
		}

		private static string GetFullPath()
		{
			return (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
		}
	}
}