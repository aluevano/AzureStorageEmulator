using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class OperationStatus
	{
		private long totalTableServerRoundTripCount;

		private long totalXStreamTimeInMicroSeconds;

		private long totalXStreamLength;

		private long totalXStreamRoundTripCount;

		private long totalXStreamXCacheReadTimeInMicroSeconds;

		private long totalXStreamXCacheReadRoundTripCount;

		private long totalPartitionWaitTimeInMs;

		private long totalTableServerTimeInMicroSeconds;

		private long totalAccountCacheWaitTimeInMs;

		private long totalContainerCacheWaitTimeInMs;

		private long totalCustomEncryptionInfoCacheWaitTimeInMs;

		private long totalFETimeInMicroSeconds;

		private long totalXCacheTimeInMs;

		private long totalXCacheRoundTripCount;

		private IPerfTimer tsTimeTrackingStopWatch;

		public long LastTableServerErrorCode
		{
			get;
			set;
		}

		public string LastTableServerInstanceName
		{
			get;
			set;
		}

		public string LastTableServerPartitionName
		{
			get;
			set;
		}

		public long LastXStreamErrorCode
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get;
			set;
		}

		public long TotalAccountCacheWaitTimeInMs
		{
			get
			{
				return this.totalAccountCacheWaitTimeInMs;
			}
		}

		public long TotalContainerCacheWaitTimeInMs
		{
			get
			{
				return this.totalContainerCacheWaitTimeInMs;
			}
		}

		public long TotalCustomEncryptionInfoCacheWaitTimeInMs
		{
			get
			{
				return this.totalCustomEncryptionInfoCacheWaitTimeInMs;
			}
		}

		public long TotalFETimeInMicroSeconds
		{
			get
			{
				return this.totalFETimeInMicroSeconds;
			}
		}

		public long TotalPartitionWaitTimeInMs
		{
			get
			{
				return this.totalPartitionWaitTimeInMs;
			}
		}

		public long TotalTableServerRoundTripCount
		{
			get
			{
				return this.totalTableServerRoundTripCount;
			}
		}

		public long TotalTableServerTimeInMicroSeconds
		{
			get
			{
				long num = this.TotalTrackedTableServerTimeInMicroSeconds();
				if (false && num != (long)0)
				{
					return num;
				}
				return this.totalTableServerTimeInMicroSeconds;
			}
		}

		public long TotalXCacheRoundTripCount
		{
			get
			{
				return this.totalXCacheRoundTripCount;
			}
		}

		public long TotalXCacheTimeInMs
		{
			get
			{
				return this.totalXCacheTimeInMs;
			}
		}

		public long TotalXStreamLength
		{
			get
			{
				return this.totalXStreamLength;
			}
		}

		public long TotalXStreamRoundTripCount
		{
			get
			{
				return this.totalXStreamRoundTripCount;
			}
		}

		public long TotalXStreamTimeInMicroSeconds
		{
			get
			{
				return this.totalXStreamTimeInMicroSeconds;
			}
		}

		public long TotalXStreamTimeInMs
		{
			get
			{
				return this.totalXStreamTimeInMicroSeconds / (long)1000;
			}
		}

		public long TotalXStreamXCacheReadRoundTripCount
		{
			get
			{
				return this.totalXStreamXCacheReadRoundTripCount;
			}
		}

		public long TotalXStreamXCacheReadTimeInMicroSeconds
		{
			get
			{
				return this.totalXStreamXCacheReadTimeInMicroSeconds;
			}
		}

		public long TotalXStreamXCacheReadTimeInMs
		{
			get
			{
				return this.totalXStreamXCacheReadTimeInMicroSeconds / (long)1000;
			}
		}

		public OperationStatus()
		{
			bool flag = false;
			this.tsTimeTrackingStopWatch = PerfTimerFactory.CreateTimer(flag, new TimeSpan((long)0));
		}

		public void IncreaseAccountCacheWaitTimeInMs(long accountCacheWaitTimeInMs)
		{
			Interlocked.Add(ref this.totalAccountCacheWaitTimeInMs, accountCacheWaitTimeInMs);
		}

		public void IncreaseContainerCacheWaitTimeInMs(long containerCacheWaitTimeInMs)
		{
			Interlocked.Add(ref this.totalContainerCacheWaitTimeInMs, containerCacheWaitTimeInMs);
		}

		public void IncreaseCustomEncryptionInfoCacheWaitTimeInMs(long customEncryptionInfoCacheWaitTimeInMs)
		{
			Interlocked.Add(ref this.totalCustomEncryptionInfoCacheWaitTimeInMs, customEncryptionInfoCacheWaitTimeInMs);
		}

		public void IncreaseTableServerPerfCounters(long tableServerTimeInMicroSeconds, long tableServerRoundTripCount, long partitionWaitTimeInMs)
		{
			Interlocked.Add(ref this.totalTableServerTimeInMicroSeconds, tableServerTimeInMicroSeconds);
			Interlocked.Add(ref this.totalTableServerRoundTripCount, tableServerRoundTripCount);
			Interlocked.Add(ref this.totalPartitionWaitTimeInMs, partitionWaitTimeInMs);
		}

		public void IncreaseXCachePerfCounters(long totalXCacheTimeInMs, long totalXCacheRoundTripCount)
		{
			Interlocked.Add(ref this.totalXCacheTimeInMs, totalXCacheTimeInMs);
			Interlocked.Add(ref this.totalXCacheRoundTripCount, totalXCacheRoundTripCount);
		}

		public void IncreaseXStreamPerfCounters(long xStreamTimeInMicroSeconds, long xStreamLength, long xStreamRoundTripCount, bool decreaseFromTableServerTime, bool isReadStreamFromXCache)
		{
			Interlocked.Add(ref this.totalXStreamTimeInMicroSeconds, xStreamTimeInMicroSeconds);
			Interlocked.Add(ref this.totalXStreamLength, xStreamLength);
			Interlocked.Add(ref this.totalXStreamRoundTripCount, xStreamRoundTripCount);
			if (isReadStreamFromXCache)
			{
				Interlocked.Add(ref this.totalXStreamXCacheReadTimeInMicroSeconds, xStreamTimeInMicroSeconds);
				Interlocked.Add(ref this.totalXStreamXCacheReadRoundTripCount, xStreamRoundTripCount);
			}
			if (decreaseFromTableServerTime)
			{
				Interlocked.Add(ref this.totalTableServerTimeInMicroSeconds, xStreamTimeInMicroSeconds * (long)-1);
			}
		}

		public void TableServerCommandStartTimer()
		{
			lock (this.tsTimeTrackingStopWatch)
			{
				if (!this.tsTimeTrackingStopWatch.IsRunning)
				{
					this.tsTimeTrackingStopWatch.Start();
				}
			}
		}

		public void TableServerCommandStopTimer()
		{
			lock (this.tsTimeTrackingStopWatch)
			{
				if (this.tsTimeTrackingStopWatch.IsRunning)
				{
					this.tsTimeTrackingStopWatch.Stop();
				}
			}
		}

		private long TotalTrackedTableServerTimeInMicroSeconds()
		{
			return (long)this.tsTimeTrackingStopWatch.Elapsed.TotalMilliseconds * (long)1000;
		}

		public void UpdateFETimeBasedOnFinalServerProcessingTime(TimeSpan finalServerProcessingTime)
		{
			double totalMilliseconds = finalServerProcessingTime.TotalMilliseconds * 1000 - (double)this.TotalTableServerTimeInMicroSeconds - (double)this.TotalXStreamTimeInMicroSeconds - (double)((this.TotalPartitionWaitTimeInMs + this.TotalAccountCacheWaitTimeInMs + this.TotalContainerCacheWaitTimeInMs + this.TotalCustomEncryptionInfoCacheWaitTimeInMs + this.TotalXCacheTimeInMs) * (long)1000);
			if (this.TotalTrackedTableServerTimeInMicroSeconds() != (long)0)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] objArray = new object[] { this.TotalTrackedTableServerTimeInMicroSeconds(), this.totalTableServerTimeInMicroSeconds, this.TotalCustomEncryptionInfoCacheWaitTimeInMs };
				verbose.Log("TotalTrackedTableServerTimeInMicroSeconds: {0} totalTableServerTimeInMicroSeconds: {1} TotalCustomEncryptionInfoCacheWaitTimeInMs: {2}", objArray);
			}
			this.totalFETimeInMicroSeconds = (long)Math.Max(totalMilliseconds, 0);
		}
	}
}