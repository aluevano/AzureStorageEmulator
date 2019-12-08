using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the metadata value must be provided.")]
	public class EmptyMetadataKeyProtocolException : ProtocolException
	{
		private string metadataValue;

		public string MetadataValue
		{
			get
			{
				return this.metadataValue;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.EmptyMetadataKey;
			}
		}

		public EmptyMetadataKeyProtocolException(string metadataValue) : this(metadataValue, null)
		{
		}

		public EmptyMetadataKeyProtocolException(string metadataValue, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "Metadata key name cannot be empty. Key name for the value {0} is empty.", new object[] { metadataValue }), innerException)
		{
			this.metadataValue = metadataValue;
		}

		protected EmptyMetadataKeyProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.metadataValue = info.GetString("this.metadataValue");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "MetadataValue", this.MetadataValue }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.metadataValue", this.metadataValue);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new EmptyMetadataKeyProtocolException(this.MetadataValue, this);
		}
	}
}