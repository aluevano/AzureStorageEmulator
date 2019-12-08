using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public sealed class BlobObjectHelper
	{
		private readonly static TimeSpan DEFAULT_MIN_PUT_BLOB_TIMEOUT;

		private readonly static TimeSpan DEFAULT_MIN_GET_BLOB_TIMEOUT;

		private static TimeSpan s_minGetBlobTimeout;

		private static TimeSpan s_minPutBlobTimeout;

		private readonly static long s_xDriveRequestHighPriMaxLength;

		public static long GetDataRateForTimeout
		{
			get
			{
				return ServiceConstants.DefaultGetDataRateForTimeout;
			}
		}

		public static TimeSpan MinGetBlobTimeout
		{
			get
			{
				return BlobObjectHelper.s_minGetBlobTimeout;
			}
			set
			{
				BlobObjectHelper.s_minGetBlobTimeout = value;
			}
		}

		public static TimeSpan MinPutBlobTimeout
		{
			get
			{
				return BlobObjectHelper.s_minPutBlobTimeout;
			}
		}

		public static long PutDataRateForTimeout
		{
			get
			{
				return ServiceConstants.DefaultPutDataRateForTimeout;
			}
		}

		public static long XDriveRequestHighPriMaxLength
		{
			get
			{
				return BlobObjectHelper.s_xDriveRequestHighPriMaxLength;
			}
		}

		static BlobObjectHelper()
		{
			BlobObjectHelper.DEFAULT_MIN_PUT_BLOB_TIMEOUT = TimeSpan.FromMinutes(2);
			BlobObjectHelper.DEFAULT_MIN_GET_BLOB_TIMEOUT = TimeSpan.FromMinutes(2);
			BlobObjectHelper.s_minGetBlobTimeout = BlobObjectHelper.DEFAULT_MIN_GET_BLOB_TIMEOUT;
			BlobObjectHelper.s_minPutBlobTimeout = BlobObjectHelper.DEFAULT_MIN_PUT_BLOB_TIMEOUT;
			BlobObjectHelper.s_xDriveRequestHighPriMaxLength = (long)262144;
		}

		public BlobObjectHelper()
		{
		}

		public static TimeSpan GetSizeBasedTimeout(long contentLength, TimeSpan userTimeout, TimeSpan minimumTimeout, long dataRateForTimeout)
		{
			NephosAssertionException.Assert(dataRateForTimeout > (long)0);
			int num = (int)Math.Ceiling((double)contentLength / (double)dataRateForTimeout);
			TimeSpan timeSpan = TimeSpan.FromMinutes((double)Math.Max((int)minimumTimeout.TotalMinutes, num));
			if (userTimeout > timeSpan)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] totalSeconds = new object[] { (long)userTimeout.TotalSeconds, (long)timeSpan.TotalSeconds, contentLength, dataRateForTimeout };
				verbose.Log("SecurityWarning: Reducing user timeout from {0} seconds to {1} seconds based on size {2} bytes and a rate of {3} bytes per minute.", totalSeconds);
			}
			else
			{
				timeSpan = userTimeout;
			}
			IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] objArray = new object[] { timeSpan.TotalSeconds, contentLength, minimumTimeout.TotalSeconds, userTimeout.TotalSeconds, dataRateForTimeout };
			stringDataEventStream.Log("GetSizeBasedTimeout (seconds) >> Returning timeout '{0}' based on contentLength {1},  minimumTimeout '{2}', user timeout '{3}' and a rate of {4} bytes per minute.", objArray);
			return timeSpan;
		}

		public static TimeSpan GetSizeBasedTimeout(long dataSize, long dataRateForTimeout)
		{
			NephosAssertionException.Assert(dataRateForTimeout > (long)0);
			double num = (double)dataRateForTimeout / 60000;
			int num1 = (int)Math.Ceiling((double)dataSize / num);
			return TimeSpan.FromMilliseconds((double)num1);
		}
	}
}