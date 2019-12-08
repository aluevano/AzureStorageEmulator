using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the message indicating what was invalid about the XML must be provided.")]
	public class InvalidXmlProtocolException : ProtocolException
	{
		private string reason;

		private int lineNumber;

		private int linePosition;

		public int LineNumber
		{
			get
			{
				return this.lineNumber;
			}
		}

		public int LinePosition
		{
			get
			{
				return this.linePosition;
			}
		}

		public string Reason
		{
			get
			{
				return this.reason;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.InvalidXmlDocument;
			}
		}

		public InvalidXmlProtocolException(string reason, int lineNumber, int linePosition) : this(reason, lineNumber, linePosition, null)
		{
		}

		public InvalidXmlProtocolException(string reason, int lineNumber, int linePosition, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "{0}. Line info: ({1}, {2}).", new object[] { reason, lineNumber, linePosition }), innerException)
		{
			this.reason = reason;
			this.lineNumber = lineNumber;
			this.linePosition = linePosition;
		}

		public InvalidXmlProtocolException(string message) : base(message)
		{
		}

		public InvalidXmlProtocolException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidXmlProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.reason = info.GetString("this.reason");
			this.lineNumber = info.GetInt32("this.lineNumber");
			this.linePosition = info.GetInt32("this.linePosition");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(3);
			int lineNumber = this.LineNumber;
			nameValueCollection.Add("LineNumber", lineNumber.ToString(CultureInfo.InvariantCulture));
			int linePosition = this.LinePosition;
			nameValueCollection.Add("LinePosition", linePosition.ToString(CultureInfo.InvariantCulture));
			nameValueCollection.Add("Reason", this.Reason);
			return nameValueCollection;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.reason", this.reason);
			info.AddValue("this.lineNumber", this.lineNumber);
			info.AddValue("this.linePosition", this.linePosition);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidXmlProtocolException(this.Reason, this.LineNumber, this.LinePosition, this);
		}
	}
}