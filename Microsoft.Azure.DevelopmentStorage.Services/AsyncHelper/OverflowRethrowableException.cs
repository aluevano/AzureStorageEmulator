using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(OverflowException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class OverflowRethrowableException : OverflowException, IRethrowableException
	{
		public OverflowRethrowableException()
		{
		}

		public OverflowRethrowableException(string message) : base(message)
		{
		}

		public OverflowRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected OverflowRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a OverflowException, not a general exception.")]
		public static Exception CreateRethrowableClone(OverflowException ex)
		{
			if (ex == null)
			{
				throw new OverflowRethrowableException("Can only create a clone of an existing exception.");
			}
			return new OverflowRethrowableException(ex.Message, ex);
		}

		public Exception GetRethrowableClone()
		{
			return OverflowRethrowableException.CreateRethrowableClone(this);
		}
	}
}