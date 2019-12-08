using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(ArgumentException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class ArgumentRethrowableException : ArgumentException, IRethrowableException
	{
		public ArgumentRethrowableException()
		{
		}

		public ArgumentRethrowableException(string message) : base(message)
		{
		}

		public ArgumentRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public ArgumentRethrowableException(string message, string paramName) : base(message, paramName)
		{
		}

		public ArgumentRethrowableException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
		{
		}

		protected ArgumentRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a ArgumentException, not a general exception.")]
		public static Exception CreateRethrowableClone(ArgumentException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			if (ex.Message != null)
			{
				if (ex.ParamName == null)
				{
					return new ArgumentRethrowableException(ex.Message, ex);
				}
				return new ArgumentRethrowableException(ex.Message, ex.ParamName, ex);
			}
			string str = "Value does not fall within the expected range.";
			if (ex.ParamName == null)
			{
				return new ArgumentRethrowableException(str, ex);
			}
			return new ArgumentRethrowableException(str, ex.ParamName, ex);
		}

		public Exception GetRethrowableClone()
		{
			return ArgumentRethrowableException.CreateRethrowableClone(this);
		}
	}
}