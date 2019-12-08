using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class UpdateAccountCommand : Command
	{
		public string ArgumentAccountName
		{
			get;
			set;
		}

		public string ArgumentSecondaryReadEnabled
		{
			get;
			set;
		}

		public override string CommandName
		{
			get
			{
				return "updateaccount";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public UpdateAccountCommand()
		{
		}

		public override void PrintHelp()
		{
			object[] moduleName = new object[] { Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName, "{", "}" };
			Console.WriteLine("{0} {1} -accountname accountName [-secondaryreadenabled {2}true|false{3}]", moduleName);
			Console.WriteLine("Updates given properties for the specified account.");
			Console.WriteLine("    -accountname accountName : Specifies the name for storage account. 3-24 characters long, numbers and lower-case letters only.");
			Console.WriteLine("    -secondaryreadenabled {true|false} : Specifies whether secondary read is enabled for the storage account. Current value will be kept if not specified.");
		}

		public override void RunCommand()
		{
			EmulatorStorageAccount emulatorStorageAccount;
			bool flag;
			try
			{
				bool? nullable = null;
				if (this.ArgumentSecondaryReadEnabled != null)
				{
					if (!bool.TryParse(this.ArgumentSecondaryReadEnabled, out flag))
					{
						throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, "Argument secodnaryReadEnabled should be true or false.");
					}
					nullable = new bool?(flag);
				}
				emulatorStorageAccount = EmulatorInstance.CommonInstance.UpdateAccount(this.ArgumentAccountName, nullable);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, argumentException.Message, argumentException);
			}
			string successUpdateAccount = Resource.SuccessUpdateAccount;
			object[] accountName = new object[] { emulatorStorageAccount.AccountName, emulatorStorageAccount.PrimaryKey, emulatorStorageAccount.SecondaryKey, emulatorStorageAccount.SecondaryReadEnabled };
			Console.WriteLine(successUpdateAccount, accountName);
		}
	}
}