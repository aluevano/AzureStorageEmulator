using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface IFrontEndPerfMetricsEventsStream
	{
		void LogMetric(IFrontEndPerfMetricsEventSettings settings, string account, string operation, string container, string status, long requestHeaderSize, long requestSize, long responseHeaderSize, long responseSize, long errorResponseByte, long timeInMs, long processingTimeInMs, long userTimeoutInMs, long operationTimeoutInMs, long maxAllowedTimeoutInMs, long slaUsedInMs, long readLatencyInMs, long writeLatencyInMs, string clientIP, string userAgent, string requestVersion, string processorVersionUsed, string requestUrl, string clientRequestId, string measurementStatus, long httpStatusCode, long totalFeTimeInMs, long totalTableServerTimeInMs, long totalTableServerRoundTripCount, long totalPartitionWaitTimeInMs, long totalXStreamTimeInMs, long totalXStreamRoundTripCount, long smbOpLockBreakLatency, string lastTableServerInstanceName, string lastTableServerErrorCode, string internalStatus, string requestContentType, string responseContentType, string partitionKey, long itemsReturned, long batchOperationCount, string lastXStreamErrorCode, string authenticationType, long accountConcurrentReq, long overallConcurrentReq, string range, string entityType, bool isSrpOperation, string lastTableServerPartition);
	}
}