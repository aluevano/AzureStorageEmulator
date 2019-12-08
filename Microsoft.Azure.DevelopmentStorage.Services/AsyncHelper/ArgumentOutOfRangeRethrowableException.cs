using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(ArgumentOutOfRangeException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class ArgumentOutOfRangeRethrowableException : ArgumentOutOfRangeException, IRethrowableException
	{
		public ArgumentOutOfRangeRethrowableException()
		{
		}

		public ArgumentOutOfRangeRethrowableException(string message) : base(message)
		{
		}

		public ArgumentOutOfRangeRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ArgumentOutOfRangeRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a ArgumentOutOfRangeException, not a general exception.")]
		public static Exception CreateRethrowableClone(ArgumentOutOfRangeException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			return new ArgumentOutOfRangeRethrowableException(ex.Message ?? "Specified argument was out of the range of valid values.", ex);
		}

		public Exception GetRethrowableClone()
		{
			return ArgumentOutOfRangeRethrowableException.CreateRethrowableClone(this);
		}
	}
}