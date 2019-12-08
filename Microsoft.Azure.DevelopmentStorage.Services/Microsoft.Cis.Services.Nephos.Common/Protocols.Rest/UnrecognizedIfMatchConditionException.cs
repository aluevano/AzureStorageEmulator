using AsyncHelper;
using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class UnrecognizedIfMatchConditionException : Exception, IRethrowableException
	{
		private bool? isForSource;

		public NephosStatusEntry StatusEntry
		{
			get
			{
				if (!this.isForSource.HasValue)
				{
					return CommonStatusEntries.DefaultConditionNotMet;
				}
				if (this.isForSource.Value)
				{
					return CommonStatusEntries.SourceConditionNotMet;
				}
				return CommonStatusEntries.TargetConditionNotMet;
			}
		}

		public UnrecognizedIfMatchConditionException(bool? isForSource, Exception innerException) : base("Could not recogonize the If-Match ETag", innerException)
		{
			this.isForSource = isForSource;
		}

		protected UnrecognizedIfMatchConditionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.isForSource = (bool?)info.GetValue("this.isForSource", typeof(bool?));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.isForSource", this.isForSource, typeof(bool?));
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new UnrecognizedIfMatchConditionException(this.isForSource, this);
		}
	}
}