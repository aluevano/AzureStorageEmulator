using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ConditionNotMetException : StorageManagerException
	{
		public System.Net.HttpStatusCode? HttpStatusCode
		{
			get;
			set;
		}

		public bool? IsForSource
		{
			get;
			private set;
		}

		public string UserCondition
		{
			get;
			set;
		}

		public ConditionNotMetException() : base("The condition is not met.")
		{
		}

		public ConditionNotMetException(string message) : base(message)
		{
		}

		public ConditionNotMetException(string message, bool? isForSource, Exception innerException) : base(message, innerException)
		{
			this.IsForSource = isForSource;
		}

		public ConditionNotMetException(string message, bool? isForSource, Exception innerException, System.Net.HttpStatusCode? httpStatusCode) : this(message, isForSource, innerException)
		{
			this.HttpStatusCode = httpStatusCode;
		}

		protected ConditionNotMetException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.IsForSource = (bool?)info.GetValue("this.IsForSource", typeof(bool?));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.IsForSource", this.IsForSource, typeof(bool?));
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new ConditionNotMetException(this.Message, this.IsForSource, this, this.HttpStatusCode);
		}
	}
}