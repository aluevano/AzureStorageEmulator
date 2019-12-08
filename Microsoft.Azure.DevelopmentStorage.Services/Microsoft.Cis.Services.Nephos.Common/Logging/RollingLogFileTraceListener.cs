using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public class RollingLogFileTraceListener : TraceListener
	{
		private const string BaseFileNameAttributeName = "baseFileName";

		private const string MaxFileSizeAttributeName = "maxFileSize";

		private const string MaxFilesAttributeName = "maxFiles";

		private RollingLogStreamWriter streamWriter;

		private string baseFileName;

		private long? maxFileSize;

		private int? maxFiles;

		private object syncObj = new object();

		public string BaseFileName
		{
			get
			{
				if (this.baseFileName == null && base.Attributes.ContainsKey("baseFileName"))
				{
					this.baseFileName = base.Attributes["baseFileName"];
				}
				if (this.baseFileName == null)
				{
					throw new InvalidOperationException("baseFileName must be specified");
				}
				return this.baseFileName;
			}
			set
			{
				this.baseFileName = value;
			}
		}

		public int MaxFiles
		{
			get
			{
				if (!this.maxFiles.HasValue && base.Attributes.ContainsKey("maxFiles"))
				{
					this.maxFiles = new int?(int.Parse(base.Attributes["maxFiles"], CultureInfo.InvariantCulture));
				}
				if (!this.maxFiles.HasValue)
				{
					throw new InvalidOperationException("maxFiles must be specified");
				}
				return this.maxFiles.Value;
			}
			set
			{
				this.maxFiles = new int?(value);
			}
		}

		public long MaxFileSize
		{
			get
			{
				if (!this.maxFileSize.HasValue && base.Attributes.ContainsKey("maxFileSize"))
				{
					this.maxFileSize = new long?(long.Parse(base.Attributes["maxFileSize"], CultureInfo.InvariantCulture));
				}
				if (!this.maxFileSize.HasValue)
				{
					throw new InvalidOperationException("maxFileSize must be specified");
				}
				return this.maxFileSize.Value;
			}
			set
			{
				this.maxFileSize = new long?(value);
			}
		}

		private RollingLogStreamWriter StreamWriter
		{
			get
			{
				if (this.streamWriter == null)
				{
					this.streamWriter = new RollingLogStreamWriter(this.BaseFileName, this.MaxFileSize, this.MaxFiles, 4096);
				}
				return this.streamWriter;
			}
		}

		public RollingLogFileTraceListener()
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.streamWriter != null)
			{
				this.streamWriter.Dispose();
				this.streamWriter = null;
			}
			base.Dispose(disposing);
		}

		protected override string[] GetSupportedAttributes()
		{
			return new string[] { "baseFileName", "maxFileSize", "maxFiles" };
		}

		public override void Write(string message)
		{
			this.StreamWriter.Write(message);
			this.StreamWriter.Flush();
		}

		public override void WriteLine(string message)
		{
			this.StreamWriter.WriteLine(message);
			this.StreamWriter.Flush();
		}
	}
}