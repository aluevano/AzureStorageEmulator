using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(NotImplementedException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class NotImplementedRethrowableException : NotImplementedException, IRethrowableException
	{
		public NotImplementedRethrowableException()
		{
		}

		public NotImplementedRethrowableException(string message) : base(message)
		{
		}

		public NotImplementedRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NotImplementedRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a NotImplementedException, not a general exception.")]
		public static Exception CreateRethrowableClone(NotImplementedException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			return new NotImplementedRethrowableException(ex.Message ?? "The method or operation is not implemented.", ex);
		}

		public Exception GetRethrowableClone()
		{
			return NotImplementedRethrowableException.CreateRethrowableClone(this);
		}
	}
}