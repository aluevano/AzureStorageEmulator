using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IMessageData
	{
		int DequeueCount
		{
			get;
		}

		DateTime ExpiryTime
		{
			get;
		}

		Guid Id
		{
			get;
		}

		DateTime InsertionTime
		{
			get;
		}

		byte[] Message
		{
			get;
		}

		DateTime VisibilityStart
		{
			get;
		}
	}
}