using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class DbQueueManager : RealQueueManager
	{
		public DbQueueManager(IStorageManager storageMgr, AuthorizationManager authMgr) : base(storageMgr, authMgr, QueueManagerConfiguration.GetDefaultConfiguration())
		{
		}

		protected override IQueueMessageReceipt DecodeReceipt(byte[] data, Guid id)
		{
			return Receipt.DeSerialize(id, data);
		}

		protected override byte[] EncodeReceipt(IMessageData message)
		{
			return Receipt.Serialize(message);
		}

		protected override PoppedMessage GetPoppedMessageInfoFromReceipt(IQueueMessageReceipt popreceipt)
		{
			Receipt receipt = (Receipt)popreceipt;
			Guid messageId = receipt.MessageId;
			return new PoppedMessage(messageId.ToString(), null, DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc), receipt.VisibilityStart, 1, Receipt.Serialize(receipt));
		}
	}
}