using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public class RollingLogStreamWriter : TextWriter
	{
		private readonly int maxFiles;

		private readonly long maxFileSize;

		private readonly string baseFileName;

		private readonly int bufferSize;

		private RollingLogStreamWriter.StreamWriterRollingHelper rollingHelper;

		private readonly System.Text.Encoding encoding;

		public string BaseFileName
		{
			get
			{
				return this.baseFileName;
			}
		}

		public override System.Text.Encoding Encoding
		{
			get
			{
				return this.encoding;
			}
		}

		public RollingLogStreamWriter(string baseFileName, long maxFileSize, int maxFiles, int bufferSize) : this(baseFileName, maxFileSize, maxFiles, bufferSize, null)
		{
		}

		public RollingLogStreamWriter(string baseFileName, long maxFileSize, int maxFiles, int bufferSize, IFormatProvider formatProvider) : base(formatProvider)
		{
			string directoryName = Path.GetDirectoryName(baseFileName);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			this.baseFileName = baseFileName;
			this.maxFiles = maxFiles;
			this.maxFileSize = maxFileSize;
			this.bufferSize = bufferSize;
			this.rollingHelper = new RollingLogStreamWriter.StreamWriterRollingHelper(this);
			this.encoding = this.rollingHelper.Writer.Encoding;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && this.rollingHelper != null)
				{
					this.rollingHelper.Close();
				}
			}
			finally
			{
				this.rollingHelper = null;
				base.Dispose(disposing);
			}
		}

		public override void Flush()
		{
			this.rollingHelper.Writer.Flush();
		}

		public override void Write(char value)
		{
			this.rollingHelper.Writer.Write(value);
		}

		public override void Write(char[] buffer)
		{
			this.rollingHelper.Writer.Write(buffer);
		}

		public override void Write(char[] buffer, int index, int count)
		{
			this.rollingHelper.Writer.Write(buffer, index, count);
		}

		public override void Write(string value)
		{
			this.rollingHelper.Writer.Write(value);
		}

		private sealed class StreamWriterRollingHelper : IDisposable
		{
			private RollingLogStreamWriter owner;

			private RollingLogStreamWriter.TallyKeepingFileStreamWriter writer;

			internal StreamWriter Writer
			{
				get
				{
					this.RollIfNecessary();
					return this.writer;
				}
			}

			public StreamWriterRollingHelper(RollingLogStreamWriter owner)
			{
				this.owner = owner;
				this.UpdateRollingInformationIfNecessary();
			}

			private bool CheckIsRollNecessary()
			{
				if (this.owner.maxFileSize <= (long)0)
				{
					return false;
				}
				if (this.writer == null)
				{
					return false;
				}
				return this.writer.Tally > this.owner.maxFileSize;
			}

			public void Close()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void ComputeCurrentFileAndNextFile(out string currentFile, out string nextFile)
			{
				string baseFileName = this.owner.BaseFileName;
				string directoryName = Path.GetDirectoryName(baseFileName);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
				string extension = Path.GetExtension(baseFileName);
				int num = RollingLogStreamWriter.StreamWriterRollingHelper.FindMaxSequenceNumber(directoryName, fileNameWithoutExtension, extension);
				if (num >= 0)
				{
					object[] objArray = new object[] { fileNameWithoutExtension, "_", num, extension };
					currentFile = Path.Combine(directoryName, string.Concat(objArray));
				}
				else
				{
					currentFile = null;
				}
				object[] objArray1 = new object[] { fileNameWithoutExtension, "_", num + 1, extension };
				nextFile = Path.Combine(directoryName, string.Concat(objArray1));
			}

			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool disposing)
			{
				try
				{
					if (disposing && this.writer != null)
					{
						this.writer.Close();
					}
				}
				finally
				{
					this.writer = null;
				}
			}

			[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId="System.String.Format(System.String,System.Object,System.Object)")]
			private static int FindMaxSequenceNumber(string directoryName, string fileName, string extension)
			{
				string[] files = Directory.GetFiles(directoryName, string.Format("{0}*{1}", fileName, extension));
				int num = -1;
				Regex regex = new Regex(string.Format("{0}_(?<sequence>\\d+){1}", fileName, extension));
				for (int i = 0; i < (int)files.Length; i++)
				{
					Match match = regex.Match(files[i]);
					if (match.Success)
					{
						int num1 = 0;
						if (int.TryParse(match.Groups["sequence"].Value, out num1) && num1 > num)
						{
							num = num1;
						}
					}
				}
				return num;
			}

			private static System.Text.Encoding GetEncodingWithFallback()
			{
				System.Text.Encoding replacementFallback = (System.Text.Encoding)(new UTF8Encoding(false)).Clone();
				replacementFallback.EncoderFallback = EncoderFallback.ReplacementFallback;
				replacementFallback.DecoderFallback = DecoderFallback.ReplacementFallback;
				return replacementFallback;
			}

			private void PerformRoll()
			{
				this.writer.Close();
				this.writer = null;
				this.UpdateRollingInformationIfNecessary();
			}

			private void PurgeLogsIfNecessary()
			{
				if (this.owner.maxFiles > 0)
				{
					string baseFileName = this.owner.BaseFileName;
					string directoryName = Path.GetDirectoryName(baseFileName);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
					string extension = Path.GetExtension(baseFileName);
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { fileNameWithoutExtension, extension };
					List<string> strs = new List<string>(Directory.GetFiles(directoryName, string.Format(invariantCulture, "{0}*{1}", objArray)));
					if (strs.Count > this.owner.maxFiles)
					{
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						object[] objArray1 = new object[] { fileNameWithoutExtension, extension };
						Regex regex = new Regex(string.Format(cultureInfo, "{0}_(?<sequence>\\d+){1}", objArray1));
						List<RollingLogStreamWriter.StreamWriterRollingHelper.FileNameSequencePair> fileNameSequencePairs = new List<RollingLogStreamWriter.StreamWriterRollingHelper.FileNameSequencePair>();
						foreach (string str in strs)
						{
							Match match = regex.Match(str);
							if (!match.Success)
							{
								continue;
							}
							int num = 0;
							if (!int.TryParse(match.Groups["sequence"].Value, out num))
							{
								continue;
							}
							fileNameSequencePairs.Add(new RollingLogStreamWriter.StreamWriterRollingHelper.FileNameSequencePair(str, num));
						}
						fileNameSequencePairs.Sort((RollingLogStreamWriter.StreamWriterRollingHelper.FileNameSequencePair left, RollingLogStreamWriter.StreamWriterRollingHelper.FileNameSequencePair right) => {
							if (left.SequenceNum == right.SequenceNum)
							{
								return 0;
							}
							if (left.SequenceNum > right.SequenceNum)
							{
								return 1;
							}
							return -1;
						});
						int count = strs.Count;
						int num1 = 0;
						while (count > this.owner.maxFiles)
						{
							try
							{
								File.Delete(Path.Combine(directoryName, fileNameSequencePairs[num1].FileName));
							}
							catch (IOException oException)
							{
							}
							num1++;
							count--;
						}
					}
				}
			}

			private void RollIfNecessary()
			{
				if (this.CheckIsRollNecessary())
				{
					this.PerformRoll();
					this.PurgeLogsIfNecessary();
				}
			}

			private void UpdateRollingInformationIfNecessary()
			{
				string str;
				string str1;
				if (this.writer == null)
				{
					this.ComputeCurrentFileAndNextFile(out str, out str1);
					if (str == null || (new FileInfo(str)).Length > this.owner.maxFileSize)
					{
						str = str1;
					}
					bool exists = (new FileInfo(str)).Exists;
					FileStream fileStream = new FileStream(str, FileMode.Append, FileAccess.Write, FileShare.Read, this.owner.bufferSize, FileOptions.SequentialScan);
					this.writer = new RollingLogStreamWriter.TallyKeepingFileStreamWriter(fileStream, RollingLogStreamWriter.StreamWriterRollingHelper.GetEncodingWithFallback(), this.owner.bufferSize);
					if (exists)
					{
						this.writer.Tally = (new FileInfo(str)).Length;
					}
				}
			}

			private class FileNameSequencePair
			{
				public readonly string FileName;

				public readonly int SequenceNum;

				public FileNameSequencePair(string fileName, int sequenceNum)
				{
					this.FileName = fileName;
					this.SequenceNum = sequenceNum;
				}
			}
		}

		private sealed class TallyKeepingFileStreamWriter : StreamWriter
		{
			private long tally;

			public long Tally
			{
				get
				{
					return this.tally;
				}
				set
				{
					this.tally = value;
				}
			}

			public TallyKeepingFileStreamWriter(FileStream stream, System.Text.Encoding encoding, int bufferSize) : base(stream, encoding, bufferSize)
			{
				this.tally = stream.Length;
			}

			public override void Write(char value)
			{
				base.Write(value);
				RollingLogStreamWriter.TallyKeepingFileStreamWriter byteCount = this;
				long num = byteCount.tally;
				System.Text.Encoding encoding = this.Encoding;
				char[] chrArray = new char[] { value };
				byteCount.tally = num + (long)encoding.GetByteCount(chrArray);
			}

			public override void Write(char[] buffer)
			{
				base.Write(buffer);
				this.tally += (long)this.Encoding.GetByteCount(buffer);
			}

			public override void Write(char[] buffer, int index, int count)
			{
				base.Write(buffer, index, count);
				this.tally += (long)this.Encoding.GetByteCount(buffer, index, count);
			}

			public override void Write(string value)
			{
				base.Write(value);
				this.tally += (long)this.Encoding.GetByteCount(value);
			}
		}
	}
}