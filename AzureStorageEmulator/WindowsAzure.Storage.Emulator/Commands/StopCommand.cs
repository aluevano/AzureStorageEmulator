using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class StopCommand : Command
	{
		public override string CommandName
		{
			get
			{
				return "stop";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public StopCommand()
		{
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1}", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Stops the storage emulator.");
		}

		public override void RunCommand()
		{
			EmulatorInstance.CommonInstance.Stop();
			Console.WriteLine(Resource.SuccessStop);
		}
	}
}