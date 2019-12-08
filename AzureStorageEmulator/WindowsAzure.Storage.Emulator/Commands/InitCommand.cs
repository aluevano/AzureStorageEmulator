using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class InitCommand : Command
	{
		public bool ArgumentAutoDetect
		{
			get;
			set;
		}

		public bool ArgumentForceCreate
		{
			get;
			set;
		}

		public bool ArgumentInProcess
		{
			get;
			set;
		}

		public bool ArgumentReservePorts
		{
			get;
			set;
		}

		public string ArgumentServer
		{
			get;
			set;
		}

		public bool ArgumentSkipCreate
		{
			get;
			set;
		}

		public string ArgumentSqlInstance
		{
			get;
			set;
		}

		public bool ArgumentUnreservePorts
		{
			get;
			set;
		}

		public override string CommandName
		{
			get
			{
				return "init";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public InitCommand()
		{
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1} [-server serverName] [-sqlinstance instanceName] [-forcecreate|-skipcreate] [-reserveports|-unreserveports]", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Performs one-time initialization to set up the emulator.");
			Console.WriteLine("    -server serverName : Specifies the server hosting the SQL instance.");
			Console.WriteLine("    -sqlinstance instanceName : Specifies the name of the SQL instance to be used.");
			Console.WriteLine("    -forcecreate : Forces creation of the SQL database, even if it already exists.");
			Console.WriteLine("    -skipcreate : Skips creation of the SQL database. This takes precedent over -forcecreate.");
			Console.WriteLine("    -reserveports : Attempts to reserve the HTTP ports associated with the services.");
			Console.WriteLine("    -unreserveports : Attempts to remove reservations for the HTTP ports associated with the services. This takes precedent over -reserveports.");
			Console.WriteLine("    -inprocess : Performs initialization in the current process instead of spawning a new process. This requires the current process to have been launched with elevated permissions.");
		}

		public override void RunCommand()
		{
			EmulatorInstance.CommonInstance.Initialize(this.ArgumentServer, this.ArgumentSqlInstance, this.ArgumentForceCreate, this.ArgumentSkipCreate, this.ArgumentAutoDetect, this.ArgumentInProcess, this.ArgumentReservePorts, this.ArgumentUnreservePorts);
			Console.WriteLine(Resource.SuccessInit);
		}
	}
}