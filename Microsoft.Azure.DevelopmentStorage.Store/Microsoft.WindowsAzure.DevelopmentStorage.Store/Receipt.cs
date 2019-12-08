using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class Receipt : IQueueMessageReceipt
	{
		private const int receiptByteLen = 12;

		public int DequeueCount
		{
			get;
			set;
		}

		public Guid MessageId
		{
			get;
			set;
		}

		public DateTime VisibilityStart
		{
			get;
			internal set;
		}

		public Receipt()
		{
		}

		public static Receipt DeSerialize(Guid id, byte[] popReceipt)
		{
			if ((int)popReceipt.Length != 12)
			{
				throw new PopReceiptMismatchException();
			}
			long[] numArray = new long[1];
			int[] numArray1 = new int[1];
			Buffer.BlockCopy(popReceipt, 0, numArray, 0, 8);
			Buffer.BlockCopy(popReceipt, 8, numArray1, 0, 4);
			Receipt receipt = new Receipt()
			{
				DequeueCount = numArray1[0],
				VisibilityStart = new DateTime(numArray[0], DateTimeKind.Utc),
				MessageId = id
			};
			return receipt;
		}

		public static byte[] Serialize(IMessageData message)
		{
			Receipt receipt = new Receipt()
			{
				MessageId = message.Id,
				VisibilityStart = message.VisibilityStart,
				DequeueCount = message.DequeueCount
			};
			return Receipt.Serialize(receipt);
		}

		public static byte[] Serialize(Receipt receiptObj)
		{
			byte[] numArray = new byte[12];
			long[] ticks = new long[] { receiptObj.VisibilityStart.Ticks };
			int[] dequeueCount = new int[] { receiptObj.DequeueCount };
			Buffer.BlockCopy(ticks, 0, numArray, 0, 8);
			Buffer.BlockCopy(dequeueCount, 0, numArray, 8, 4);
			return numArray;
		}
	}
}