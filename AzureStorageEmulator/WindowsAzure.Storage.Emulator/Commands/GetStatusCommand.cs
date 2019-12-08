using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class GetStatusCommand : Command
	{
		public override string CommandName
		{
			get
			{
				return "status";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public GetStatusCommand()
		{
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1}", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Prints the status of the storage emulator.");
		}

		public override void RunCommand()
		{
			EmulatorStatus status = EmulatorInstance.CommonInstance.Status;
			Console.WriteLine("IsRunning: {0}", status.IsRunning);
			Console.WriteLine("BlobEndpoint: {0}", status.BlobEndpoint);
			Console.WriteLine("QueueEndpoint: {0}", status.QueueEndpoint);
			Console.WriteLine("TableEndpoint: {0}", status.TableEndpoint);
		}
	}
}