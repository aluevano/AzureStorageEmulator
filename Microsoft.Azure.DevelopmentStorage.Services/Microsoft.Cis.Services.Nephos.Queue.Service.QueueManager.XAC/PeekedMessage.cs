using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public class PeekedMessage
	{
		private string id;

		private byte[] body;

		private DateTime insertionTime;

		private DateTime expiryTime;

		private int dequeueCount;

		public byte[] Body
		{
			get
			{
				return this.body;
			}
		}

		public int DequeueCount
		{
			get
			{
				return this.dequeueCount;
			}
		}

		public DateTime ExpiryTime
		{
			get
			{
				return this.expiryTime;
			}
		}

		public string Id
		{
			get
			{
				return this.id;
			}
		}

		public DateTime InsertionTime
		{
			get
			{
				return this.insertionTime;
			}
		}

		public PeekedMessage(string id, byte[] body, DateTime insertionTime, DateTime expiryTime, int dequeueCount)
		{
			this.id = id;
			this.body = body;
			this.insertionTime = insertionTime;
			this.expiryTime = expiryTime;
			this.dequeueCount = dequeueCount;
		}
	}
}