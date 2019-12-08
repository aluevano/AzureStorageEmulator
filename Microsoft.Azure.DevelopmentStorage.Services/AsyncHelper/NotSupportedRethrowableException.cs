using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(NotSupportedException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class NotSupportedRethrowableException : NotSupportedException, IRethrowableException
	{
		public NotSupportedRethrowableException()
		{
		}

		public NotSupportedRethrowableException(string message) : base(message)
		{
		}

		public NotSupportedRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NotSupportedRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a NotSupportedException, not a general exception.")]
		public static Exception CreateRethrowableClone(NotSupportedException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			return new NotSupportedRethrowableException(ex.Message ?? "Specified method is not supported.", ex);
		}

		public Exception GetRethrowableClone()
		{
			return NotSupportedRethrowableException.CreateRethrowableClone(this);
		}
	}
}