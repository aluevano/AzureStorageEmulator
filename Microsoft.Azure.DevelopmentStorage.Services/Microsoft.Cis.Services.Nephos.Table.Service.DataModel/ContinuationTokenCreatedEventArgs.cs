using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.DataModel
{
	public class ContinuationTokenCreatedEventArgs : EventArgs
	{
		public Dictionary<string, string> ContinuationToken
		{
			get;
			private set;
		}

		public ContinuationTokenCreatedEventArgs(Dictionary<string, string> continuationToken)
		{
			this.ContinuationToken = continuationToken;
		}
	}
}