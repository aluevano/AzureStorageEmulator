using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Permissions;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal class ProcessWrapper
	{
		internal ProcessStartInfo StartInfo
		{
			get;
			set;
		}

		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		internal ProcessWrapper(string binaryName, string arguments)
		{
			string str = ProcessWrapper.ResolveBinaryFullPath(binaryName);
			ProcessStartInfo processStartInfo = new ProcessStartInfo()
			{
				Arguments = arguments,
				FileName = str,
				UseShellExecute = true,
				CreateNoWindow = true,
				WorkingDirectory = Path.GetDirectoryName(str)
			};
			this.StartInfo = processStartInfo;
			if (Environment.GetEnvironmentVariable("_DF_CONSOLE") != null)
			{
				this.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
				return;
			}
			this.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		}

		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		internal void EnsureStarted()
		{
			Process.Start(this.StartInfo);
		}

		[PermissionSet(SecurityAction.Demand, Name="FullTrust")]
		internal Process[] GetInstances()
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.StartInfo.FileName);
			string fileName = Path.GetFileName(this.StartInfo.FileName);
			List<Process> processes = new List<Process>();
			Process[] processesByName = Process.GetProcessesByName(fileNameWithoutExtension);
			for (int i = 0; i < (int)processesByName.Length; i++)
			{
				Process process = processesByName[i];
				try
				{
					if (string.Compare(process.ProcessName, fileNameWithoutExtension, StringComparison.OrdinalIgnoreCase) == 0)
					{
						using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(string.Concat("SELECT CommandLine FROM Win32_Process WHERE ProcessId = ", process.Id)))
						{
							foreach (ManagementObject managementObject in managementObjectSearcher.Get())
							{
								if (managementObject["CommandLine"] == null)
								{
									continue;
								}
								string item = (string)managementObject["CommandLine"];
								int num = item.IndexOf(fileName, StringComparison.OrdinalIgnoreCase);
								int length = num + fileName.Length;
								ProcessStartInfo startInfo = process.StartInfo;
								string str = item.Substring(0, length);
								char[] chrArray = new char[] { '\\' };
								startInfo.FileName = str.Trim(chrArray);
								process.StartInfo.Arguments = item.Substring(length + 1).Trim();
							}
						}
						processes.Add(process);
					}
				}
				catch (Exception exception)
				{
				}
			}
			return processes.ToArray();
		}

		internal static string ResolveBinaryFullPath(string binaryName)
		{
			string directoryName = ExecutingAssemblyInfo.DirectoryName;
			string fullPath = Path.GetFullPath(Path.Combine(directoryName, binaryName));
			if (File.Exists(fullPath))
			{
				return fullPath;
			}
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows Azure Storage Emulator", RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.QueryValues);
			if (registryKey != null)
			{
				directoryName = (string)registryKey.GetValue("InstallPath", directoryName, RegistryValueOptions.None);
			}
			try
			{
				fullPath = Path.GetFullPath(Path.Combine(directoryName, binaryName));
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				CultureInfo currentCulture = CultureInfo.CurrentCulture;
				string formatUnableToFindFilePleaseVerifyInstall = Resource.FormatUnableToFindFilePleaseVerifyInstall;
				object[] objArray = new object[] { binaryName };
				string str = string.Format(currentCulture, formatUnableToFindFilePleaseVerifyInstall, objArray);
				throw new EmulatorException(EmulatorErrorCode.CorruptInstallation, str, exception);
			}
			if (!File.Exists(fullPath))
			{
				CultureInfo cultureInfo = CultureInfo.CurrentCulture;
				string formatUnableToFindFilePleaseVerifyInstall1 = Resource.FormatUnableToFindFilePleaseVerifyInstall;
				object[] objArray1 = new object[] { binaryName };
				throw new EmulatorException(EmulatorErrorCode.CorruptInstallation, string.Format(cultureInfo, formatUnableToFindFilePleaseVerifyInstall1, objArray1));
			}
			return fullPath;
		}
	}
}