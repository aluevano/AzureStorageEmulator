using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public struct QueueManagerConfiguration
	{
		public long DefaultVisibilityTimeoutSeconds
		{
			get;
			set;
		}

		public int MaxMessagesToReturn
		{
			get;
			set;
		}

		public int MaxMessagesToReturnForListMessages
		{
			get;
			set;
		}

		public long MaxTtlSeconds
		{
			get;
			set;
		}

		public long MaxVisibilityTimeoutSeconds
		{
			get;
			set;
		}

		public long MinVisibilityTimeoutSeconds
		{
			get;
			set;
		}

		public static QueueManagerConfiguration GetDefaultConfiguration()
		{
			QueueManagerConfiguration queueManagerConfiguration = new QueueManagerConfiguration()
			{
				MaxTtlSeconds = (long)604800,
				MaxMessagesToReturn = 32,
				DefaultVisibilityTimeoutSeconds = (long)30,
				MaxVisibilityTimeoutSeconds = (long)7200,
				MinVisibilityTimeoutSeconds = (long)1,
				MaxMessagesToReturnForListMessages = 5000
			};
			return queueManagerConfiguration;
		}
	}
}