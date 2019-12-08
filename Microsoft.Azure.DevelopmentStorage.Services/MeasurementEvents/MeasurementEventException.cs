using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace MeasurementEvents
{
	[Serializable]
	public class MeasurementEventException : Exception, IRethrowableException
	{
		public MeasurementEventException()
		{
		}

		public MeasurementEventException(string message) : base(message)
		{
		}

		public MeasurementEventException(string message, Exception innerException) : base(message)
		{
		}

		protected MeasurementEventException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new MeasurementEventException(this.Message, this);
		}
	}
}