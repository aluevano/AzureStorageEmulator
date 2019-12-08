using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal class RegenerateKeyCommand : Command
	{
		public string ArgumentAccountName
		{
			get;
			set;
		}

		public string ArgumentKeyType
		{
			get;
			set;
		}

		public override string CommandName
		{
			get
			{
				return "regeneratekey";
			}
		}

		public override int PositionalParameterCount
		{
			get
			{
				return 0;
			}
		}

		public RegenerateKeyCommand()
		{
		}

		public override void PrintHelp()
		{
			object[] moduleName = new object[] { Process.GetCurrentProcess().MainModule.ModuleName, this.CommandName, "{", "}" };
			Console.WriteLine("{0} {1} -accountname accountName -keytype {2}primary|secondary{3}", moduleName);
			Console.WriteLine("Regenerates the primary or secondary key for the specified account.");
			Console.WriteLine("    -accountname accountName : Specifies the name for storage account. 3-24 characters long, numbers and lower-case letters only.");
			Console.WriteLine("    -keytype {primary|secondary} : Specifies whether a key to be regenerated is primary or secondary.");
		}

		public override void RunCommand()
		{
			string[] strArrays;
			KeyType keyType;
			try
			{
				string lowerInvariant = this.ArgumentKeyType.ToLowerInvariant();
				string str = lowerInvariant;
				if (lowerInvariant != null)
				{
					if (str == "primary")
					{
						keyType = KeyType.Primary;
					}
					else
					{
						if (str != "secondary")
						{
							throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, string.Format("Unexpected key type in regenerate key command: {0}", this.ArgumentKeyType));
						}
						keyType = KeyType.Secondary;
					}
					strArrays = EmulatorInstance.CommonInstance.RegenerateKey(this.ArgumentAccountName, keyType);
					Console.WriteLine(Resource.SuccessRegenerateKey, this.ArgumentAccountName, strArrays[0], strArrays[1]);
					return;
				}
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, string.Format("Unexpected key type in regenerate key command: {0}", this.ArgumentKeyType));
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, argumentException.Message, argumentException);
			}
			Console.WriteLine(Resource.SuccessRegenerateKey, this.ArgumentAccountName, strArrays[0], strArrays[1]);
		}
	}
}