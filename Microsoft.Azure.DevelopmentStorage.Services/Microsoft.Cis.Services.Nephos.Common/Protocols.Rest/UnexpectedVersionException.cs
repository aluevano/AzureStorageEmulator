using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the version must be provided.")]
	public class UnexpectedVersionException : Exception, IRethrowableException
	{
		public static DateTime LastAlertTime;

		public static TimeSpan AlertInterval;

		public bool Critical
		{
			get;
			private set;
		}

		public string Version
		{
			get;
			private set;
		}

		static UnexpectedVersionException()
		{
			UnexpectedVersionException.LastAlertTime = DateTime.MinValue;
			UnexpectedVersionException.AlertInterval = TimeSpan.FromHours(1);
		}

		public UnexpectedVersionException(string message, string version) : this(message, version, false)
		{
		}

		public UnexpectedVersionException(string message, string version, bool critical) : this(message, version, critical, null)
		{
		}

		public UnexpectedVersionException(string message, string version, bool critical, Exception innerException) : base(message, innerException)
		{
			this.Version = version;
			this.Critical = critical;
		}

		protected UnexpectedVersionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.Version = info.GetString("this.Version");
			this.Critical = info.GetBoolean("this.Critical");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.Version", this.Version);
			info.AddValue("this.Critical", this.Critical);
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new UnexpectedVersionException(this.Version, this.Message, this.Critical, this);
		}

		public static void RaiseAlert(UnexpectedVersionException exception)
		{
			if (DateTime.UtcNow.Subtract(UnexpectedVersionException.AlertInterval) <= UnexpectedVersionException.LastAlertTime)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log(exception.Message);
				return;
			}
			UnexpectedVersionException.LastAlertTime = DateTime.UtcNow;
			Logger<IRestProtocolHeadLogger>.Instance.Critical.Log(exception.Message);
		}
	}
}