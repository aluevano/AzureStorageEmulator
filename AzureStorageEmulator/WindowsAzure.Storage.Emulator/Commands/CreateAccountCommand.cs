using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class CreateAccountCommand : Command
	{
		private bool secondaryReadEnabled = true;

		public string ArgumentAccountName
		{
			get;
			set;
		}

		public string ArgumentPrimaryKey
		{
			get;
			set;
		}

		public string ArgumentSecondaryKey
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
				return "createaccount";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public CreateAccountCommand()
		{
		}

		public override void PrintHelp()
		{
			object[] moduleName = new object[] { Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName, "{", "}" };
			Console.WriteLine("{0} {1} -accountname accountName [-primarykey primaryKey] [-secondarykey secondaryKey] [-secondaryreadenabled {2}true|false{3}]", moduleName);
			Console.WriteLine("Creates a new storage account with specified properties.");
			Console.WriteLine("    -accountname accountName : Specifies the name for storage account. 3-24 characters long, numbers and lower-case letters only.");
			Console.WriteLine("    -primarykey primaryKey : Specifies the Base64 encoded primary access key for the storage account. Will be generated if not specified.");
			Console.WriteLine("    -secondarykey secondaryKey : Specifies the Base64 encoded secondary access key for the storage account. Will be generated if not specified.");
			Console.WriteLine("    -secondaryreadenabled {true|false} : Specifies whether secondary read is enabled for the storage account.");
		}

		public override void RunCommand()
		{
			EmulatorStorageAccount emulatorStorageAccount;
			bool flag;
			try
			{
				if (this.ArgumentSecondaryReadEnabled == null)
				{
					this.ArgumentSecondaryReadEnabled = "true";
				}
				if (!bool.TryParse(this.ArgumentSecondaryReadEnabled, out flag))
				{
					throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, "Argument secondaryReadEnabled should be true or false.");
				}
				this.secondaryReadEnabled = flag;
				emulatorStorageAccount = EmulatorInstance.CommonInstance.CreateAccount(this.ArgumentAccountName, this.ArgumentPrimaryKey, this.ArgumentSecondaryKey, this.secondaryReadEnabled);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, argumentException.Message, argumentException);
			}
			string successCreateAccount = Resource.SuccessCreateAccount;
			object[] accountName = new object[] { emulatorStorageAccount.AccountName, emulatorStorageAccount.PrimaryKey, emulatorStorageAccount.SecondaryKey, emulatorStorageAccount.SecondaryReadEnabled };
			Console.WriteLine(successCreateAccount, accountName);
		}
	}
}