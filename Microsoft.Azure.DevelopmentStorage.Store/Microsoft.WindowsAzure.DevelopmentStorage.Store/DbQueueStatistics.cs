using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbQueueStatistics : IQueueStatistics
	{
		public long TotalMessages
		{
			get
			{
				return JustDecompileGenerated_get_TotalMessages();
			}
			set
			{
				JustDecompileGenerated_set_TotalMessages(value);
			}
		}

		private long JustDecompileGenerated_TotalMessages_k__BackingField;

		public long JustDecompileGenerated_get_TotalMessages()
		{
			return this.JustDecompileGenerated_TotalMessages_k__BackingField;
		}

		public void JustDecompileGenerated_set_TotalMessages(long value)
		{
			this.JustDecompileGenerated_TotalMessages_k__BackingField = value;
		}

		public long TotalSize
		{
			get
			{
				return JustDecompileGenerated_get_TotalSize();
			}
			set
			{
				JustDecompileGenerated_set_TotalSize(value);
			}
		}

		private long JustDecompileGenerated_TotalSize_k__BackingField;

		public long JustDecompileGenerated_get_TotalSize()
		{
			return this.JustDecompileGenerated_TotalSize_k__BackingField;
		}

		public void JustDecompileGenerated_set_TotalSize(long value)
		{
			this.JustDecompileGenerated_TotalSize_k__BackingField = value;
		}

		public DbQueueStatistics(long totalMessages, long totalSize)
		{
			this.TotalMessages = totalMessages;
			this.TotalSize = totalSize;
		}
	}
}