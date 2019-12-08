using Microsoft.Win32;
using Microsoft.WindowsAzure.Storage.Emulator;
using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class StartCommand : Command
	{
		private const string MonitorPathRegPath = "SOFTWARE\\Microsoft\\Windows Azure Emulator";

		private const string MonitorInstallPathKeyName = "InstallPath";

		private const string RelativeMonitorPath = "Emulator\\csmonitor.exe";

		public bool ArgumentInProcess
		{
			get;
			set;
		}

		public override string CommandName
		{
			get
			{
				return "start";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public StartCommand()
		{
		}

		private static string GetMonitorPath()
		{
			string empty;
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows Azure Emulator", RegistryKeyPermissionCheck.ReadSubTree, RegistryRights.QueryValues);
			if (registryKey == null)
			{
				return string.Empty;
			}
			string value = (string)registryKey.GetValue("InstallPath", string.Empty, RegistryValueOptions.None);
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}
			try
			{
				string fullPath = Path.GetFullPath(Path.Combine(value, "Emulator\\csmonitor.exe"));
				if (!File.Exists(fullPath))
				{
					return string.Empty;
				}
				return fullPath;
			}
			catch (Exception exception)
			{
				empty = string.Empty;
			}
			return empty;
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1} [-inprocess]", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Starts up the storage emulator.");
			Console.WriteLine("    -inprocess : Start the emulator in the current process instead of creating a new process.");
		}

		public override void RunCommand()
		{
			if (this.ArgumentInProcess)
			{
				Program.LaunchEmulatorInstance();
				return;
			}
			StartCommand.StartMonitor();
			EmulatorInstance.CommonInstance.Start();
			Console.WriteLine(Resource.SuccessStart);
		}

		private static bool StartMonitor()
		{
			bool flag;
			try
			{
				string monitorPath = StartCommand.GetMonitorPath();
				if (!string.IsNullOrEmpty(monitorPath))
				{
					ProcessStartInfo processStartInfo = new ProcessStartInfo()
					{
						FileName = monitorPath,
						WorkingDirectory = Path.GetDirectoryName(monitorPath),
						UseShellExecute = true
					};
					Process.Start(processStartInfo);
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			catch (Exception exception)
			{
				flag = false;
			}
			return flag;
		}
	}
}