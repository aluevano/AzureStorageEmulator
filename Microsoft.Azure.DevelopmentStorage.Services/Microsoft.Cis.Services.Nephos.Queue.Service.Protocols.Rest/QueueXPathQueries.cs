using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	internal static class QueueXPathQueries
	{
		public readonly static string SelectMessageTextQuery;

		public readonly static string SelectMessagesListQuery;

		public readonly static string SelectAllMessagesQuery;

		static QueueXPathQueries()
		{
			string[] strArrays = new string[] { "", "", "QueueMessage", "MessageText" };
			QueueXPathQueries.SelectMessageTextQuery = string.Join("/", strArrays);
			string[] strArrays1 = new string[] { "", "", "QueueMessagesList" };
			QueueXPathQueries.SelectMessagesListQuery = string.Join("/", strArrays1);
			string[] strArrays2 = new string[] { "", "", "QueueMessagesList", "QueueMessage" };
			QueueXPathQueries.SelectAllMessagesQuery = string.Join("/", strArrays2);
		}
	}
}