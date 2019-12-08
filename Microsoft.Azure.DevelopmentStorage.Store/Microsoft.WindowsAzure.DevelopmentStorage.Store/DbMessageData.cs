using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbMessageData : IMessageData
	{
		private QueueMessage m_message;

		private bool m_isPeekMessage;

		int Microsoft.Cis.Services.Nephos.Common.Storage.IMessageData.DequeueCount
		{
			get
			{
				return this.m_message.DequeueCount;
			}
		}

		DateTime Microsoft.Cis.Services.Nephos.Common.Storage.IMessageData.ExpiryTime
		{
			get
			{
				return this.m_message.ExpiryTime;
			}
		}

		Guid Microsoft.Cis.Services.Nephos.Common.Storage.IMessageData.Id
		{
			get
			{
				return this.m_message.MessageId;
			}
		}

		DateTime Microsoft.Cis.Services.Nephos.Common.Storage.IMessageData.InsertionTime
		{
			get
			{
				return this.m_message.InsertionTime;
			}
		}

		byte[] Microsoft.Cis.Services.Nephos.Common.Storage.IMessageData.Message
		{
			get
			{
				return this.m_message.Data;
			}
		}

		DateTime Microsoft.Cis.Services.Nephos.Common.Storage.IMessageData.VisibilityStart
		{
			get
			{
				if (this.m_isPeekMessage)
				{
					return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
				}
				return this.m_message.VisibilityStartTime;
			}
		}

		public DbMessageData(QueueMessage message, bool isPeekMessage)
		{
			this.m_message = message;
			this.m_isPeekMessage = isPeekMessage;
		}
	}
}