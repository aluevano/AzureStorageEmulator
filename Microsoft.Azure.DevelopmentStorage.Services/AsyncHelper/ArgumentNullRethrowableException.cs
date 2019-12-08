using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[ExceptionCloner(typeof(ArgumentNullException))]
	[Serializable]
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	public class ArgumentNullRethrowableException : ArgumentNullException, IRethrowableException
	{
		public ArgumentNullRethrowableException()
		{
		}

		public ArgumentNullRethrowableException(string message) : base(message)
		{
		}

		public ArgumentNullRethrowableException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ArgumentNullRethrowableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification="This method is explicitly designed to clone only a ArgumentNullException, not a general exception.")]
		public static Exception CreateRethrowableClone(ArgumentNullException ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullRethrowableException("Can only create a clone of an existing exception.");
			}
			if (ex.Message == null)
			{
				if (ex.ParamName == null)
				{
					return new ArgumentNullRethrowableException();
				}
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] paramName = new object[] { ex.ParamName };
				return new ArgumentNullRethrowableException(string.Format(invariantCulture, "'{0}' cannot be null", paramName), ex);
			}
			if (ex.ParamName == null)
			{
				return new ArgumentNullRethrowableException(ex.Message, ex);
			}
			CultureInfo cultureInfo = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { ex.ParamName };
			return new ArgumentNullRethrowableException(string.Format(cultureInfo, "'{0}' cannot be null", objArray), ex);
		}

		public Exception GetRethrowableClone()
		{
			return ArgumentNullRethrowableException.CreateRethrowableClone(this);
		}
	}
}