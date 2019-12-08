using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(InvalidOperationException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class InvalidOperationRethrowableException : InvalidOperationException, IRethrowableException
	{
		public InvalidOperationRethrowableException()
		{
		}

		public InvalidOperationRethrowableException(string message) : base(message)
		{
		}

		public InvalidOperationRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidOperationRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a InvalidOperationException, not a general exception.")]
		public static Exception CreateRethrowableClone(InvalidOperationException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			return new InvalidOperationRethrowableException(ex.Message ?? "Operation is not valid due to the current state of the object.", ex);
		}

		public Exception GetRethrowableClone()
		{
			return InvalidOperationRethrowableException.CreateRethrowableClone(this);
		}
	}
}