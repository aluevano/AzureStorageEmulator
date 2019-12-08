using AsyncHelper;
using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class NephosErrorDetails
	{
		public NameValueCollection AdditionalDetails
		{
			get;
			set;
		}

		public string DetailCode
		{
			get;
			set;
		}

		public string DetailMessage
		{
			get;
			set;
		}

		public Exception ErrorException
		{
			get;
			set;
		}

		public byte[] ErrorResponse
		{
			get;
			set;
		}

		public MeasurementEventStatus EventStatus
		{
			get;
			set;
		}

		public bool HasErrorResponse
		{
			get;
			set;
		}

		public bool IsFatal
		{
			get;
			set;
		}

		public NameValueCollection ResponseHeaders
		{
			get;
			set;
		}

		public bool SkipBillingAndMetrics
		{
			get;
			set;
		}

		public NephosStatusEntry StatusEntry
		{
			get;
			set;
		}

		public string UserSafeErrorMessage
		{
			get;
			set;
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus) : this(statusEntry, eventStatus, null)
		{
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus, bool skipBillingAndMetrics) : this(statusEntry, eventStatus, null, null, null, null, null, false, false, skipBillingAndMetrics)
		{
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus, Exception exception) : this(statusEntry, eventStatus, exception, null, null)
		{
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus, Exception exception, NameValueCollection headers, NameValueCollection additionalDetails) : this(statusEntry, eventStatus, exception, headers, additionalDetails, null, false, false)
		{
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus, byte[] errorResponse, bool hasErrorResponse) : this(statusEntry, eventStatus, null, null, null, errorResponse, false, hasErrorResponse)
		{
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus, Exception exception, string userSafeErrorMessage) : this(statusEntry, eventStatus, exception, userSafeErrorMessage, null, null, null, false, false, false)
		{
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus, Exception exception, NameValueCollection headers, NameValueCollection additionalDetails, byte[] errorResponse, bool isFatal, bool hasErrorResponse) : this(statusEntry, eventStatus, exception, null, headers, additionalDetails, errorResponse, isFatal, hasErrorResponse, false)
		{
		}

		public NephosErrorDetails(NephosStatusEntry statusEntry, MeasurementEventStatus eventStatus, Exception exception, string userSafeErrorMessage, NameValueCollection headers, NameValueCollection additionalDetails, byte[] errorResponse, bool isFatal, bool hasErrorResponse, bool skipBillingAndMetrics)
		{
			NephosAssertionException.Assert(statusEntry != null, "statusEntry can't be null.");
			NephosAssertionException.Assert(true, "eventStatus can't be null.");
			this.StatusEntry = statusEntry;
			this.EventStatus = eventStatus;
			this.ErrorException = exception;
			this.ResponseHeaders = headers;
			this.AdditionalDetails = additionalDetails;
			this.ErrorResponse = errorResponse;
			this.IsFatal = isFatal;
			this.HasErrorResponse = hasErrorResponse;
			this.SkipBillingAndMetrics = skipBillingAndMetrics;
			StringBuilder stringBuilder = new StringBuilder(statusEntry.UserMessage);
			if (!string.IsNullOrEmpty(userSafeErrorMessage))
			{
				stringBuilder.AppendFormat(" Additional detail: {0}", userSafeErrorMessage);
			}
			Guid activityId = Trace.ActivityId;
			if (activityId != Guid.Empty)
			{
				string str = activityId.ToString("D");
				DateTime utcNow = DateTime.UtcNow;
				stringBuilder.AppendFormat("\nRequestId:{0}\nTime:{1}", str, utcNow.ToString("o"));
			}
			this.UserSafeErrorMessage = stringBuilder.ToString();
		}
	}
}