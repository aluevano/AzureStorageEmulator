using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class DeleteAccountCommand : Command
	{
		public override string CommandName
		{
			get
			{
				return "deleteaccount";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 1;
			}
		}

		public DeleteAccountCommand()
		{
		}

		public override void PrintHelp()
		{
			Console.WriteLine("{0} {1} accountName", Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName);
			Console.WriteLine("Deletes the specified storage account.");
		}

		public override void RunCommand()
		{
			if (base.Positionals.Count == 0)
			{
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, "Account name should be specified.");
			}
			string str = base.Positionals.Single<string>();
			try
			{
				EmulatorInstance.CommonInstance.DeleteAccount(str);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, argumentException.Message, argumentException);
			}
			Console.WriteLine(Resource.SuccessDeleteAccount, str);
		}
	}
}