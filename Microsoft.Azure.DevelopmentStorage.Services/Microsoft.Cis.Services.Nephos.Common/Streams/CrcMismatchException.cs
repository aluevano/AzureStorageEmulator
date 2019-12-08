using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default construcor taking no arguments is explicitly omitted since the specified CRC and calculated CRC are required.")]
	public class CrcMismatchException : Exception, IRethrowableException
	{
		public long CalculatedCrc
		{
			get;
			private set;
		}

		public bool IsCriticalError
		{
			get;
			private set;
		}

		public long SpecifiedCrc
		{
			get;
			private set;
		}

		public CrcMismatchException(string message) : base(message)
		{
		}

		public CrcMismatchException(string message, long specifiedCrc, long calculatedCrc, bool isCriticalError = true) : this(message, specifiedCrc, calculatedCrc, isCriticalError, null)
		{
		}

		public CrcMismatchException(string message, long specifiedCrc, long calculatedCrc, bool isCriticalError, Exception innerException) : base(message, innerException)
		{
			this.SpecifiedCrc = specifiedCrc;
			this.CalculatedCrc = calculatedCrc;
			this.IsCriticalError = isCriticalError;
			if (isCriticalError)
			{
				TimeSpan? nullable = null;
				AlertsManager.AlertOrLogException(string.Format("Hit CrcMismatchException with message:{0} SpecifiedCrc:{1} CalculatedCrc:{2}", message, specifiedCrc, calculatedCrc), message, nullable);
				return;
			}
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] objArray = new object[] { message, specifiedCrc, calculatedCrc };
			error.Log("Hit CrcMismatchException with message:{0} SpecifiedCrc:{1} CalculatedCrc:{2}", objArray);
		}

		protected CrcMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.SpecifiedCrc = (long)info.GetValue("this.SpecifiedCrc", typeof(long));
			this.CalculatedCrc = (long)info.GetValue("this.CalculatedCrc", typeof(long));
			this.IsCriticalError = (bool)info.GetValue("this.IsCriticalError", typeof(bool));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.SpecifiedCrc", this.SpecifiedCrc);
			info.AddValue("this.CalculatedCrc", this.CalculatedCrc);
			info.AddValue("this.IsCriticalError", this.IsCriticalError);
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new CrcMismatchException(this.Message, this.SpecifiedCrc, this.CalculatedCrc, this.IsCriticalError, this);
		}
	}
}