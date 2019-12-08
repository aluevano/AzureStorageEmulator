using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class PushedMessage
	{
		public byte[] MessageText
		{
			get;
			set;
		}

		public TimeSpan? MessageTTL
		{
			get;
			set;
		}

		public TimeSpan VisibilityTimeout
		{
			get;
			set;
		}

		public PushedMessage(byte[] messageText, TimeSpan visibilityTimeout, TimeSpan? messageTTL)
		{
			this.MessageText = messageText;
			this.VisibilityTimeout = visibilityTimeout;
			this.MessageTTL = messageTTL;
		}
	}
}