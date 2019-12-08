using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(NullReferenceException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class NullReferenceRethrowableException : NullReferenceException, IRethrowableException
	{
		public NullReferenceRethrowableException()
		{
		}

		public NullReferenceRethrowableException(string message) : base(message)
		{
		}

		public NullReferenceRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NullReferenceRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a NullReferenceException, not a general exception.")]
		public static Exception CreateRethrowableClone(NullReferenceException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex", "Can only create a clone of an existing exception.");
			}
			return new NullReferenceRethrowableException(ex.Message ?? "Object reference not set to an instance of an object", ex);
		}

		public Exception GetRethrowableClone()
		{
			return NullReferenceRethrowableException.CreateRethrowableClone(this);
		}
	}
}