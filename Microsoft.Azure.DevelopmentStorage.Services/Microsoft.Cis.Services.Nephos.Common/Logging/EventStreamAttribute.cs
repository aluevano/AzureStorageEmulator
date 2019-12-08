using System;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class EventStreamAttribute : Attribute
	{
		private string eventStreamName;

		public string EventStreamName
		{
			get
			{
				return this.eventStreamName;
			}
		}

		public EventStreamAttribute()
		{
		}

		public EventStreamAttribute(string eventStreamName)
		{
			this.eventStreamName = eventStreamName;
		}
	}
}