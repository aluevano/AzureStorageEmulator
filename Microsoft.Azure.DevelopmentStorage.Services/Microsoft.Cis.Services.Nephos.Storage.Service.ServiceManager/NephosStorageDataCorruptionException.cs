using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	[Serializable]
	public class NephosStorageDataCorruptionException : NephosStorageException
	{
		public static DateTime LastAlertTime;

		public static TimeSpan AlertInterval;

		static NephosStorageDataCorruptionException()
		{
			NephosStorageDataCorruptionException.LastAlertTime = DateTime.MinValue;
			NephosStorageDataCorruptionException.AlertInterval = TimeSpan.FromHours(1);
		}

		public NephosStorageDataCorruptionException() : base("Data corruption was detected")
		{
		}

		public NephosStorageDataCorruptionException(string message) : base(message)
		{
		}

		public NephosStorageDataCorruptionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NephosStorageDataCorruptionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new NephosStorageDataCorruptionException(this.Message, this);
		}

		public static void RaiseAlert(NephosStorageDataCorruptionException exception)
		{
			if (DateTime.UtcNow.Subtract(NephosStorageDataCorruptionException.AlertInterval) <= NephosStorageDataCorruptionException.LastAlertTime)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log(exception.Message);
				return;
			}
			NephosStorageDataCorruptionException.LastAlertTime = DateTime.UtcNow;
			Logger<IRestProtocolHeadLogger>.Instance.Critical.Log(exception.Message);
		}
	}
}