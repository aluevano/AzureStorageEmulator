using AsyncHelper;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class NephosAssertionException : Exception, IRethrowableException
	{
		private const string defaultExceptionMessage = "An unexpected error has occurred.";

		public string AlertMessage
		{
			get;
			private set;
		}

		public NephosAssertionException() : base("An unexpected error has occurred.")
		{
			this.AlertMessage = "An unexpected error has occurred.";
		}

		public NephosAssertionException(string message) : base(message)
		{
			this.AlertMessage = message;
		}

		public NephosAssertionException(string message, Exception innerException) : base(message, innerException)
		{
			this.AlertMessage = message;
		}

		public NephosAssertionException(string format, params object[] args) : base(string.Format(CultureInfo.InvariantCulture, format, args))
		{
			this.AlertMessage = format;
		}

		public NephosAssertionException(Exception innerException, string format, params object[] args) : base(string.Format(CultureInfo.InvariantCulture, format, args), innerException)
		{
			this.AlertMessage = format;
		}

		protected NephosAssertionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.AlertMessage = info.GetString("this.AlertMessage");
		}

		public static void Assert(bool condition)
		{
			if (!condition)
			{
				NephosAssertionException.Fail();
			}
		}

		public static void Assert(bool condition, string message)
		{
			NephosAssertionException.Assert(condition, message, (Exception)null);
		}

		public static void Assert(bool condition, string message, Exception innerException)
		{
			if (!condition)
			{
				NephosAssertionException.Fail(message, innerException);
			}
		}

		public static void Assert(bool condition, string format, params object[] args)
		{
			if (!condition)
			{
				NephosAssertionException.Fail(format, args);
			}
		}

		public static void Assert(bool condition, string format, Func<string, string> preFormatFunc, string preFormatArg)
		{
			if (!condition)
			{
				object[] objArray = new object[] { preFormatFunc(preFormatArg) };
				NephosAssertionException.Fail(format, objArray);
			}
		}

		public static void Assert(bool condition, string format, Func<string, string, string> preFormatFunc, string preFormatArg1, string preFormatArg2)
		{
			if (!condition)
			{
				object[] objArray = new object[] { preFormatFunc(preFormatArg1, preFormatArg2) };
				NephosAssertionException.Fail(format, objArray);
			}
		}

		public static void Assert(bool condition, string format, Func<string, string, string, string> preFormatFunc, string preFormatArg1, string preFormatArg2, string preFormatArg3)
		{
			if (!condition)
			{
				object[] objArray = new object[] { preFormatFunc(preFormatArg1, preFormatArg2, preFormatArg3) };
				NephosAssertionException.Fail(format, objArray);
			}
		}

		public static void Fail()
		{
			throw new NephosAssertionException();
		}

		public static void Fail(string message)
		{
			throw new NephosAssertionException(message);
		}

		public static void Fail(string message, Exception innerException)
		{
			throw new NephosAssertionException(message, innerException);
		}

		public static void Fail(string format, params object[] args)
		{
			throw new NephosAssertionException(format, args);
		}

		public static void Fail(Exception innerException, string format, params object[] args)
		{
			throw new NephosAssertionException(innerException, format, args);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.AlertMessage", this.AlertMessage);
			base.GetObjectData(info, context);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="Rethrowable", Justification="The rethrowable clone terminology is used throughout our source tree.")]
		public Exception GetRethrowableClone()
		{
			return new NephosAssertionException(this.Message, this);
		}
	}
}