using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;

namespace AsyncHelper
{
	[Serializable]
	public class RethrowableWrapperException : Exception, IRethrowableException
	{
		private RethrowableWrapperException(Exception ex) : base(ex.Message, ex)
		{
		}

		private RethrowableWrapperException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected RethrowableWrapperException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			throw new RethrowableWrapperException(this);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
		internal static Exception MakeRethrowable(Exception ex)
		{
			if (ex is IRethrowableException)
			{
				return ex;
			}
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] fullName = new object[] { ex.GetType().FullName, ex.Message };
			return new RethrowableWrapperException(string.Format(invariantCulture, "An exception {0} was thrown which does not implement IRethrowableException: {1}", fullName), ex);
		}
	}
}