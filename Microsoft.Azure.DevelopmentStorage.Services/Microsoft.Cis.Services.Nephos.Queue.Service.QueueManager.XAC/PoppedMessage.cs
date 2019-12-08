using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public class PoppedMessage : PeekedMessage
	{
		private byte[] popReceipt;

		private DateTime timeNextVisible;

		public byte[] PopReceipt
		{
			get
			{
				return this.popReceipt;
			}
		}

		public DateTime TimeNextVisible
		{
			get
			{
				return this.timeNextVisible;
			}
		}

		public PoppedMessage(string id, byte[] body, DateTime insertionTime, DateTime expiryTime, DateTime timeNextVisible, int dequeueCount, byte[] popReceipt) : base(id, body, insertionTime, expiryTime, dequeueCount)
		{
			this.timeNextVisible = timeNextVisible;
			this.popReceipt = popReceipt;
		}

		public override string ToString()
		{
			return string.Format("{0};{1};{2}", this.TimeNextVisible, base.Id, base.ExpiryTime);
		}
	}
}