using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the resource name and value must be provided.")]
	public class InvalidCharacterProtocolException : ProtocolException
	{
		private string resourceName;

		private string resourceValue;

		private string additionalDetails;

		private string userMessage;

		public string AdditionalDetails
		{
			get
			{
				return this.additionalDetails;
			}
		}

		public string ResourceName
		{
			get
			{
				return this.resourceName;
			}
		}

		public string ResourceValue
		{
			get
			{
				return this.resourceValue;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return new NephosStatusEntry("InvalidResourceName", HttpStatusCode.BadRequest, this.UserMessage);
			}
		}

		public string UserMessage
		{
			get
			{
				return this.userMessage;
			}
		}

		public InvalidCharacterProtocolException(string resourceName, string resourceValue, string message) : base(null)
		{
			this.resourceName = resourceName;
			this.resourceValue = resourceValue;
			this.additionalDetails = message;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { resourceValue, resourceName };
			this.userMessage = string.Format(invariantCulture, "The value {0} provided for {1} contains invalid XML character and needs to be changed.", objArray);
		}

		protected InvalidCharacterProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.resourceName = info.GetString("this.resourceName");
			this.resourceValue = info.GetString("this.resourceValue");
			this.additionalDetails = info.GetString("this.additionalDetails");
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { this.resourceValue, this.resourceName };
			this.userMessage = string.Format(invariantCulture, "The value {0} provided for {1} contains invalid character and needs to be changed.", objArray);
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(2)
			{
				{ "ResourceName", this.ResourceName },
				{ "ResourceValue", this.ResourceValue },
				{ "AdditionalDetails", this.AdditionalDetails }
			};
			return nameValueCollection;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.resourceName", this.resourceName);
			info.AddValue("this.resourceValue", this.resourceValue);
			info.AddValue("this.additionalDetails", this.additionalDetails);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidCharacterProtocolException(this.ResourceName, this.ResourceValue, this.AdditionalDetails);
		}
	}
}