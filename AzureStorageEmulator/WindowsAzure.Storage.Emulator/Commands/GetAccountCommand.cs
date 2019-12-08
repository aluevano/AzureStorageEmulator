using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class GetAccountCommand : Command
	{
		public override string CommandName
		{
			get
			{
				return "getaccount";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 1;
			}
		}

		public GetAccountCommand()
		{
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1} accountName", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Returns information for the specified storage account.");
		}

		public override void RunCommand()
		{
			if (base.Positionals.Count == 0)
			{
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, "Account name should be specified.");
			}
			string str = base.Positionals.Single<string>();
			EmulatorStorageAccount account = EmulatorInstance.CommonInstance.GetAccount(str);
			object[] accountName = new object[] { account.AccountName, account.PrimaryKey, account.SecondaryKey, account.SecondaryReadEnabled };
			Console.WriteLine("Account Name: {0}\nPrimary Key: {1}\nSecondary Key: {2}\nSecondary Read Enabled: {3}", accountName);
		}
	}
}