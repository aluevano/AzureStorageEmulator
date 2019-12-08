using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class DeprecatedResourceNameException : StorageManagerException
	{
		public string DeprecatedResourceName
		{
			get;
			set;
		}

		public string NewResourceName
		{
			get;
			set;
		}

		public DeprecatedResourceNameException() : base("The resource name specified has been deprecated for the specified request version.")
		{
		}

		public DeprecatedResourceNameException(string message, string deprecatedResourceName, string newResourceName) : base(message)
		{
			this.NewResourceName = newResourceName;
			this.DeprecatedResourceName = deprecatedResourceName;
		}

		public DeprecatedResourceNameException(string message) : base(message)
		{
		}

		public DeprecatedResourceNameException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected DeprecatedResourceNameException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.DeprecatedResourceName = info.GetString("DeprecatedResourceName");
			this.NewResourceName = info.GetString("NewResourceName");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("DeprecatedResourceName", this.DeprecatedResourceName, typeof(string));
			info.AddValue("NewResourceName", this.NewResourceName, typeof(string));
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new DeprecatedResourceNameException(this.Message, this);
		}
	}
}