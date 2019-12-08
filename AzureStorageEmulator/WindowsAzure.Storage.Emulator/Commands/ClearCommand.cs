using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class ClearCommand : Command
	{
		public override string CommandName
		{
			get
			{
				return "clear";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 3;
			}
		}

		public ClearCommand()
		{
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1} [blob] [table] [queue] [all]", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Clears the data in all services specified on the command line.");
		}

		public override void RunCommand()
		{
			string current;
			EmulatorServiceType emulatorServiceType = EmulatorServiceType.None;
			List<string>.Enumerator enumerator = base.Positionals.GetEnumerator();
			try
			{
				while (true)
				{
					if (enumerator.MoveNext())
					{
						current = enumerator.Current;
						string lowerInvariant = current.ToLowerInvariant();
						string str = lowerInvariant;
						if (lowerInvariant == null)
						{
							break;
						}
						if (str == "blob")
						{
							emulatorServiceType |= EmulatorServiceType.Blob;
						}
						else if (str == "queue")
						{
							emulatorServiceType |= EmulatorServiceType.Queue;
						}
						else if (str == "table")
						{
							emulatorServiceType |= EmulatorServiceType.Table;
						}
						else if (str == "all")
						{
							emulatorServiceType |= EmulatorServiceType.All;
						}
						else
						{
							break;
						}
					}
					else
					{
						EmulatorInstance.CommonInstance.ClearData(emulatorServiceType);
						Console.WriteLine(Resource.SuccessClearFormat, emulatorServiceType);
						return;
					}
				}
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, string.Format("Unexpected argument to clear command: {0}", current));
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			EmulatorInstance.CommonInstance.ClearData(emulatorServiceType);
			Console.WriteLine(Resource.SuccessClearFormat, emulatorServiceType);
		}
	}
}