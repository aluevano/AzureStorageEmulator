using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(TimeoutException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class TimeoutRethrowableException : TimeoutException, IRethrowableException
	{
		public TimeoutRethrowableException()
		{
		}

		public TimeoutRethrowableException(string message) : base(message)
		{
		}

		public TimeoutRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected TimeoutRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a TimeoutException, not a general exception.")]
		public static Exception CreateRethrowableClone(TimeoutException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			return new TimeoutRethrowableException(ex.Message ?? "The operation has timed out.", ex);
		}

		public Exception GetRethrowableClone()
		{
			return TimeoutRethrowableException.CreateRethrowableClone(this);
		}
	}
}