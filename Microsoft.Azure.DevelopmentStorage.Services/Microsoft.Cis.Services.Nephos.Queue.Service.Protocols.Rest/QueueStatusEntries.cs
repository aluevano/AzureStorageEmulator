using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public static class QueueStatusEntries
	{
		public readonly static NephosStatusEntry QueueNotFound;

		public readonly static NephosStatusEntry QueueDisabled;

		public readonly static NephosStatusEntry QueueAlreadyExists;

		public readonly static NephosStatusEntry QueueBeingDeleted;

		public readonly static NephosStatusEntry QueueNotEmpty;

		public readonly static NephosStatusEntry PopReceiptMismatch;

		public readonly static NephosStatusEntry MessageNotFound;

		public readonly static NephosStatusEntry MessageTooLarge;

		public readonly static NephosStatusEntry InvalidMarker;

		static QueueStatusEntries()
		{
			QueueStatusEntries.QueueNotFound = new NephosStatusEntry("QueueNotFound", HttpStatusCode.NotFound, "The specified queue does not exist.");
			QueueStatusEntries.QueueDisabled = new NephosStatusEntry("QueueDisabled", HttpStatusCode.Conflict, "The specified queue is disabled by the administrator.");
			QueueStatusEntries.QueueAlreadyExists = new NephosStatusEntry("QueueAlreadyExists", HttpStatusCode.Conflict, "The specified queue already exists.");
			QueueStatusEntries.QueueBeingDeleted = new NephosStatusEntry("QueueBeingDeleted", HttpStatusCode.Conflict, "The specified queue is being deleted.");
			QueueStatusEntries.QueueNotEmpty = new NephosStatusEntry("QueueNotEmpty", HttpStatusCode.Conflict, "The specified queue is not empty.");
			QueueStatusEntries.PopReceiptMismatch = new NephosStatusEntry("PopReceiptMismatch", HttpStatusCode.BadRequest, "The specified pop receipt did not match.");
			QueueStatusEntries.MessageNotFound = new NephosStatusEntry("MessageNotFound", HttpStatusCode.NotFound, "The specified message does not exist.");
			QueueStatusEntries.MessageTooLarge = new NephosStatusEntry("MessageTooLarge", HttpStatusCode.BadRequest, "The message exceedes the maximum allowed size. The encoded message can be up to 64KB in size for versions 2011-08-18 and newer, or 8KB in size for previous versions.");
			QueueStatusEntries.InvalidMarker = new NephosStatusEntry("InvalidMarker", HttpStatusCode.BadRequest, "The specified marker is invalid.");
		}
	}
}