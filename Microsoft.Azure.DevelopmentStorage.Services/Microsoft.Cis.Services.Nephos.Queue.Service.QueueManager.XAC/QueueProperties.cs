using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public class QueueProperties
	{
		private string account;

		private string name;

		private long? visibleMessageCount;

		private NameValueCollection metadata;

		public string Account
		{
			get
			{
				return this.account;
			}
		}

		public NameValueCollection Metadata
		{
			get
			{
				return this.metadata;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public long? VisibleMessageCount
		{
			get
			{
				return this.visibleMessageCount;
			}
		}

		public QueueProperties(string account, string name, long? visibleMessageCount, NameValueCollection metadata)
		{
			this.account = account;
			this.name = name;
			this.visibleMessageCount = visibleMessageCount;
			this.metadata = metadata;
		}

		public static QueueProperties Create(string account, string name, long? visibleMessageCount, NameValueCollection metadata)
		{
			return new QueueProperties(account, name, visibleMessageCount, metadata);
		}
	}
}