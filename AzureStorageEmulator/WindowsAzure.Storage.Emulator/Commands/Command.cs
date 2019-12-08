using Microsoft.WindowsAzure.Storage.Emulator.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Commands
{
	internal abstract class Command
	{
		protected const string PropertyPrefix = "Argument";

		public abstract string CommandName
		{
			get;
		}

		public abstract int PositionalParameterCount
		{
			get;
		}

		protected List<string> Positionals
		{
			get;
			private set;
		}

		public Command()
		{
			this.Positionals = new List<string>();
		}

		public void AddPositional(string positionalValue)
		{
			if (this.Positionals.Count >= this.PositionalParameterCount)
			{
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, "Too many command line arguments provided.");
			}
			this.Positionals.Add(positionalValue);
		}

		public abstract void PrintHelp();

		public abstract void RunCommand();

		public void SetArgument(string argumentName, string argumentValue)
		{
			this.SetProperty(argumentName, argumentValue);
		}

		public void SetFlag(string flagName)
		{
			this.SetProperty(flagName, true);
		}

		private void SetProperty(string propertyName, object propertyValue)
		{
			PropertyInfo propertyInfo = this.GetType().GetProperties().FirstOrDefault<PropertyInfo>((PropertyInfo prop) => prop.Name.Equals(string.Concat("Argument", propertyName), StringComparison.InvariantCultureIgnoreCase));
			if (propertyInfo == null)
			{
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, "Unexpected argument encountered.");
			}
			if (propertyInfo.PropertyType != propertyValue.GetType())
			{
				if (propertyInfo.PropertyType != true.GetType())
				{
					throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, "You must specify a parameter string after this argument.");
				}
				throw new EmulatorException(EmulatorErrorCode.CommandLineParsingFailed, string.Format("Found unexpected argument text: '{0}'", propertyValue));
			}
			propertyInfo.SetValue(this, propertyValue, null);
		}
	}
}