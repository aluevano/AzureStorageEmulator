using Microsoft.WindowsAzure.Storage.Emulator.Commands;
using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.Storage.Emulator
{
	public class Program
	{
		private readonly static List<Command> Commands;

		private readonly static List<Command> AccountManagementCommands;

		static Program()
		{
			List<Command> commands = new List<Command>()
			{
				new InitCommand(),
				new StartCommand(),
				new StopCommand(),
				new GetStatusCommand(),
				new ClearCommand()
			};
			Program.Commands = commands;
			List<Command> commands1 = new List<Command>()
			{
				new CreateAccountCommand(),
				new DeleteAccountCommand(),
				new GetAccountCommand(),
				new ListAccountsCommand(),
				new RegenerateKeyCommand(),
				new UpdateAccountCommand()
			};
			Program.AccountManagementCommands = commands1;
		}

		public Program()
		{
		}

		private static void HandleUserInterfaceEvents()
		{
			using (EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, "DevelopmentStorage-62F76D03-83B4-40ac-A100-F990EA19CF99"))
			{
				while (true)
				{
					eventWaitHandle.WaitOne();
					string localPath = (new Uri(Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
					ProcessStartInfo processStartInfo = new ProcessStartInfo(Environment.ExpandEnvironmentVariables("%windir%\\System32\\cmd.exe"), string.Format("/K {0} help", Path.GetFileName(localPath)))
					{
						WorkingDirectory = Path.GetDirectoryName(localPath)
					};
					ProcessStartInfo processStartInfo1 = processStartInfo;
					Console.WriteLine(Environment.ExpandEnvironmentVariables("%windir%\\System32\\cmd.exe"));
					Process.Start(processStartInfo1);
					eventWaitHandle.Reset();
				}
			}
		}

		internal static void LaunchEmulatorInstance()
		{
			bool flag = false;
			using (EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, "DevelopmentStorage-7580AFBB-2BEC-4269-B083-46C1361A69B0", out flag))
			{
				using (EventWaitHandle eventWaitHandle1 = new EventWaitHandle(false, EventResetMode.ManualReset, "DevelopmentStorage-Failure-7580AFBB-2BEC-4269-B083-46C1361A69B0"))
				{
					bool flag1 = false;
					try
					{
						eventWaitHandle1.Reset();
						if (!flag)
						{
							eventWaitHandle1.Set();
							throw new EmulatorException(EmulatorErrorCode.AlreadyRunning);
						}
						using (EventWaitHandle eventWaitHandle2 = new EventWaitHandle(false, EventResetMode.ManualReset, "DevelopmentStorage-3D75486A-E34F-447c-BF4B-A35284FA8D96"))
						{
							ServiceController.ServiceStatusChanged += new StatusEventHandler(Program.LogServiceStatusChange);
							ServiceController.InitializeServices((string msg) => Console.Error.WriteLine(msg), new StorageContext());
							Thread thread = new Thread(() => Program.HandleUserInterfaceEvents())
							{
								IsBackground = true
							};
							thread.Start();
							eventWaitHandle.Set();
							flag1 = true;
							eventWaitHandle2.WaitOne();
							ServiceController.DestroyServices();
						}
					}
					finally
					{
						if (!flag1)
						{
							eventWaitHandle1.Set();
						}
					}
				}
			}
		}

		private static void LogServiceStatusChange(ServiceStatusInfo status)
		{
			object[] serviceName = new object[] { status.ServiceName, status.ServiceEndpoint, status.IsRunning, status.Error };
			Console.WriteLine("Service Status: {0} {1} {2} {3}", serviceName);
		}

		[STAThread]
		public static int Main(string[] args)
		{
			int emulatorErrorCode;
			Program.PrintBanner();
			if ((int)args.Length <= 0)
			{
				Console.Error.WriteLine("Error: Expected command as first argument.");
				Program.PrintGeneralHelp();
				return -2;
			}
			string str = args[0];
			bool flag = false;
			if (str.Equals("help", StringComparison.InvariantCultureIgnoreCase))
			{
				if ((int)args.Length < 2)
				{
					Program.PrintGeneralHelp();
					return 0;
				}
				str = args[1];
				flag = true;
			}
			Command command = Program.Commands.FirstOrDefault<Command>((Command c) => c.CommandName.Equals(str, StringComparison.InvariantCultureIgnoreCase));
			if (command == null)
			{
				Console.Error.WriteLine("Error: Unknown command '{0}'.", str);
				Program.PrintGeneralHelp();
				return -2;
			}
			if (flag)
			{
				command.PrintHelp();
				return 0;
			}
			int num = 1;
			while (true)
			{
				if (num < (int)args.Length)
				{
					string str1 = args[num];
					try
					{
						if (str1.StartsWith("/") || str1.StartsWith("-"))
						{
							str1 = str1.Substring(1);
							bool flag1 = false;
							if (str1.Equals("primarykey", StringComparison.InvariantCultureIgnoreCase) || str1.Equals("secondarykey", StringComparison.InvariantCultureIgnoreCase))
							{
								flag1 = true;
							}
							if (num + 1 >= (int)args.Length || !flag1 && args[num + 1].StartsWith("/") || args[num + 1].StartsWith("-"))
							{
								command.SetFlag(str1);
							}
							else
							{
								num++;
								command.SetArgument(str1, args[num]);
							}
						}
						else
						{
							command.AddPositional(str1);
						}
					}
					catch (EmulatorException emulatorException1)
					{
						EmulatorException emulatorException = emulatorException1;
						Console.Error.WriteLine("Error parsing '{0}': {1}", str1, emulatorException.Message);
						command.PrintHelp();
						emulatorErrorCode = (int)emulatorException.EmulatorErrorCode;
						break;
					}
					num++;
				}
				else
				{
					try
					{
						command.RunCommand();
					}
					catch (EmulatorException emulatorException3)
					{
						EmulatorException emulatorException2 = emulatorException3;
						Console.Error.WriteLine("Error: {0}", emulatorException2.Message);
						emulatorErrorCode = (int)emulatorException2.EmulatorErrorCode;
						break;
					}
					return 0;
				}
			}
			return emulatorErrorCode;
		}

		public static void PrintAccountManagementHelp()
		{
			string moduleName = Process.GetCurrentProcess().MainModule.ModuleName;
			Console.WriteLine("    {0} createaccount   : Create an account for the emulator.", moduleName);
			Console.WriteLine("    {0} deleteaccount   : Deletes the specified storage account.", moduleName);
			Console.WriteLine("    {0} getaccount      : Returns information for the specified storage account.", moduleName);
			Console.WriteLine("    {0} listaccounts    : Lists all the available storage accounts.", moduleName);
			Console.WriteLine("    {0} regeneratekey   : Regenerates the primary or secondary key for the specified account.", moduleName);
			Console.WriteLine("    {0} updateaccount   : Updates given properties for the specified account.", moduleName);
		}

		public static void PrintBanner()
		{
			Console.WriteLine("Windows Azure Storage Emulator {0} command line tool", "5.2.0.0");
		}

		public static void PrintGeneralHelp()
		{
			string moduleName = Process.GetCurrentProcess().MainModule.ModuleName;
			Console.WriteLine("Usage:");
			Console.WriteLine("    {0} init            : Initialize the emulator database and configuration.", moduleName);
			Console.WriteLine("    {0} start           : Start the emulator.", moduleName);
			Console.WriteLine("    {0} stop            : Stop the emulator.", moduleName);
			Console.WriteLine("    {0} status          : Get current emulator status.", moduleName);
			Console.WriteLine("    {0} clear           : Delete all data in the emulator.", moduleName);
			Console.WriteLine("    {0} help [command]  : Show general or command-specific help.", moduleName);
			Console.WriteLine();
			Console.WriteLine("See the following URL for more command line help: {0}", "http://go.microsoft.com/fwlink/?LinkId=392235");
		}
	}
}