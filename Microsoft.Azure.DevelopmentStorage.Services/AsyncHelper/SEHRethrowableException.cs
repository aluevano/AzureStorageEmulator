using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(SEHException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class SEHRethrowableException : SEHException, IRethrowableException
	{
		public SEHRethrowableException()
		{
		}

		public SEHRethrowableException(string message) : base(message)
		{
		}

		public SEHRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SEHRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a SEHException, not a general exception.")]
		public static Exception CreateRethrowableClone(SEHException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			return new SEHRethrowableException(ex.Message, ex);
		}

		public Exception GetRethrowableClone()
		{
			return SEHRethrowableException.CreateRethrowableClone(this);
		}
	}
}