using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class ListAccountsCommand : Command
	{
		public override string CommandName
		{
			get
			{
				return "listaccounts";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public ListAccountsCommand()
		{
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1}", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Prints information of all the available storage accounts.");
		}

		public override void RunCommand()
		{
			foreach (EmulatorStorageAccount emulatorStorageAccount in EmulatorInstance.CommonInstance.ListAccounts())
			{
				object[] accountName = new object[] { emulatorStorageAccount.AccountName, emulatorStorageAccount.PrimaryKey, emulatorStorageAccount.SecondaryKey, emulatorStorageAccount.SecondaryReadEnabled };
				Console.WriteLine("Account Name: {0}\nPrimary Key: {1}\nSecondary Key: {2}\nSecondary Read Enabled: {3}\n", accountName);
			}
		}
	}
}