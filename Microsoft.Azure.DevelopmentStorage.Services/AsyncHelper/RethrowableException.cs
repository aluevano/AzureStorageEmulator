using System;
using System.Diagnostics.CodeAnalysis;

namespace AsyncHelper
{
	[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable")]
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification="This is a static class, so it's not trying to imitate an exception when it's not really one.")]
	public static class RethrowableException
	{
		public static bool TryClone(Exception exception, out Exception clone)
		{
			if (exception == null)
			{
				throw new ArgumentNullException("exception");
			}
			clone = null;
			IRethrowableException rethrowableException = exception as IRethrowableException;
			if (rethrowableException == null)
			{
				return false;
			}
			clone = rethrowableException.GetRethrowableClone();
			return true;
		}
	}
}