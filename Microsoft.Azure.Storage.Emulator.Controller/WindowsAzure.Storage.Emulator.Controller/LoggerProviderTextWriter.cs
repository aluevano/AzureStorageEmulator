using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.IO;
using System.Text;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal class LoggerProviderTextWriter : TextWriter
	{
		public override System.Text.Encoding Encoding
		{
			get
			{
				return System.Text.Encoding.Default;
			}
		}

		public LoggerProviderTextWriter()
		{
		}

		public override void Write(string format)
		{
			IStringDataEventStream logger = (IStringDataEventStream)LoggerProvider.Instance.GetLogger(typeof(IStringDataEventStream), "LinqToSQL");
			string[] strArrays = format.Split(new char[] { '\n' });
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				logger.Log(strArrays[i]);
			}
		}

		public override void Write(char value)
		{
			this.Write(value.ToString());
		}

		public override void WriteLine(string format)
		{
			this.Write(format);
		}
	}
}