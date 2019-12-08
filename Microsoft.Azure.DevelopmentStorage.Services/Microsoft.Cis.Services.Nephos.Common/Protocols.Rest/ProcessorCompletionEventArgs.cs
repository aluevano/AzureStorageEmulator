using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class ProcessorCompletionEventArgs : EventArgs
	{
		private TimeSpan duration;

		private bool success;

		public TimeSpan Duration
		{
			get
			{
				return this.duration;
			}
		}

		public bool IsSuccess
		{
			get
			{
				return this.success;
			}
		}

		public ProcessorCompletionEventArgs(TimeSpan duration, bool success)
		{
			this.duration = duration;
			this.success = success;
		}
	}
}