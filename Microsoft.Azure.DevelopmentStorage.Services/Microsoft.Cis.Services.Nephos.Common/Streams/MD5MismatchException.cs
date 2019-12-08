using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default construcor taking no arguments is explicitly omitted since the specified MD5 and calculated MD5 are required.")]
	public class MD5MismatchException : Exception, IRethrowableException
	{
		private byte[] specifiedMD5;

		private byte[] calculatedMD5;

		public MD5MismatchException(string message, byte[] specifiedMD5, byte[] calculatedMD5) : this(message, specifiedMD5, calculatedMD5, null)
		{
		}

		public MD5MismatchException(string message, byte[] specifiedMD5, byte[] calculatedMD5, Exception innerException) : base(message, innerException)
		{
			NephosAssertionException.Assert(specifiedMD5 != null);
			NephosAssertionException.Assert(calculatedMD5 != null);
			this.specifiedMD5 = specifiedMD5;
			this.calculatedMD5 = calculatedMD5;
		}

		protected MD5MismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.specifiedMD5 = (byte[])info.GetValue("this.specifiedMD5", typeof(byte[]));
			this.calculatedMD5 = (byte[])info.GetValue("this.calculatedMD5", typeof(byte[]));
		}

		public byte[] GetCalculatedMD5()
		{
			return (byte[])this.calculatedMD5.Clone();
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.specifiedMD5", this.specifiedMD5);
			info.AddValue("this.calculatedMD5", this.calculatedMD5);
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new MD5MismatchException(this.Message, this.specifiedMD5, this.calculatedMD5, this);
		}

		public byte[] GetSpecifiedMD5()
		{
			return (byte[])this.specifiedMD5.Clone();
		}
	}
}