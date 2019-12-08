using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class OperationContextWithAuthAndAccountContainer : IDisposable
	{
		private IPerfTimer duration;

		private TimeSpan clientReadNetworkLatency;

		private TimeSpan clientWriteNetworkLatency;

		private TimeSpan internetRequestRoundTripLatency;

		public string AccountName
		{
			get;
			set;
		}

		public string AccountVersion
		{
			get;
			set;
		}

		public IAccountIdentifier CallerIdentity
		{
			get;
			set;
		}

		public string ConditionsUsed
		{
			get;
			set;
		}

		public string ContainerName
		{
			get;
			set;
		}

		public TimeSpan CurrentServerProcessingTime
		{
			get
			{
				return this.OperationDuration.Elapsed.Subtract(this.TotalNonServerProcessingLatency);
			}
		}

		public string ETag
		{
			get;
			set;
		}

		public ulong? FileId
		{
			get;
			set;
		}

		public TimeSpan FinalServerProcessingTime
		{
			get;
			private set;
		}

		public TimeSpan FinalTotalTime
		{
			get;
			private set;
		}

		public INephosBaseMeasurementEvent HttpRequestMeasurementEvent
		{
			get;
			set;
		}

		public bool IsAcquireLeaseOperation
		{
			get;
			set;
		}

		public bool IsBreakLeaseOperation
		{
			get;
			set;
		}

		public bool IsChangeLeaseOperation
		{
			get;
			set;
		}

		public bool IsReleaseLeaseOperation
		{
			get;
			set;
		}

		public bool IsRenewLeaseOperation
		{
			get;
			set;
		}

		public bool IsRequestAnonymous
		{
			get;
			set;
		}

		public bool IsRequestByAnalyticsAdmin
		{
			get;
			set;
		}

		public bool IsRequestByArchiveSystemKey
		{
			get;
			set;
		}

		public bool IsRequestForOSTesting
		{
			get;
			set;
		}

		public bool IsRequestSAS
		{
			get;
			set;
		}

		public bool IsResourceTypeService
		{
			get;
			set;
		}

		public bool IsRootContainerImplicit
		{
			get;
			set;
		}

		public bool IsSizeBased
		{
			get;
			set;
		}

		public TimeSpan MaxAllowedTimeout
		{
			get;
			set;
		}

		public TimeSpan OperationClientReadNetworkLatency
		{
			get
			{
				return this.clientReadNetworkLatency;
			}
			set
			{
				this.clientReadNetworkLatency = value;
			}
		}

		public TimeSpan OperationClientWriteNetworkLatency
		{
			get
			{
				return this.clientWriteNetworkLatency;
			}
			set
			{
				this.clientWriteNetworkLatency = value;
			}
		}

		public IPerfTimer OperationDuration
		{
			get
			{
				return this.duration;
			}
		}

		public TimeSpan OperationInternetRequestRoundTripLatency
		{
			get
			{
				return this.internetRequestRoundTripLatency;
			}
			set
			{
				this.internetRequestRoundTripLatency = value;
			}
		}

		public INephosBaseOperationMeasurementEvent OperationMeasurementEvent
		{
			get;
			set;
		}

		public DateTime OperationStartTime
		{
			get;
			private set;
		}

		public TimeSpan OperationTimeout
		{
			get;
			set;
		}

		public ConditionInformation RequestConditionInformation
		{
			get;
			set;
		}

		public TimeSpan SmbOpLockBreakLatency
		{
			get;
			set;
		}

		public INephosBaseOperationMeasurementEvent TemporaryGetContainerMetadataMeasurementEvent
		{
			get;
			set;
		}

		public INephosBaseOperationMeasurementEvent TemporaryGetContainerPropertiesMeasurementEvent
		{
			get;
			set;
		}

		public TimeSpan TotalNonServerProcessingLatency
		{
			get
			{
				return ((this.clientReadNetworkLatency + this.clientWriteNetworkLatency) + this.internetRequestRoundTripLatency) + this.SmbOpLockBreakLatency;
			}
		}

		public TimeSpan UserTimeout
		{
			get;
			private set;
		}

		public OperationContextWithAuthAndAccountContainer()
		{
			this.clientReadNetworkLatency = TimeSpan.Zero;
			this.clientWriteNetworkLatency = TimeSpan.Zero;
			this.internetRequestRoundTripLatency = TimeSpan.Zero;
			this.FinalTotalTime = TimeSpan.Zero;
			this.FinalServerProcessingTime = TimeSpan.Zero;
			this.UserTimeout = TimeSpan.MaxValue;
			this.OperationTimeout = TimeSpan.MaxValue;
			this.MaxAllowedTimeout = TimeSpan.MaxValue;
			this.IsRequestForOSTesting = false;
			this.IsAcquireLeaseOperation = false;
			this.IsReleaseLeaseOperation = false;
			this.IsRenewLeaseOperation = false;
			this.IsBreakLeaseOperation = false;
			this.IsRequestAnonymous = false;
			this.IsRequestSAS = false;
			this.IsRequestByAnalyticsAdmin = false;
			this.IsRequestByArchiveSystemKey = false;
			this.IsSizeBased = false;
		}

		public OperationContextWithAuthAndAccountContainer(TimeSpan elapsedTime) : this()
		{
			this.SetElapsedTime(elapsedTime);
		}

		public void Complete()
		{
			this.FinalTotalTime = this.OperationDuration.Elapsed;
			if (this.TotalNonServerProcessingLatency <= this.FinalTotalTime)
			{
				this.FinalServerProcessingTime = this.FinalTotalTime.Subtract(this.TotalNonServerProcessingLatency);
				return;
			}
			this.FinalServerProcessingTime = TimeSpan.Zero;
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] totalMilliseconds = new object[] { this.TotalNonServerProcessingLatency.TotalMilliseconds, this.FinalTotalTime.TotalMilliseconds, this.SmbOpLockBreakLatency.TotalMilliseconds };
			error.Log("TotalNonServerProcessingLatency '{0}' exceeded FinalTotalTime '{1}' with SmbOpLockBreakLatency '{2}'", totalMilliseconds);
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && this.HttpRequestMeasurementEvent != null)
			{
				this.HttpRequestMeasurementEvent.Dispose();
				this.HttpRequestMeasurementEvent = null;
			}
		}

		public TimeSpan RemainingTimeForCategorization(TimeSpan defaultOperationTimeout)
		{
			if (this.OperationTimeout == TimeSpan.MaxValue)
			{
				return this.duration.Remaining(defaultOperationTimeout);
			}
			return this.duration.Remaining(this.OperationTimeout);
		}

		public TimeSpan RemainingTimeout()
		{
			if (this.OperationTimeout == TimeSpan.MaxValue)
			{
				return TimeSpan.MaxValue;
			}
			return this.duration.Remaining(this.OperationTimeout);
		}

		public void SetElapsedTime(TimeSpan elapsedTime)
		{
			this.OperationStartTime = DateTime.UtcNow - elapsedTime;
			this.duration = PerfTimerFactory.CreateTimer(false, elapsedTime);
			this.duration.Start();
		}

		public void SetUserTimeout(TimeSpan userTimeout)
		{
			this.UserTimeout = userTimeout;
			if (this.OperationTimeout == TimeSpan.MaxValue)
			{
				this.OperationTimeout = userTimeout;
			}
		}

		public void SetUserTimeout(TimeSpan userTimeout, TimeSpan maxAllowedTimeout)
		{
			if (userTimeout > maxAllowedTimeout)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] totalMilliseconds = new object[] { (long)userTimeout.TotalMilliseconds, (long)maxAllowedTimeout.TotalMilliseconds };
				verbose.Log("Limiting user timeout '{0}'ms to max allowed timeout '{1}ms'.", totalMilliseconds);
				userTimeout = maxAllowedTimeout;
			}
			this.UserTimeout = userTimeout;
			if (this.OperationTimeout == TimeSpan.MaxValue)
			{
				this.OperationTimeout = userTimeout;
			}
		}
	}
}